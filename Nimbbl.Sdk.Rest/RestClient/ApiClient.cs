using System.Net;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
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
    private readonly bool _encryptPayload;
        private readonly Dictionary<string, string> _defaultHeaders = new(StringComparer.OrdinalIgnoreCase);
        private string? _explicitBearerToken;
        private DateTime? _explicitTokenExpiryUtc;

    private JsonDocument? _tokenDoc;
    private readonly Logger _logger;
    
    private JsonElement Token => _tokenDoc?.RootElement ?? default;
    
    /// <summary>
    /// Gets the access key for authentication
    /// </summary>
    internal string GetConfigKey() => _key;
    
    /// <summary>
    /// Gets the access secret for authentication
    /// </summary>
    internal string GetConfigSecret() => _secret;
    
    /// <summary>
    /// Gets whether payload encryption is enabled
    /// </summary>
    internal bool IsEncryptPayloadEnabled() => _encryptPayload;
    
    // handler: optional HttpMessageHandler for tests (mock/stub the transport). Null uses a real HttpClient.
    public ApiClient(string key, string secret, string baseUrl, bool encryptPayload = false, HttpMessageHandler? handler = null)
    {
        _key = key;
        _secret = secret;
        _encryptPayload = encryptPayload;
        _logger = Logger.GetInstance();
        
        // Log encryption flag status for debugging
        _logger.DebugWithCaller($"ApiClient initialized - encryptPayload: {_encryptPayload}");
        _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            // Keep '+', '/', and unicode unescaped in request bodies (parity with PHP; also makes
            // the raw request log match the masked log). Valid JSON — servers parse it identically.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            // Don't auto-convert strings to DateTime - let properties handle their own types
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        _client = handler != null ? new HttpClient(handler) : new HttpClient();
        _client.BaseAddress = new(baseUrl);
        _client.Timeout = TimeSpan.FromSeconds(ApiConstants.DefaultHttpTimeoutSeconds);
        // Default User-Agent: SDK name/version + runtime
        _defaultHeaders["User-Agent"] = $"{SdkConstants.SdkName}/{SdkConstants.SdkVersion} .NET/{Environment.Version}";
    }

    /// <summary>
    /// Sends a GET request to the specified URI
    /// Merchant token is automatically generated and used for authentication
    /// </summary>
    /// <param name="requestUri">Request URI</param>
    public async Task<TResponse> Get<TResponse>(string requestUri)
    {
        return await ExecuteRequestAsync<TResponse>(CreateRequest(HttpMethod.Get, requestUri));
    }

    /// <summary>
    /// Sends a GET request with query parameters
    /// Merchant token is automatically generated and used for authentication
    /// </summary>
    /// <param name="requestUri">Request URI</param>
    /// <param name="queryParams">Query parameters</param>
    public async Task<TResponse> Get<TResponse>(string requestUri, Dictionary<string, object?>? queryParams)
    {
        var uri = BuildQueryString(requestUri, queryParams);
        return await ExecuteRequestAsync<TResponse>(CreateRequest(HttpMethod.Get, uri));
    }

    /// <summary>
    /// Sends a POST request with a request body
    /// Merchant token is automatically generated and used for authentication
    /// </summary>
    /// <param name="requestUri">Request URI</param>
    /// <param name="body">Request body</param>
    public async Task<TResponse> Post<TRequest, TResponse>(string requestUri, TRequest body)
        where TRequest : class
    {
        return await ExecuteRequestAsync<TResponse>(CreateRequest(HttpMethod.Post, requestUri, body));
    }

    /// <summary>
    /// Sends a PATCH request with a request body
    /// Merchant token is automatically generated and used for authentication
    /// </summary>
    /// <param name="requestUri">Request URI</param>
    /// <param name="body">Request body</param>
    public async Task<TResponse> Patch<TRequest, TResponse>(string requestUri, TRequest body)
        where TRequest : class
    {
        return await ExecuteRequestAsync<TResponse>(CreateRequest(HttpMethod.Patch, requestUri, body));
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI
    /// Merchant token is automatically generated and used for authentication
    /// </summary>
    /// <param name="requestUri">Request URI</param>
    public async Task<TResponse> Delete<TResponse>(string requestUri)
    {
        return await ExecuteRequestAsync<TResponse>(CreateRequest(HttpMethod.Delete, requestUri));
    }

    /// <summary>
    /// Sends a DELETE request with query parameters
    /// Merchant token is automatically generated and used for authentication
    /// </summary>
    /// <param name="requestUri">Request URI</param>
    /// <param name="queryParams">Query parameters</param>
    public async Task<TResponse> Delete<TResponse>(string requestUri, Dictionary<string, object?>? queryParams)
    {
        var uri = BuildQueryString(requestUri, queryParams);
        return await ExecuteRequestAsync<TResponse>(CreateRequest(HttpMethod.Delete, uri));
    }

    /// <summary>
    /// Sets an explicit bearer token for authentication
    /// </summary>
    public void SetBearerToken(string token, DateTime? expiresAtUtc = null)
    {
        _explicitBearerToken = token;
        _explicitTokenExpiryUtc = expiresAtUtc;
    }

    /// <summary>
    /// Adds a default header to all requests
    /// </summary>
    public void AddHeader(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            _defaultHeaders[key] = value;
        }


    /// <summary>
    /// Executes an HTTP request with logging, retry logic, and error handling
    /// Merchant token is automatically generated and used for authentication
    /// </summary>
    /// <param name="message">HTTP request message</param>
    private async Task<TResponse> ExecuteRequestAsync<TResponse>(HttpRequestMessage message)
    {
        // Get caller info from request log to reuse for response log
        var callerInfo = await LogRequestAsync(message);
        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await RetryRequestAsync(async (req, ci) => await SendRequestAsync(req, ci), message, ApiConstants.DefaultRetryCount, callerInfo);
        }
        catch (System.Exception ex)
        {
            _logger.ExceptionWithCaller("RetryRequestAsync failed", ex, callerInfo);
            throw;
        }
        
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
            _logger.ExceptionWithCaller("Error reading response body", ex, callerInfo);
            throw;
        }
        
        // Log raw response body before deserialization, reusing caller info from request
        await LogResponseAsync(httpResponse, responseBody, callerInfo);

        // If HTTP status is success but body carries an error envelope, surface it
        if (httpResponse.IsSuccessStatusCode)
        {
            ThrowIfErrorEnvelope(responseBody);
        }
        
        // Note: Raw JSON response logging is handled by LogResponseAsync() to avoid duplication
        // Check if response contains encrypted_response and decrypt it if needed
        if (httpResponse.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                using var responseDoc = JsonDocument.Parse(responseBody);
                var responseRoot = responseDoc.RootElement;
                
                // Check if response contains encrypted_response field
                if (responseRoot.TryGetProperty(JsonKeys.EncryptedResponse, out var encryptedResponseProp) 
                    && encryptedResponseProp.ValueKind == JsonValueKind.String)
                {
                    var encryptedResponse = encryptedResponseProp.GetString();
                    if (!string.IsNullOrWhiteSpace(encryptedResponse))
                    {
                        try
                        {
                            // Decrypt the encrypted response
                            var encryption = new Encryption(_secret);
                            var decryptedJson = encryption.Decrypt(encryptedResponse, true);
                            responseBody = decryptedJson;
                            
                            _logger.InfoWithCaller("Successfully decrypted encrypted response", callerInfo);
                            
                            // Update the content with decrypted response
                            httpResponse.Content = new StringContent(responseBody, System.Text.Encoding.UTF8, "application/json");
                        }
                        catch (System.Exception decryptEx)
                        {
                            _logger.ExceptionWithCaller($"Failed to decrypt encrypted response: {decryptEx.Message}", decryptEx, callerInfo);
                            // Continue with original responseBody - let deserialization handle it
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, continue with original responseBody
            }
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
            // Log the error with the raw response for debugging
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
                _logger.ErrorWithCaller(errorMsg, callerInfo);
            }
            catch
            {
                // If logging fails, ignore
            }
            throw;
        }
        catch (System.Exception ex)
        {
            // Catch any other exceptions during deserialization
            if (responseBody != null)
            {
                try
                {
                    _logger.ErrorWithCaller($"Unexpected error during deserialization:\n{ex.GetType().Name}: {ex.Message}\n\nRaw JSON:\n{responseBody}", callerInfo);
                }
                catch
                {
                    // If logging fails, ignore
                }
            }
            throw;
        }
        
        if (response == null) throw new ApplicationException(ErrorMessages.MessageNoValueReturned);
        return response;
    }

    /// <summary>
    /// Retries a request if it fails, with automatic token refresh on auth failures
    /// Merchant token is automatically generated and used for authentication
    /// retryCount: Number of retry attempts (1 = 1 retry = 2 total attempts)
    /// </summary>
    private async Task<HttpResponseMessage> RetryRequestAsync(Func<HttpRequestMessage, (string Module, string Function, int Line)?, Task<HttpResponseMessage>> sendRequest, HttpRequestMessage message, ushort retryCount, (string Module, string Function, int Line)? callerInfo = null)
    {
        HttpResponseMessage response;
        do
        {
            response = await sendRequest(CloneRequest(message), callerInfo);
            if (response.IsSuccessStatusCode) 
            {
                return response;
            }
            if (IsAuthFailure(response))
            {
                // Clear all token caches to force regeneration on retry
                _tokenDoc?.Dispose();
                _tokenDoc = null;
                _explicitBearerToken = null;
                _explicitTokenExpiryUtc = null;
                _logger.InfoWithCaller("Authentication failure detected. Clearing token cache for retry.", callerInfo);
            }
        } while (retryCount-- > 0);
        await HandleErrorResponseAsync(response, callerInfo);
        return response;
    }

    /// <summary>
    /// Checks if the response indicates an authentication failure
    /// </summary>
    private static bool IsAuthFailure(HttpResponseMessage response)
    {
        return response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized;
    }

    /// <summary>
    /// Handles error responses by parsing error messages and throwing appropriate exceptions
    /// Always logs error responses with masking (unmasked only when debug is enabled)
    /// </summary>
    private async Task HandleErrorResponseAsync(HttpResponseMessage httpResponse, (string Module, string Function, int Line)? callerInfo = null)
    {
        var errorText = await httpResponse.Content.ReadAsStringAsync();
        
        // Build error log message
        var statusCode = (int)httpResponse.StatusCode;
        var statusText = httpResponse.StatusCode.ToString();
        var uri = httpResponse.RequestMessage?.RequestUri?.ToString() ?? "unknown";
        var errorLogMessage = $"HTTP {statusCode} {statusText} for {uri}";
        
        // Log error response headers (unmasked for exceptions/errors)
        var headersToLog = CentralMasker.GetUnmaskedHeaders(httpResponse.Headers, httpResponse.Content?.Headers);
        if (headersToLog.Any())
        {
            // Use compact JSON (no indentation) for better log readability
            var headersJson = JsonSerializer.Serialize(headersToLog, new JsonSerializerOptions 
            { 
                WriteIndented = false 
            });
            errorLogMessage += $"\nResponse Headers: {headersJson}";
        }
        
        // Log the error response body (unmasked for exceptions/errors)
        errorLogMessage += $"\nResponse Body: {errorText}";
        
        _logger.ErrorWithCaller(errorLogMessage, callerInfo);
        
        // Check if error response is encrypted and decrypt if needed
        if (IsValidJson(errorText))
        {
            try
            {
                using var errorDoc = JsonDocument.Parse(errorText);
                var errorRoot = errorDoc.RootElement;
                
                // Check if response contains encrypted_response field
                if (errorRoot.TryGetProperty(JsonKeys.EncryptedResponse, out var encryptedResponseProp) 
                    && encryptedResponseProp.ValueKind == JsonValueKind.String)
                {
                    var encryptedResponse = encryptedResponseProp.GetString();
                    if (!string.IsNullOrWhiteSpace(encryptedResponse))
                    {
                        try
                        {
                            // Decrypt the encrypted error response
                            var encryption = new Encryption(_secret);
                            var decryptedJson = encryption.Decrypt(encryptedResponse, true);
                            errorText = decryptedJson;
                            
                            _logger.InfoWithCaller("Successfully decrypted encrypted error response", callerInfo);
                            
                            // Re-parse the decrypted JSON
                            errorDoc.Dispose();
                            using var decryptedDoc = JsonDocument.Parse(errorText);
                            var decryptedRoot = decryptedDoc.RootElement;
                            
                            // Try to get error object from decrypted response
                            if (decryptedRoot.TryGetProperty(JsonKeys.Error, out var errorObj))
                            {
                                var merchantMessage = errorObj.TryGetProperty(JsonKeys.ErrorMerchantMessage, out var mm) ? mm.GetString() : null;
                                var consumerMessage = errorObj.TryGetProperty(JsonKeys.ErrorConsumerMessage, out var cm) ? cm.GetString() : null;
                                var errorCode = errorObj.TryGetProperty(JsonKeys.ErrorCode, out var ec) ? ec.GetString() : null;
                                
                                var message = merchantMessage ?? consumerMessage ?? errorCode ?? ErrorMessages.MessageApiRequestFailed;
                                throw MapException(httpResponse.StatusCode, message, errorCode ?? ErrorCodes.ServerError);
                            }
                        }
                        catch (System.Exception decryptEx)
                        {
                            _logger.ExceptionWithCaller($"Failed to decrypt encrypted error response: {decryptEx.Message}", decryptEx, callerInfo);
                            // Fall through to handle as regular error
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, continue with original errorText
            }
        }
        
        // Use unmasked errorText for exceptions (exceptions are not logging)
        if (!IsValidJson(errorText)) throw MapException(httpResponse.StatusCode, errorText, ErrorCodes.ServerError);
        
        try
        {
            using var errorDoc = JsonDocument.Parse(errorText);
            var errorRoot = errorDoc.RootElement;
            
            // Try to get error object
            if (errorRoot.TryGetProperty(JsonKeys.Error, out var errorObj))
            {
                var merchantMessage = errorObj.TryGetProperty(JsonKeys.ErrorMerchantMessage, out var mm) ? mm.GetString() : null;
                var consumerMessage = errorObj.TryGetProperty(JsonKeys.ErrorConsumerMessage, out var cm) ? cm.GetString() : null;
                var errorCode = errorObj.TryGetProperty(JsonKeys.ErrorCode, out var ec) ? ec.GetString() : null;
                
                // Use merchant message if available, otherwise consumer message, otherwise error code
                var message = merchantMessage ?? consumerMessage ?? errorCode ?? ErrorMessages.MessageApiRequestFailed;
                throw MapException(httpResponse.StatusCode, message, errorCode ?? ErrorCodes.ServerError);
            }
        }
        catch (JsonException)
        {
            // If deserialization fails, try to extract error message manually
            if (errorText.Contains(JsonKeys.ErrorMerchantMessage))
            {
                // Try to extract the message from JSON
                var startIdx = errorText.IndexOf($"\"{JsonKeys.ErrorMerchantMessage}\"");
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

    /// <summary>
    /// Maps HTTP status codes to appropriate Nimbbl exception types
    /// </summary>
    private static NimbblException MapException(HttpStatusCode statusCode, string message, string? errorCode)
    {
        var code = (int)statusCode;
        var safeMessage = string.IsNullOrWhiteSpace(message) ? ErrorMessages.MessageApiRequestFailed : message;
        return statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AuthenticationException(safeMessage, code, errorCode ?? ErrorCodes.AuthError),
            HttpStatusCode.BadRequest or (HttpStatusCode)HttpStatusCodes.UnprocessableEntity => new BadRequestException(safeMessage, code, errorCode ?? ErrorCodes.ServerError),
            HttpStatusCode.NotFound => new NotFoundException(safeMessage, code, errorCode ?? ErrorCodes.ServerError),
            (HttpStatusCode)HttpStatusCodes.TooManyRequests => new RateLimitException(safeMessage, code, errorCode ?? ErrorCodes.ServerError),
            HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout
                => new ServerException(safeMessage, code, errorCode ?? ErrorCodes.ServerError),
            _ => new ApiException(safeMessage, code, errorCode ?? ErrorCodes.ServerError)
        };
    }
    /// <summary>
    /// Sends an HTTP request with automatic authorization
    /// Merchant token is automatically generated and used for authentication
    /// </summary>
    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, (string Module, string Function, int Line)? callerInfo = null)
    {
        try
        {
            await LazyAuthorizeRequestAsync(request);
            var response = await _client.SendAsync(request);
            return response;
        }
        catch (System.Exception ex)
        {
            _logger.ExceptionWithCaller("SendRequestAsync failed", ex, callerInfo);
            throw;
        }
    }

    /// <summary>
    /// Checks if the cached token is expired or will expire within the expiration threshold
    /// Checks both explicit bearer token and cached token document
    /// </summary>
    private bool IsTokenExpired()
    {
        var expirationThreshold = TimeSpan.FromMinutes(ApiConstants.TokenExpirationThresholdMinutes);

        // First check explicit bearer token
        if (!string.IsNullOrEmpty(_explicitBearerToken))
        {
            if (_explicitTokenExpiryUtc == null)
            {
                // No expiration provided, consider valid (don't auto-regenerate)
                return false;
            }
            
            // Check if explicit token expires within the expiration threshold
            var explicitTimeRemaining = _explicitTokenExpiryUtc.Value - DateTime.UtcNow;
            
            if (explicitTimeRemaining > expirationThreshold)
            {
                // Explicit token is still valid
                return false;
            }
        }

        // Check cached token document
        if (_tokenDoc == null || Token.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        if (!Token.TryGetProperty(JsonKeys.ExpiresAt, out var expiresProp) || expiresProp.ValueKind != JsonValueKind.String)
        {
            // No expiration field, consider expired to force regeneration
            return true;
        }

        var expiresStr = expiresProp.GetString();
        if (string.IsNullOrEmpty(expiresStr) || !DateTime.TryParse(expiresStr, out var expiresAt))
        {
            // Can't parse expiration, consider expired
            return true;
        }

        // Check if token expires within the expiration threshold
        // If time remaining is within the threshold, consider it expired
        var cachedTimeRemaining = expiresAt - DateTime.UtcNow;

        return cachedTimeRemaining <= expirationThreshold;
    }

    /// <summary>
    /// Ensures a valid merchant token exists, generating one if needed
    /// This method will:
    /// 1. Check if token exists and is not expired (beyond expiration threshold)
    /// 2. If no token or expired, generate a new merchant token
    /// 3. Cache the token with expiration time (UTC)
    /// </summary>
    private async Task<string> EnsureMerchantTokenAsync()
    {
        // Validate that access_key and access_secret are available
        var accessKey = GetConfigKey();
        var accessSecret = GetConfigSecret();
        
        if (string.IsNullOrWhiteSpace(accessKey))
        {
            throw new ApplicationException(ErrorMessages.AccessKeyMissing);
        }
        
        if (string.IsNullOrWhiteSpace(accessSecret))
        {
            throw new ApplicationException(ErrorMessages.AccessSecretMissing);
        }
        
        // First check explicit bearer token if it's not expired
        if (!string.IsNullOrEmpty(_explicitBearerToken))
        {
            if (_explicitTokenExpiryUtc == null || DateTime.UtcNow < _explicitTokenExpiryUtc.Value)
            {
                // Check if it expires within the expiration threshold
                if (_explicitTokenExpiryUtc == null || (_explicitTokenExpiryUtc.Value - DateTime.UtcNow) > TimeSpan.FromMinutes(ApiConstants.TokenExpirationThresholdMinutes))
                {
                    return _explicitBearerToken;
                }
            }
        }

        // Check if we have a valid cached token (not expired within the expiration threshold)
        if (!IsTokenExpired() && _tokenDoc != null && Token.ValueKind == JsonValueKind.Object)
        {
            var tokenValue = Token.TryGetProperty(JsonKeys.Token, out var cachedTokenProp) ? cachedTokenProp.GetString() : null;
            if (!string.IsNullOrEmpty(tokenValue))
            {
                return tokenValue;
            }
        }

        // Token doesn't exist or is expired, generate a new one
        try
        {
            var tokenResponse = await GenerateTokenInternalAsync();

            if (tokenResponse.ValueKind != JsonValueKind.Object || 
                !tokenResponse.TryGetProperty(JsonKeys.Token, out var tokenProp) || 
                tokenProp.ValueKind != JsonValueKind.String)
            {
                throw new ApplicationException(ErrorMessages.TokenNotFoundInResponse);
            }

            var token = tokenProp.GetString();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ApplicationException(ErrorMessages.TokenEmpty);
            }

            // Parse expires_at if available
            DateTime? expiresAt = null;
            if (tokenResponse.TryGetProperty(JsonKeys.ExpiresAt, out var expiresProp) && expiresProp.ValueKind == JsonValueKind.String)
            {
                var expiresStr = expiresProp.GetString();
                if (!string.IsNullOrWhiteSpace(expiresStr) && DateTime.TryParse(expiresStr, out var parsedExpires))
                {
                    expiresAt = parsedExpires;
                }
            }

            // Cache the token with expiration time (expires_at is in UTC)
            SetBearerToken(token, expiresAt);

            // Also cache in _tokenDoc for backward compatibility
            _tokenDoc?.Dispose();
            // Serialize the JsonElement to string and parse it into a new JsonDocument
            var tokenResponseJson = System.Text.Json.JsonSerializer.Serialize(tokenResponse);
            _tokenDoc = JsonDocument.Parse(tokenResponseJson);

            return token;
        }
        catch (System.Exception ex)
        {
            _logger.ExceptionWithCaller($"Failed to ensure merchant token: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Internal method to generate authentication token using access_key and access_secret
    /// This is used both by Auth.GenerateTokenAsync() and by automatic token generation
    /// Uses ExecuteRequestAsync to ensure proper logging of request/response
    /// </summary>
    internal async Task<JsonElement> GenerateTokenInternalAsync()
    {
        var contentBody = new Dictionary<string, object?>
        {
            [JsonKeys.AccessKey] = _key,
            [JsonKeys.AccessSecret] = _secret
        };
        
        // Use ExecuteRequestAsync to ensure proper logging (request/response are logged with masking)
        var response = await ExecuteRequestAsync<JsonElement>(CreateRequest(HttpMethod.Post, ApiConstants.AuthGenerateToken, contentBody));
        
        // Check if valid
        if (response.TryGetProperty(JsonKeys.Valid, out var validProp) && !validProp.GetBoolean())
        {
            throw new ApplicationException(ErrorMessages.InvalidTokenResponse);
        }
        
        // Return the response (already a JsonElement from ExecuteRequestAsync)
        return response;
    }

    /// <summary>
    /// Lazily authorizes the request by setting bearer token (cached or explicitly set)
    /// Priority: 1. Explicit bearer token > 2. Auto-generated merchant token > 3. Cached token
    /// Note: generate-token endpoint doesn't require authentication (uses access_key/access_secret in body)
    /// </summary>
    private async Task LazyAuthorizeRequestAsync(HttpRequestMessage request)
    {
        // Check if this is the generate-token endpoint - it doesn't require bearer token authentication
        var requestUri = request.RequestUri?.ToString() ?? "";
        if (requestUri.Contains(ApiConstants.AuthGenerateToken, StringComparison.OrdinalIgnoreCase))
        {
            // Generate token endpoint uses access_key/access_secret in body, no bearer token needed
            return;
        }
        
        // Priority 1: Explicitly set bearer token if valid (or no expiry provided)
        if (!string.IsNullOrEmpty(_explicitBearerToken))
        {
            if (_explicitTokenExpiryUtc == null || DateTime.UtcNow < _explicitTokenExpiryUtc.Value)
            {
                request.Headers.Authorization = new("Bearer", _explicitBearerToken);
                return;
            }
        }

        // Priority 2: Auto-generate merchant token if needed (for all non-auth requests)
        // Since nimbbl_api supports merchant tokens for all endpoints (higher_level_token_supported=True by default),
        // we can auto-generate merchant tokens for all APIs
        System.Exception? tokenGenerationException;
        try
        {
            var merchantToken = await EnsureMerchantTokenAsync();
            request.Headers.Authorization = new("Bearer", merchantToken);
            return;
        }
        catch (System.Exception ex)
        {
            // Store the exception for better error reporting
            tokenGenerationException = ex;
            // If token generation fails, try cached token as fallback
            _logger.WarningWithCaller($"Failed to auto-generate merchant token: {ex.Message}. Trying cached token as fallback.");
        }

        // Priority 3: Fallback to cached token if still no token
        if (_tokenDoc != null && Token.ValueKind == JsonValueKind.Object)
        {
            var tokenString = Token.TryGetProperty(JsonKeys.Token, out var finalTokenProp) ? finalTokenProp.GetString() : null;
            if (!string.IsNullOrEmpty(tokenString))
            {
                request.Headers.Authorization = new("Bearer", tokenString);
                return;
            }
        }

        // If we still don't have a token, throw an error with details from token generation failure
        var errorMessage = ErrorMessages.NoValidTokenAvailable;
        if (tokenGenerationException != null)
        {
            // If the exception already contains a clear error message (from token generation),
            // use it directly. Otherwise, provide generic guidance.
            if (tokenGenerationException.Message.Contains(ErrorMessages.AccessKeyKeyword) || 
                tokenGenerationException.Message.Contains(ErrorMessages.AccessSecretKeyword) ||
                tokenGenerationException.Message.Contains(ErrorMessages.AuthenticationFailedKeyword) ||
                tokenGenerationException.Message.Contains(ErrorMessages.ServiceUnavailableKeyword) ||
                tokenGenerationException.Message.Contains(ErrorMessages.NetworkKeyword) ||
                tokenGenerationException.Message.Contains(ErrorMessages.UnreachableKeyword))
            {
                errorMessage = tokenGenerationException.Message;
            }
            else
            {
                errorMessage += ErrorMessages.TokenGenerationErrorPrefix + tokenGenerationException.Message;
                errorMessage += " " + ErrorMessages.CheckCredentialsOrNetwork;
            }
        }
        else
        {
            errorMessage += " " + ErrorMessages.CheckCredentialsOrNetwork;
        }
        throw new ApplicationException(errorMessage, tokenGenerationException);
    }



    /// <summary>
    /// Builds a query string from a dictionary of parameters
    /// </summary>
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

    /// <summary>
    /// Extracts an order id for the structured log context (parity with the PHP SDK request/response logs).
    /// Looks in the JSON body (order_id | nimbbl_order_id | order.order_id) and falls back to the
    /// URI query string (?order_id=...). Returns null when none is present.
    /// </summary>
    private static string? ExtractOrderId(string? json, string? uri)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    var orderId = JsonUtils.TryGetString(root, JsonKeys.OrderId)
                        ?? JsonUtils.TryGetString(root, JsonKeys.NimbblOrderId);
                    if (string.IsNullOrEmpty(orderId)
                        && root.TryGetProperty(JsonKeys.Order, out var order)
                        && order.ValueKind == JsonValueKind.Object)
                    {
                        orderId = JsonUtils.TryGetString(order, JsonKeys.OrderId);
                    }
                    if (!string.IsNullOrEmpty(orderId)) return orderId;
                }
            }
            catch (JsonException)
            {
                // not JSON — fall through to URI
            }
        }

        if (!string.IsNullOrWhiteSpace(uri))
        {
            var match = System.Text.RegularExpressions.Regex.Match(uri, @"[?&]order_id=([^&]+)");
            if (match.Success) return Uri.UnescapeDataString(match.Groups[1].Value);
        }

        return null;
    }

    /// <summary>
    /// Creates an HTTP request message without a body
    /// </summary>
    private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
    {
        var message = new HttpRequestMessage(method, requestUri);
        ApplyDefaultHeaders(message);
        return message;
    }

    /// <summary>
    /// Creates an HTTP request message with a serialized body
    /// </summary>
    private HttpRequestMessage CreateRequest<TRequest>(HttpMethod method, string requestUri, TRequest body) where TRequest : class
    {
        var message = CreateRequest(method, requestUri);
        // Serialize body to string first for logging, then create content
        var bodyJson = JsonSerializer.Serialize(body, _serializerOptions);
        message.Content = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");
        return message;
    }

    /// <summary>
    /// Clones an HTTP request message for retry purposes
    /// </summary>
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
    private static void ThrowIfErrorEnvelope(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody)) return;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return;
            if (!root.TryGetProperty(JsonKeys.Error, out var errorObj) || errorObj.ValueKind != JsonValueKind.Object) return;

            var merchantMessage = errorObj.TryGetProperty(JsonKeys.ErrorMerchantMessage, out var mm) ? mm.GetString() : null;
            var consumerMessage = errorObj.TryGetProperty(JsonKeys.ErrorConsumerMessage, out var cm) ? cm.GetString() : null;
            var errorCode = errorObj.TryGetProperty(JsonKeys.ErrorCode, out var ec) ? ec.GetString() : null;
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
    /// Applies default headers to the request message
    /// </summary>
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

    /// <summary>
    /// Logs the HTTP request details and returns caller info for reuse in response logs
    /// Only logs when debug mode is enabled
    /// </summary>
    private async Task<(string Module, string Function, int Line)> LogRequestAsync(HttpRequestMessage request)
    {
        try
        {
            // Get caller info once to reuse for response logs
            var callerInfo = Logger.GetCaller();
            
            // Only log when debug mode is enabled
            if (!Logger.IsDebugEnabled())
            {
                return callerInfo;
            }
            
            // Get full URL (absolute URI)
            var uri = request.RequestUri?.IsAbsoluteUri == true 
                ? request.RequestUri.ToString() 
                : (_client.BaseAddress != null && request.RequestUri != null
                    ? new Uri(_client.BaseAddress, request.RequestUri).ToString()
                    : request.RequestUri?.ToString() ?? "unknown");
            var method = request.Method.ToString();
            
            // Build log message with full URL
            var logMessage = $"{method} {uri}";
            
            // Log request headers (masked for security)
            var headersToLog = CentralMasker.MaskHeaders(request.Headers, request.Content?.Headers);
            if (headersToLog.Any())
            {
                // Use compact JSON (no indentation) for better log readability
                var headersJson = JsonSerializer.Serialize(headersToLog, new JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
                logMessage += $"\nRequest Headers: {headersJson}";
            }
            
            // Log request body if present
            string? requestBody = null;
            if (request.Content != null)
            {
                // Read body (for StringContent, this is safe to read multiple times)
                requestBody = await request.Content.ReadAsStringAsync();
                var bodyToLog = CentralMasker.MaskBody(requestBody);
                logMessage += $"\nRequest Body: {bodyToLog}";
                
                // Recreate content from the string so it can be read again for the actual HTTP request
                // This is safe for StringContent and ensures the stream isn't consumed
                request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
            }
            
            // Use Logger.InfoWithCaller for logging with caller info + structured HTTP context
            var requestContext = new LogContext
            {
                ApiVersion = ApiConstants.ApiVersion,
                ApiTag = callerInfo.Module,
                Uri = uri,
                OrderId = ExtractOrderId(requestBody, uri)
            };
            _logger.InfoWithCaller(logMessage, callerInfo, requestContext);

            // Log raw request body for debugging (unmasked)
            if (requestBody != null)
            {
                _logger.DebugWithCaller($"Raw JSON Request (before sending):\n{requestBody}", callerInfo);
            }
            else
            {
                _logger.DebugWithCaller("Request body is NULL", callerInfo);
            }
            
            return callerInfo;
        }
        catch (System.Exception ex)
        {
            // Log the exception using Logger
            _logger.ExceptionWithCaller("LogRequestAsync failed", ex);
            return ("unknown", "-", 0);
        }
    }



    /// <summary>
    /// Logs the HTTP response details
    /// Only logs when debug mode is enabled
    /// </summary>
    private static async Task LogResponseAsync(HttpResponseMessage response, string? responseBody = null, (string Module, string Function, int Line)? callerInfo = null)
    {
        try
        {
            // Only log when debug mode is enabled
            if (!Logger.IsDebugEnabled())
            {
                return;
            }
            
            var logger = Logger.GetInstance();
            var statusCode = (int)response.StatusCode;
            var uri = response.RequestMessage?.RequestUri?.ToString() ?? "unknown";

            // Short message; status and URI are carried in the structured context ([StatusCode], [URI]).
            var logMessage = "Response received";
            
            // Log response headers (masked for security)
            var headersToLog = CentralMasker.MaskHeaders(response.Headers, response.Content?.Headers);
            if (headersToLog.Any())
            {
                // Use compact JSON (no indentation) for better log readability
                var headersJson = JsonSerializer.Serialize(headersToLog, new JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
                logMessage += $"\nResponse Headers: {headersJson}";
            }
            
            // Log response body (masked for security)
            if (responseBody != null)
            {
                var bodyToLog = CentralMasker.MaskBody(responseBody);
                logMessage += $"\nResponse Body: {bodyToLog}";
            }
            else if (response.Content != null)
            {
                var body = await response.Content.ReadAsStringAsync();
                var bodyToLog = CentralMasker.MaskBody(body);
                logMessage += $"\nResponse Body: {bodyToLog}";
            }
            
            // Use Logger.Info with caller info from request log (or get fresh if not provided) + structured HTTP context
            var responseContext = new LogContext
            {
                ApiVersion = ApiConstants.ApiVersion,
                ApiTag = callerInfo?.Module,
                Uri = uri,
                StatusCode = statusCode.ToString(),
                OrderId = ExtractOrderId(responseBody, uri)
            };
            logger.InfoWithCaller(logMessage, callerInfo, responseContext);
            
            // Log raw response body for debugging (unmasked)
            var rawBody = responseBody ?? (response.Content != null ? await response.Content.ReadAsStringAsync() : null);
            if (rawBody != null)
            {
                logger.DebugWithCaller($"Raw JSON Response (before deserialization):\n{rawBody}", callerInfo);
            }
            else
            {
                logger.DebugWithCaller("Response body is NULL", callerInfo);
            }
        }
        catch (System.Exception ex)
        {
            // Log the exception but don't throw to avoid breaking the API call
            var logger = Logger.GetInstance();
            logger.ExceptionWithCaller("LogResponseAsync failed", ex, callerInfo);
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
    }
}