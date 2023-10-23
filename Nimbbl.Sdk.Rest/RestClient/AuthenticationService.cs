using System.Net.Http.Json;
using System.Text.Json;
namespace Nimbbl.Sdk.Rest.RestClient;

internal class AuthenticationService
{
    private readonly HttpClient _client;
    private readonly Config _config;
    private readonly JsonSerializerOptions _serializerOptions;
    public AuthenticationService(HttpClient client, Config config, JsonSerializerOptions serializerOptions)
    {
        _client = client;
        _config = config;
        _serializerOptions = serializerOptions;
    }
    public async Task<TokenResponse> Authenticate()
    {
        var contentBody = new { access_key = _config.Key, access_secret = _config.Secret };
        var httpResponse = await _client.PostAsJsonAsync("v2/generate-token", contentBody, _serializerOptions);
        var response = await httpResponse.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<TokenResponse>(_serializerOptions);
        if (!response.Valid) throw new ApplicationException("Server returned no value");
        return response;
    }
}