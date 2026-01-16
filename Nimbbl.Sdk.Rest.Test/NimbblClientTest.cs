using System;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest.Api;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class NimbblClientTest : TestBase
{
    [Fact]
    public void ShouldInitializeNimbblApi()
    {
        Assert.NotNull(Api);
        Assert.NotNull(Api.Orders());
        Assert.NotNull(Api.Transactions());
        Assert.NotNull(Api.Payments());
        Assert.NotNull(Api.PaymentLinks());
        Assert.NotNull(Api.Addresses());
        Assert.NotNull(Api.Refunds());
        Assert.NotNull(Api.CheckoutUtilities());
        Assert.NotNull(Api.Auth());
    }

    [Fact]
    public void ShouldAddCustomHeader()
    {
        // Read from environment variables (required)
        var accessKey = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY");
        var accessSecret = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET");
        var ApiHost = Environment.GetEnvironmentVariable("NIMBBL_API_HOST");
        
        if (string.IsNullOrWhiteSpace(accessKey))
            throw new InvalidOperationException("NIMBBL_ACCESS_KEY environment variable is required");
        if (string.IsNullOrWhiteSpace(accessSecret))
            throw new InvalidOperationException("NIMBBL_ACCESS_SECRET environment variable is required");
        
        var Api = NimbblApi.Initialize(
            accessKey!,
            accessSecret!,
            ApiHost
        );

        Api.AddHeader("X-Custom-Header", "test-value");
        // Header should be added without exception
        Assert.True(true);
    }

    [Fact]
    public void ShouldSetBearerToken()
    {
        // Read from environment variables (required)
        var accessKey = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY");
        var accessSecret = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET");
        var ApiHost = Environment.GetEnvironmentVariable("NIMBBL_API_HOST");
        
        if (string.IsNullOrWhiteSpace(accessKey))
            throw new InvalidOperationException("NIMBBL_ACCESS_KEY environment variable is required");
        if (string.IsNullOrWhiteSpace(accessSecret))
            throw new InvalidOperationException("NIMBBL_ACCESS_SECRET environment variable is required");
        
        var Api = NimbblApi.Initialize(
            accessKey!,
            accessSecret!,
            ApiHost
        );

        Api.SetBearerToken("test_token");
        // Token should be set without exception
        Assert.True(true);
    }

    [Fact]
    public void ShouldDisposeNimbblApi()
    {
        // Read from environment variables (required)
        var accessKey = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY");
        var accessSecret = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET");
        var ApiHost = Environment.GetEnvironmentVariable("NIMBBL_API_HOST");
        
        if (string.IsNullOrWhiteSpace(accessKey))
            throw new InvalidOperationException("NIMBBL_ACCESS_KEY environment variable is required");
        if (string.IsNullOrWhiteSpace(accessSecret))
            throw new InvalidOperationException("NIMBBL_ACCESS_SECRET environment variable is required");
        
        var Api = NimbblApi.Initialize(
            accessKey!,
            accessSecret!,
            ApiHost
        );

        Api.Dispose();
        // Should dispose without exception
        Assert.True(true);
    }
}