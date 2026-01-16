using System;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest.Exception;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class AuthTest : TestBase
{
    [Fact]
    public async Task ShouldGenerateToken()
    {
        var response = await Api.Auth().GenerateTokenAsync();
        
        Assert.True(response.ValueKind == JsonValueKind.Object);
        Assert.True(response.TryGetProperty("token", out var tokenProp));
        Assert.True(tokenProp.ValueKind == JsonValueKind.String);
        Assert.True(!string.IsNullOrWhiteSpace(tokenProp.GetString()));
    }

    [Fact]
    public async Task ShouldGenerateTokenWithExpiration()
    {
        var response = await Api.Auth().GenerateTokenAsync();
        
        Assert.True(response.ValueKind == JsonValueKind.Object);
        
        if (response.TryGetProperty("expires_at", out var expiresProp) && expiresProp.ValueKind == JsonValueKind.String)
        {
            var expiresStr = expiresProp.GetString();
            if (!string.IsNullOrWhiteSpace(expiresStr) && DateTime.TryParse(expiresStr, out var expiresAt))
            {
                Assert.True(DateTime.UtcNow < expiresAt);
            }
        }
    }

    [Fact]
    public async Task ShouldThrowWhenRefreshTokenIsEmpty()
    {
        var ex = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Auth().RefreshTokenAsync("", "some_token"));
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task ShouldThrowWhenTokenIsEmptyForRefresh()
    {
        var ex = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Auth().RefreshTokenAsync("refresh_token", ""));
        Assert.NotNull(ex);
    }
}
