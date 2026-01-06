using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
using Nimbbl.Sdk.Rest.Exception;
using static Nimbbl.Sdk.Rest.Common.ErrorCodes;
using static Nimbbl.Sdk.Rest.Common.HttpStatusCodes;

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
    /// Automatically caches the token for subsequent API calls
    /// </summary>
    /// <returns>JSON response containing authentication token</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/generate-token-v-3/">Generate Token API</see> for more details.</remarks>
    public async Task<JsonElement> GenerateTokenAsync()
    {
        var requestBody = new Dictionary<string, object?>
        {
            ["access_key"] = _apiClient.GetConfigKey(),
            ["access_secret"] = _apiClient.GetConfigSecret()
        };
        
        var response = await _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.AuthGenerateToken, requestBody);
        
        // Auto-cache the token for subsequent API calls
        if (response.ValueKind == JsonValueKind.Object)
        {
            if (response.TryGetProperty("token", out var tokenProp) && tokenProp.ValueKind == JsonValueKind.String)
            {
                var token = tokenProp.GetString();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    // Parse expires_at if available
                    DateTime? expiresAt = null;
                    if (response.TryGetProperty("expires_at", out var expiresProp) && expiresProp.ValueKind == JsonValueKind.String)
                    {
                        var expiresStr = expiresProp.GetString();
                        if (!string.IsNullOrWhiteSpace(expiresStr) && DateTime.TryParse(expiresStr, out var parsedExpires))
                        {
                            expiresAt = parsedExpires;
                        }
                    }
                    
                    // Cache the token
                    _apiClient.SetBearerToken(token, expiresAt);
                }
            }
        }
        
        return response;
    }

    /// <summary>
    /// Refresh an authentication token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token obtained from previous authentication</param>
    /// <param name="token">Current bearer token</param>
    /// <returns>JSON response containing new authentication token</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/refresh-token-v-3/">Refresh Token API</see> for more details.</remarks>
    public Task<JsonElement> RefreshTokenAsync(string refreshToken, string token)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new NimbblException(
                ErrorMessages.RefreshTokenRequired,
                BadRequest,
                RefreshTokenRequired
            );
        }
        
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new NimbblException(
                ErrorMessages.TokenRequired,
                Unauthorized,
                TokenRequired
            );
        }
        
        var attributes = new Dictionary<string, object?>
        {
            ["refresh_token"] = refreshToken
        };
        
        // Bearer token required for refresh token API
        // Note: In .NET, we need to set the bearer token before making the request
        _apiClient.SetBearerToken(token);
        
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.AuthRefreshToken, attributes);
    }
}

