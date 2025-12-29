using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Log;
using NimbblException = Nimbbl.Sdk.Rest.Exception.NimbblException;
using AuthenticationException = Nimbbl.Sdk.Rest.Exception.AuthenticationException;
using BadRequestException = Nimbbl.Sdk.Rest.Exception.BadRequestException;
using NotFoundException = Nimbbl.Sdk.Rest.Exception.NotFoundException;
using RateLimitException = Nimbbl.Sdk.Rest.Exception.RateLimitException;
using ServerException = Nimbbl.Sdk.Rest.Exception.ServerException;
using ApiException = Nimbbl.Sdk.Rest.Exception.ApiException;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Nimbbl.Sdk.Rest.Test")]
namespace Nimbbl.Sdk.Rest.RestClient;

internal class ApiClient : IDisposable
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly HttpClient _client;
    private readonly string _key;
    private readonly string _secret;
    private readonly string _baseUrl;
    private readonly Action<string, string>? _logAction;
        private readonly Dictionary<string, string> _defaultHeaders = new(StringComparer.OrdinalIgnoreCase);
        private string? _explicitBearerToken;
        private DateTime? _explicitTokenExpiryUtc;

    private JsonDocument? _tokenDoc;
    private readonly AuthenticationService _authenticationService;
    
    private JsonElement Token => _tokenDoc?.RootElement ?? default;
    
    // Expose config Key and Secret for Auth class to use in generate-token request
    internal string GetConfigKey() => _key;
    internal string GetConfigSecret() => _secret;
    
    public ApiClient(string key, string secret, string baseUrl, Action<string, string>? logAction = null)
    {
        _key = key;
        _secret = secret;
        _baseUrl = baseUrl;
        _logAction = logAction;
        _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            // Don't auto-convert strings to DateTime - let properties handle their own types
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        _client = new()
        {
            BaseAddress = new(baseUrl)
        };
        // Default User-Agent: SDK name/version + runtime
        _defaultHeaders["User-Agent"] = $"{SdkConstants.SdkName}/{SdkConstants.SdkVersion} .NET/{Environment.Version}";
        _authenticationService = new(_client, key, secret, _serializerOptions);
    }

    public async Task<TResponse> GetWithAuth<TResponse>(string requestUri)
    {
        return await SendWithAuthGuardAsync<TResponse>(CreateRequest(HttpMethod.Get, requestUri));
    }

    public async Task<TResponse> GetWithAuth<TResponse>(string requestUri, Dictionary<string, object?>? queryParams)
    {
        var uri = BuildQueryString(requestUri, queryParams);
        return await SendWithAuthGuardAsync<TResponse>(CreateRequest(HttpMethod.Get, uri));
    }

    public async Task<TResponse> PostWithAuth<TRequest, TResponse>(string requestUri, TRequest body)
        where TRequest : class
    {
        return await SendWithAuthGuardAsync<TResponse>(CreateRequest(HttpMethod.Post, requestUri, body));
    }

    public async Task<TResponse> PatchWithAuth<TRequest, TResponse>(string requestUri, TRequest body)
        where TRequest : class
    {
        return await SendWithAuthGuardAsync<TResponse>(CreateRequest(HttpMethod.Patch, requestUri, body));
    }

    public async Task<TResponse> DeleteWithAuth<TResponse>(string requestUri)
    {
        return await SendWithAuthGuardAsync<TResponse>(CreateRequest(HttpMethod.Delete, requestUri));
    }

    public async Task<TResponse> DeleteWithAuth<TResponse>(string requestUri, Dictionary<string, object?>? queryParams)
    {
        var uri = BuildQueryString(requestUri, queryParams);
        return await SendWithAuthGuardAsync<TResponse>(CreateRequest(HttpMethod.Delete, uri));
    }

        public void SetBearerToken(string token, DateTime? expiresAtUtc = null)
        {
            _explicitBearerToken = token;
            _explicitTokenExpiryUtc = expiresAtUtc;
        }

        public void AddHeader(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            _defaultHeaders[key] = value;
        }


    private async Task<TResponse> SendWithAuthGuardAsync<TResponse>(HttpRequestMessage message)
    {
        // Store caller info from request log to reuse for response log and error logs
        var callerInfo = await LogRequestAsync(message);
        var httpResponse = await SendResilientRequestAsync(callerInfo);
        
        // Read response body for logging (clone it first to avoid consuming)
        string? responseBody = null;
        try
        {
            if (httpResponse.Content != null)
            {
                responseBody = await httpResponse.Content.ReadAsStringAsync();
                // Recreate content from the string so it can be read again for deserialization
                httpResponse.Content = new StringContent(responseBody ?? string.Empty, System.Text.Encoding.UTF8, "application/json");
            }
        }
        catch (System.Exception ex)
        {
            var logger = Logger.GetInstance();
            logger.ExceptionWithCaller("Error reading response body", ex, callerInfo.Module, callerInfo.Line, callerInfo.Function);
            throw;
        }
        
        // Log raw response body before deserialization, reusing caller info from request
        await LogResponseAsync(httpResponse, responseBody, callerInfo);

        // If HTTP status is success but body carries an error envelope, surface it
        if (httpResponse.IsSuccessStatusCode)
        {
            ThrowIfErrorEnvelope(responseBody, httpResponse.StatusCode);
        }
        
        // Log raw JSON for debugging (before deserialization attempt)
        if (responseBody != null)
        {
            // Use Logger class for consistent format, reusing caller info from request log
            var logger = Logger.GetInstance();
            logger.DebugWithCaller($"Raw JSON Response (before deserialization):\n{responseBody}", callerInfo.Module, callerInfo.Line, callerInfo.Function);
        }
        else
        {
            // Use Logger class for consistent format, reusing caller info from request log
            var logger = Logger.GetInstance();
            logger.DebugWithCaller("Response body is NULL", callerInfo.Module, callerInfo.Line, callerInfo.Function);
        }
        
        // Attempt deserialization with error handling
        if (httpResponse.Content == null)
        {
            throw new ApplicationException(ErrorMessages.MessageResponseContentNull);
        }
        
        TResponse? response;
        try
        {
            // If TResponse is JsonElement, parse directly without deserialization
            // Note: JsonElement.Clone() creates an independent copy that doesn't require the document to stay alive
            if (typeof(TResponse) == typeof(JsonElement))
            {
                using var jsonDoc = JsonDocument.Parse(responseBody ?? "{}");
                response = (TResponse)(object)jsonDoc.RootElement.Clone();
            }
            else
            {
                response = await httpResponse.Content.ReadFromJsonAsync<TResponse>(_serializerOptions);
            }
        }
        catch (JsonException ex)
        {
            // Log the error with the raw response for debugging - ALWAYS log this
            if (_logAction != null)
            {
                try
                {
                    var errorMsg = $"Failed to deserialize response:\n{ex.Message}\nPath: {ex.Path}\nLineNumber: {ex.LineNumber}\nBytePositionInLine: {ex.BytePositionInLine}";
                    if (responseBody != null)
                    {
                        errorMsg += $"\n\nRaw JSON that failed:\n{responseBody}";
                    }
                    else
                    {
                        errorMsg += "\n\nResponse body was NULL";
                    }
                    _logAction("DESERIALIZATION_ERROR", errorMsg);
                }
                catch
                {
                    // If logging fails, use Logger as fallback
                    var logger = Logger.GetInstance();
                    logger.Exception("Failed to log deserialization error", ex);
                }
            }
            throw;
        }
        catch (System.Exception ex)
        {
            // Catch any other exceptions during deserialization
            if (_logAction != null && responseBody != null)
            {
                try
                {
                    _logAction("DESERIALIZATION_ERROR", 
                        $"Unexpected error during deserialization:\n{ex.GetType().Name}: {ex.Message}\n\nRaw JSON:\n{responseBody}");
                }
                catch
                {
                    var logger = Logger.GetInstance();
                    logger.Exception("Failed to log deserialization error", ex);
                }
            }
            throw;
        }
        
        if (response == null) throw new ApplicationException(ErrorMessages.MessageNoValueReturned);
        return response;

        async Task<HttpResponseMessage> SendResilientRequestAsync((string Module, string Function, int Line) callerInfo)
        {
            try
            {
                var result = await WithRetry(SendRequestAsync, message, 1);
                return result;
            }
            catch (System.Exception ex)
            {
                var logger = Logger.GetInstance();
                logger.ExceptionWithCaller("SendResilientRequestAsync failed", ex, callerInfo.Module, callerInfo.Line, callerInfo.Function);
                throw;
            }
        }
    }

    private async Task<HttpResponseMessage> WithRetry(Func<HttpRequestMessage, Task<HttpResponseMessage>> request, HttpRequestMessage message, ushort retryCount)
    {
        HttpResponseMessage response;
        do
        {
            response = await request(CloneRequest(message));
            if (response.IsSuccessStatusCode) 
            {
                return response;
            }
            if (IsAuthFailure(response))
            {
                _tokenDoc?.Dispose();
                _tokenDoc = null;
            }
        } while (retryCount-- != 0);
        await HandleErrorResponseAsync(response);
        return response;
    }

    private static bool IsAuthFailure(HttpResponseMessage response)
    {
        return response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized;
    }

    private async Task HandleErrorResponseAsync(HttpResponseMessage httpResponse)
    {
        var errorText = await httpResponse.Content.ReadAsStringAsync();
        
        // Log the error response (masked) - CentralMasker is ONLY used for logging
        var maskedErrorText = CentralMasker.MaskBody(errorText);
        // Use Logger class for consistent format
        var logger = Logger.GetInstance();
        logger.Error($"HTTP {httpResponse.StatusCode}: {maskedErrorText}");
        
        // Use unmasked errorText for exceptions (exceptions are not logging)
        if (!IsValidJson(errorText)) throw MapException(httpResponse.StatusCode, errorText, ErrorCodes.ServerError);
        
        try
        {
            using var errorDoc = JsonDocument.Parse(errorText);
            var errorRoot = errorDoc.RootElement;
            
            // Try to get error object
            if (errorRoot.TryGetProperty(ErrorMessages.ResponseKeyError, out var errorObj))
            {
                var merchantMessage = errorObj.TryGetProperty(ErrorMessages.ErrorKeyMerchantMessage, out var mm) ? mm.GetString() : null;
                var consumerMessage = errorObj.TryGetProperty(ErrorMessages.ErrorKeyConsumerMessage, out var cm) ? cm.GetString() : null;
                var errorCode = errorObj.TryGetProperty(ErrorMessages.ErrorKeyErrorCode, out var ec) ? ec.GetString() : null;
                
                // Use merchant message if available, otherwise consumer message, otherwise error code
                var message = merchantMessage ?? consumerMessage ?? errorCode ?? ErrorMessages.MessageApiRequestFailed;
                throw MapException(httpResponse.StatusCode, message, errorCode ?? ErrorCodes.ServerError);
            }
        }
        catch (JsonException)
        {
            // If deserialization fails, try to extract error message manually
            if (errorText.Contains(ErrorMessages.ErrorKeyMerchantMessage))
            {
                // Try to extract the message from JSON
                var startIdx = errorText.IndexOf($"\"{ErrorMessages.ErrorKeyMerchantMessage}\"");
                if (startIdx > 0)
                {
                    var valueStart = errorText.IndexOf('"', startIdx + 25) + 1;
                    var valueEnd = errorText.IndexOf('"', valueStart);
                    if (valueEnd > valueStart)
                    {
                        var message = errorText.Substring(valueStart, valueEnd - valueStart);
                        throw MapException(httpResponse.StatusCode, message, null);
                    }
                }
            }
        }
        
        // Fallback: throw with raw error text (unmasked - exceptions are not logging)
        throw MapException(httpResponse.StatusCode, errorText, ErrorCodes.ServerError);

        static bool IsValidJson(string errorText)
        {
            return errorText.Trim().StartsWith('{');
        }
    }

    private static NimbblException MapException(HttpStatusCode statusCode, string message, string? errorCode)
    {
        var code = (int)statusCode;
        var safeMessage = string.IsNullOrWhiteSpace(message) ? ErrorMessages.MessageApiRequestFailed : message;
        return statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AuthenticationException(safeMessage, code, errorCode ?? ErrorCodes.AuthError),
            HttpStatusCode.BadRequest or (HttpStatusCode)422 => new BadRequestException(safeMessage, code, errorCode ?? ErrorCodes.ServerError),
            HttpStatusCode.NotFound => new NotFoundException(safeMessage, code, errorCode ?? ErrorCodes.ServerError),
            (HttpStatusCode)429 => new RateLimitException(safeMessage, code, errorCode ?? ErrorCodes.ServerError),
            HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout
                => new ServerException(safeMessage, code, errorCode ?? ErrorCodes.ServerError),
            _ => new ApiException(safeMessage, code, errorCode ?? ErrorCodes.ServerError)
        };
    }
    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
    {
        try
        {
            await LazyAuthorizeRequestAsync(request);
            var response = await _client.SendAsync(request);
            return response;
        }
        catch (System.Exception ex)
        {
            // Capture caller info for better error logging
            var callerInfo = Logger.GetInstance().GetCallerInfo();
            var logger = Logger.GetInstance();
            logger.ExceptionWithCaller("SendRequestAsync failed", ex, callerInfo.Module, callerInfo.Line, callerInfo.Function);
            throw;
        }
    }

    private async Task LazyAuthorizeRequestAsync(HttpRequestMessage request)
    {
            // Prefer explicitly set bearer token if valid (or no expiry provided)
            if (!string.IsNullOrEmpty(_explicitBearerToken))
            {
                if (_explicitTokenExpiryUtc == null || DateTime.UtcNow < _explicitTokenExpiryUtc.Value)
                {
                    request.Headers.Authorization = new("Bearer", _explicitBearerToken);
                    return;
                }
            }

            // Check if cached token is valid (exists, has token property, and not expired)
            bool isValid = false;
            if (_tokenDoc != null && Token.ValueKind == JsonValueKind.Object)
            {
                var tokenValue = Token.TryGetProperty("token", out var tokenProp) ? tokenProp.GetString() : null;
                if (!string.IsNullOrEmpty(tokenValue))
                {
                    // Check expiration
                    if (Token.TryGetProperty("expires_at", out var expiresProp))
                    {
                        var expiresStr = expiresProp.GetString();
                        if (!string.IsNullOrEmpty(expiresStr) && DateTime.TryParse(expiresStr, out var expiresAt))
                        {
                            isValid = DateTime.UtcNow < expiresAt;
                        }
                        else
                        {
                            isValid = true; // If we can't parse, assume valid
                        }
                    }
                    else
                    {
                        isValid = true; // No expiration field, assume valid
                    }
                }
            }
            
            if (!isValid)
            {
                _tokenDoc?.Dispose();
                var tokenElement = await _authenticationService.Authenticate();
                // Create a new JsonDocument from the element's raw text
                _tokenDoc = JsonDocument.Parse(tokenElement.GetRawText());
            }
            
            var tokenString = Token.TryGetProperty("token", out var token) ? token.GetString() : null;
            if (string.IsNullOrEmpty(tokenString))
            {
            throw new ApplicationException(ErrorMessages.MessageTokenMissing);
            }
            request.Headers.Authorization = new("Bearer", tokenString);
    }



    private static string BuildQueryString(string requestUri, Dictionary<string, object?>? queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
            return requestUri;
        
        var queryParts = new List<string>();
        foreach (var kv in queryParams)
        {
            if (kv.Value != null)
            {
                var value = kv.Value.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    queryParts.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(value)}");
                }
            }
        }
        
        if (queryParts.Count == 0)
            return requestUri;
        
        var queryString = string.Join("&", queryParts);
        var separator = requestUri.Contains('?') ? "&" : "?";
        return $"{requestUri}{separator}{queryString}";
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
    {
        var normalizedPath = NormalizeRelativePath(requestUri);
        var message = new HttpRequestMessage(method, normalizedPath);
        ApplyDefaultHeaders(message);
        return message;
    }

    private HttpRequestMessage CreateRequest<TRequest>(HttpMethod method, string requestUri, TRequest body) where TRequest : class
    {
        var message = CreateRequest(method, requestUri);
        // Serialize body to string first for logging, then create content
        var bodyJson = JsonSerializer.Serialize(body, _serializerOptions);
        message.Content = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");
        return message;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage message)
    {
            var clone = new HttpRequestMessage(message.Method, message.RequestUri)
            {
                Content = message.Content
            };
            foreach (var header in message.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return clone;
    }

    /// <summary>
    /// Detects error envelope in a 2xx response and throws mapped exception.
    /// </summary>
    private void ThrowIfErrorEnvelope(string? responseBody, HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(responseBody)) return;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return;
            if (!root.TryGetProperty(ErrorMessages.ResponseKeyError, out var errorObj) || errorObj.ValueKind != JsonValueKind.Object) return;

            var merchantMessage = errorObj.TryGetProperty(ErrorMessages.ErrorKeyMerchantMessage, out var mm) ? mm.GetString() : null;
            var consumerMessage = errorObj.TryGetProperty(ErrorMessages.ErrorKeyConsumerMessage, out var cm) ? cm.GetString() : null;
            var errorCode = errorObj.TryGetProperty(ErrorMessages.ErrorKeyErrorCode, out var ec) ? ec.GetString() : null;
            var message = merchantMessage ?? consumerMessage ?? errorCode ?? ErrorMessages.MessageUnknownError;

            // Map as BadRequest when HTTP is 2xx but error payload present
            throw MapException(HttpStatusCode.BadRequest, message, errorCode);
        }
        catch (JsonException)
        {
            // If not JSON, ignore
        }
    }

    /// <summary>
    /// If base URL already contains a version suffix (e.g., .../v3), avoid double prefixing when requestUri also includes v3/.
    /// </summary>
    private string NormalizeRelativePath(string requestUri)
    {
        var trimmedBase = _client.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
        var trimmedReq = requestUri.TrimStart('/');

        // Detect version suffix in base (e.g., /v3 or /v2)
        var baseHasVersion = trimmedBase.EndsWith("/v3", StringComparison.OrdinalIgnoreCase)
                             || trimmedBase.EndsWith("/v2", StringComparison.OrdinalIgnoreCase)
                             || trimmedBase.EndsWith("/v1", StringComparison.OrdinalIgnoreCase);

        if (baseHasVersion && (trimmedReq.StartsWith("v3/", StringComparison.OrdinalIgnoreCase)
            || trimmedReq.StartsWith("v2/", StringComparison.OrdinalIgnoreCase)
            || trimmedReq.StartsWith("v1/", StringComparison.OrdinalIgnoreCase)))
        {
            // Strip leading version from request to avoid double-prefix
            var slashIndex = trimmedReq.IndexOf('/');
            if (slashIndex >= 0 && slashIndex < trimmedReq.Length - 1)
            {
                trimmedReq = trimmedReq[(slashIndex + 1)..];
            }
            else
            {
                trimmedReq = string.Empty;
            }
        }

        return trimmedReq;
    }

        private void ApplyDefaultHeaders(HttpRequestMessage message)
        {
            foreach (var kvp in _defaultHeaders)
            {
                // Do not override if already present
                if (!message.Headers.Contains(kvp.Key))
                {
                    message.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }
        }

    private async Task<(string Module, string Function, int Line)> LogRequestAsync(HttpRequestMessage request)
    {
        var defaultCaller = ("unknown", "-", 0);
        try
        {
            // Get full URL (absolute URI)
            var uri = request.RequestUri?.IsAbsoluteUri == true 
                ? request.RequestUri.ToString() 
                : (_client.BaseAddress != null && request.RequestUri != null
                    ? new Uri(_client.BaseAddress, request.RequestUri).ToString()
                    : request.RequestUri?.ToString() ?? "unknown");
            var method = request.Method.ToString();
            
            // Use Logger class for logging
            var logger = Logger.GetInstance();
            
            // Get caller info before logging (to reuse for response log)
            var callerInfo = logger.GetCallerInfo();
            
            // Build log message with full URL
            var logMessage = $"{method} {uri}";
            
            // Log request headers (with masking for sensitive values)
            var maskedHeaders = CentralMasker.MaskHeaders(request.Headers, request.Content?.Headers);
            if (maskedHeaders.Any())
            {
                var headersJson = System.Text.Json.JsonSerializer.Serialize(maskedHeaders, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                logMessage += $"\nRequest Headers: {headersJson}";
            }
            
            // Log request body if present
            string? requestBody = null;
            if (request.Content != null)
            {
                // Read body (for StringContent, this is safe to read multiple times)
                requestBody = await request.Content.ReadAsStringAsync();
                var maskedBody = CentralMasker.MaskBody(requestBody);
                logMessage += $"\nRequest Body: {maskedBody}";
                
                // Recreate content from the string so it can be read again for the actual HTTP request
                // This is safe for StringContent and ensures the stream isn't consumed
                request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
            }
            
            // Use Logger.Info for logging
            logger.Info(logMessage);
            
            return callerInfo;
        }
        catch (System.Exception ex)
        {
            // Log the exception using Logger
            var logger = Logger.GetInstance();
            logger.Exception("LogRequestAsync failed", ex);
            return defaultCaller;
        }
    }



    private async Task LogResponseAsync(HttpResponseMessage response, string? responseBody = null, (string Module, string Function, int Line)? callerInfo = null)
    {
        try
        {
            // Use Logger class for logging
            var logger = Logger.GetInstance();
            var statusCode = (int)response.StatusCode;
            var statusText = response.StatusCode.ToString();
            var uri = response.RequestMessage?.RequestUri?.ToString() ?? "unknown";
            
            // Build log message
            var logMessage = $"{statusCode} {statusText} for {uri}";
            
            // Log response body (use provided body or read it)
            if (responseBody != null)
            {
                logMessage += $"\nResponse Body: {CentralMasker.MaskBody(responseBody)}";
            }
            else if (response.Content != null)
            {
                var body = await response.Content.ReadAsStringAsync();
                logMessage += $"\nResponse Body: {CentralMasker.MaskBody(body)}";
            }
            
            // Use Logger.Info with caller info from request log
            if (callerInfo.HasValue)
            {
                logger.InfoWithCaller(logMessage, callerInfo.Value.Module, callerInfo.Value.Line, callerInfo.Value.Function);
            }
            else
            {
                logger.Info(logMessage);
            }
        }
        catch
        {
            // Silently fail logging to not break the API call
        }
    }

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _tokenDoc?.Dispose();
                _client?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}