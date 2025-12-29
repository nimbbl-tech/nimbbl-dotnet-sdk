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
        var contentBody = new { access_key = _key, access_secret = _secret };
        var httpResponse = await _client.PostAsJsonAsync(ApiConstants.AuthGenerateToken, contentBody, _serializerOptions);
        var responseBody = await httpResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        
        using var jsonDoc = JsonDocument.Parse(responseBody);
        var response = jsonDoc.RootElement;
        
        // Check if valid
        if (response.TryGetProperty(ErrorMessages.ResponseKeyValid, out var validProp) && !validProp.GetBoolean())
        {
            throw new ApplicationException(ErrorMessages.InvalidTokenResponse);
        }
        
        // Return a clone of the root element (independent copy that doesn't require document to stay alive)
        return response.Clone();
    }
}