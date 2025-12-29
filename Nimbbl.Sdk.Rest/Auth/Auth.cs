using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
using Nimbbl.Sdk.Rest.Exception;

namespace Nimbbl.Sdk.Rest.Auth;

/// <summary>
/// Authentication wrapper for generating and refreshing tokens
/// </summary>
public class Auth
{
    private readonly ApiClient _apiClient;

    internal Auth(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Generate token using access_key and access_secret from config
    /// </summary>
    public Task<JsonElement> GenerateTokenAsync(Dictionary<string, object?>? attributes = null)
    {
        // Always includes access_key and access_secret in request body
        var requestBody = new Dictionary<string, object?>
        {
            ["access_key"] = _apiClient.GetConfigKey(),
            ["access_secret"] = _apiClient.GetConfigSecret()
        };
        
        // Merge any additional attributes if provided
        if (attributes != null)
        {
            foreach (var kvp in attributes)
            {
                if (!requestBody.ContainsKey(kvp.Key))
                {
                    requestBody[kvp.Key] = kvp.Value;
                }
            }
        }
        
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.AuthGenerateToken, requestBody);
    }

    /// <summary>
    /// Refresh token
    /// </summary>
    public Task<JsonElement> RefreshTokenAsync(string refreshToken, string token)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new NimbblException(
                ErrorMessages.RefreshTokenRequired,
                400,
                "REFRESH_TOKEN_REQUIRED"
            );
        }
        
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new NimbblException(
                ErrorMessages.TokenRequired,
                401,
                "TOKEN_REQUIRED"
            );
        }
        
        var attributes = new Dictionary<string, object?>
        {
            ["refresh_token"] = refreshToken
        };
        
        // Bearer token required for refresh token API
        // Note: In .NET, we need to set the bearer token before making the request
        _apiClient.SetBearerToken(token);
        
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.AuthRefreshToken, attributes);
    }
}

