using System;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class ApiClientTest : TestBase
{
    [Fact]
    public async Task AuthorizationInitializationTest()
    {
        // Use Auth.GenerateTokenAsync() which now uses the same internal logic as automatic token generation
        var response = await Api.Auth().GenerateTokenAsync();
        
        Assert.True(response.ValueKind == JsonValueKind.Object);
        Assert.True(response.TryGetProperty("token", out var tokenProp));
        Assert.True(tokenProp.ValueKind == JsonValueKind.String);
        Assert.True(!string.IsNullOrWhiteSpace(tokenProp.GetString()));
        
        if (response.TryGetProperty("expires_at", out var expiresProp) && expiresProp.ValueKind == JsonValueKind.String)
        {
            var expiresStr = expiresProp.GetString();
            if (!string.IsNullOrWhiteSpace(expiresStr) && DateTime.TryParse(expiresStr, out var expiresAt))
            {
                Assert.True(DateTime.UtcNow < expiresAt);
            }
        }
    }
}
