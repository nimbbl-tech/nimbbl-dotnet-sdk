using Nimbbl.Sdk.Rest.Exception;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class ExceptionTest
{
    [Fact]
    public void ShouldCreateNimbblException()
    {
        var exception = new NimbblException("Test error", 400, "TEST_ERROR");
        
        Assert.Equal("Test error", exception.Message);
        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("TEST_ERROR", exception.ErrorCode);
    }

    [Fact]
    public void ShouldCreateApiException()
    {
        var exception = new ApiException("API error", 500);
        
        Assert.Equal("API error", exception.Message);
        Assert.Equal(500, exception.StatusCode);
    }

    [Fact]
    public void ShouldCreateAuthenticationException()
    {
        var exception = new AuthenticationException("Auth failed", 401);
        
        Assert.Equal("Auth failed", exception.Message);
        Assert.Equal(401, exception.StatusCode);
    }

    [Fact]
    public void ShouldCreateBadRequestException()
    {
        var exception = new BadRequestException("Bad request", 400);
        
        Assert.Equal("Bad request", exception.Message);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public void ShouldCreateNotFoundException()
    {
        var exception = new NotFoundException("Not found", 404);
        
        Assert.Equal("Not found", exception.Message);
        Assert.Equal(404, exception.StatusCode);
    }

    [Fact]
    public void ShouldCreateRateLimitException()
    {
        var exception = new RateLimitException("Rate limited", 429);
        
        Assert.Equal("Rate limited", exception.Message);
        Assert.Equal(429, exception.StatusCode);
    }

    [Fact]
    public void ShouldCreateServerException()
    {
        var exception = new ServerException("Server error", 500);
        
        Assert.Equal("Server error", exception.Message);
        Assert.Equal(500, exception.StatusCode);
    }
}
