using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest.RestClient;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class ApiClientTest
{
    [Fact]
    public async Task AuthorizationInitializationTest()
    {
        var config = new Config("https://api.nimbbl.tech/api/", "access_key_1MwvMkKkweorz0ry", "access_secret_81x7ByYkRpB4g05N");
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        };
        var client = new HttpClient()
        {
            BaseAddress = new(config.Url)
        };
        var authentication = new AuthenticationService(client, config, serializerOptions);
        var response = await authentication.Authenticate();
        Assert.True(!string.IsNullOrWhiteSpace(response.Token));
        Assert.True(response.AuthPrincipal.Active);
        Assert.Equal("merchant", response.AuthPrincipal.Type);
        Assert.True(DateTime.UtcNow < response.ExpiresAt);
    }
}