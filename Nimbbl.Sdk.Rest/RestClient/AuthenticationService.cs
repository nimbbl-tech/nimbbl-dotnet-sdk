using System.Net.Http.Json;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
namespace Nimbbl.Sdk.Rest.RestClient;

internal class AuthenticationService
{
    private readonly HttpClient _client;
    private readonly string _key;
    private readonly string _secret;
    private readonly JsonSerializerOptions _serializerOptions;
    public AuthenticationService(HttpClient client, string key, string secret, JsonSerializerOptions serializerOptions)
    {
        _client = client;
        _key = key;
        _secret = secret;
        _serializerOptions = serializerOptions;
    }
    public async Task<JsonElement> Authenticate()
    {
        var contentBody = new Dictionary<string, object?>
        {
            [JsonKeys.AccessKey] = _key,
            [JsonKeys.AccessSecret] = _secret
        };
        var httpResponse = await _client.PostAsJsonAsync(ApiConstants.AuthGenerateToken, contentBody, _serializerOptions);
        
        // Handle different HTTP status codes with appropriate error messages
        if (!httpResponse.IsSuccessStatusCode)
        {
            var statusCode = httpResponse.StatusCode;
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            
            // Try to parse error response if available
            string errorMessage;
            try
            {
                using var jsonDoc = JsonDocument.Parse(responseBody);
                var response = jsonDoc.RootElement;
                if (response.TryGetProperty("error", out var errorProp))
                {
                    if (errorProp.TryGetProperty("nimbbl_merchant_message", out var merchantMsg))
                    {
                        errorMessage = merchantMsg.GetString() ?? "Unknown error";
                    }
                    else if (errorProp.TryGetProperty("nimbbl_consumer_message", out var consumerMsg))
                    {
                        errorMessage = consumerMsg.GetString() ?? "Unknown error";
                    }
                    else
                    {
                        errorMessage = responseBody;
                    }
                }
                else
                {
                    errorMessage = responseBody;
                }
            }
            catch
            {
                errorMessage = responseBody;
            }
            
            // Provide specific error messages based on status code
            if (statusCode == System.Net.HttpStatusCode.Unauthorized || statusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ApplicationException(string.Format(ErrorMessages.AuthenticationFailedFormat, (int)statusCode, errorMessage));
            }
            else if (statusCode == System.Net.HttpStatusCode.ServiceUnavailable || statusCode == System.Net.HttpStatusCode.BadGateway || statusCode == System.Net.HttpStatusCode.GatewayTimeout)
            {
                throw new ApplicationException(string.Format(ErrorMessages.ServiceUnavailableFormat, (int)statusCode, errorMessage));
            }
            else if (statusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new ApplicationException(string.Format(ErrorMessages.BadRequestFormat, (int)statusCode, errorMessage));
            }
            else
            {
                throw new ApplicationException(string.Format(ErrorMessages.TokenGenerationFailedFormat, (int)statusCode, errorMessage));
            }
        }
        
        // Success path - read response body
        var successResponseBody = await httpResponse.Content.ReadAsStringAsync();
        
        using var successJsonDoc = JsonDocument.Parse(successResponseBody);
        var successResponse = successJsonDoc.RootElement;
        
        // Check if valid
        if (successResponse.TryGetProperty(JsonKeys.Valid, out var validProp) && !validProp.GetBoolean())
        {
            throw new ApplicationException(ErrorMessages.InvalidTokenResponse);
        }
        
        // Return a clone of the root element (independent copy that doesn't require document to stay alive)
        return successResponse.Clone();
    }
}