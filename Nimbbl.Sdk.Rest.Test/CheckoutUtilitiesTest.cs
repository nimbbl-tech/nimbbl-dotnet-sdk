using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest.Exception;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class CheckoutUtilitiesTest : TestBase
{
    [Fact]
    public async Task ShouldListPaymentModes()
    {
        // First create an order
        var orderRequest = CreateTestOrderRequest();
        var orderResponse = await Api.Orders().CreateOrderAsync(orderRequest);
        var orderId = orderResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);

        var request = new Dictionary<string, object?>
        {
            ["order_id"] = orderId
        };

        var response = await Api.CheckoutUtilities().ListPaymentModesAsync(request);
        Assert.True(response.ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task ShouldListBanks()
    {
        var request = new Dictionary<string, object?>
        {
            ["order_id"] = "test_order_id"
        };

        // This will fail with real API (invalid order_id), but tests the method signature
        await Assert.ThrowsAsync<NotFoundException>(() => 
            Api.CheckoutUtilities().ListBanksAsync(request));
    }

    [Fact]
    public async Task ShouldListWallets()
    {
        var request = new Dictionary<string, object?>
        {
            ["order_id"] = "test_order_id"
        };

        // This will fail with real API (invalid order_id), but tests the method signature
        await Assert.ThrowsAsync<NotFoundException>(() => 
            Api.CheckoutUtilities().ListWalletsAsync(request));
    }

    [Fact]
    public async Task ShouldListBanksWithEncryptionCheck()
    {
        // This test verifies list banks (respecting ENCRYPT_PAYLOAD env flag)
        // by first creating a fresh order to ensure a valid order_id exists.
        
        var orderRequest = OrderTest.GivenNewOrderRequest();
        var orderResponse = await Api.Orders().CreateOrderAsync(orderRequest);
        var orderId = orderResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);
        
        var listBanksRequest = new Dictionary<string, object?>
        {
            ["order_id"] = orderId
        };
        
        var banksResponse = await Api.CheckoutUtilities().ListBanksAsync(listBanksRequest);
        Assert.True(banksResponse.ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task ShouldListWalletsWithEncryptionCheck()
    {
        // This test verifies list wallets (respecting ENCRYPT_PAYLOAD env flag)
        // by first creating a fresh order to ensure a valid order_id exists.
        
        var orderRequest = OrderTest.GivenNewOrderRequest();
        var orderResponse = await Api.Orders().CreateOrderAsync(orderRequest);
        var orderId = orderResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);
        
        var listWalletsRequest = new Dictionary<string, object?>
        {
            ["order_id"] = orderId
        };
        
        var walletsResponse = await Api.CheckoutUtilities().ListWalletsAsync(listWalletsRequest);
        Assert.True(walletsResponse.ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task ShouldListEmis()
    {
        var request = new Dictionary<string, object?>
        {
            ["order_id"] = "test_order_id",
            ["amount"] = 1000
        };

        // This will fail with real API (invalid order_id or missing required params), but tests the method signature
        var ex = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.CheckoutUtilities().ListEmisAsync(request));
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task ShouldGetOffers()
    {
        var request = new Dictionary<string, object?>
        {
            ["order_id"] = "test_order_id"
        };

        // This will fail with real API (invalid order_id), but tests the method signature
        await Assert.ThrowsAsync<NotFoundException>(() => 
            Api.CheckoutUtilities().GetOffersAsync(request));
    }

    [Fact]
    public async Task ShouldGetCardBinData()
    {
        var request = new Dictionary<string, object?>
        {
            ["card_bin"] = "411111"
        };

        var response = await Api.CheckoutUtilities().GetCardBinDataAsync(request);
        Assert.True(response.ValueKind == JsonValueKind.Object || response.ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task ShouldGetCardDetails()
    {
        var request = new Dictionary<string, object?>
        {
            ["encrypted_card_details"] = "test_encrypted_data"
        };

        // This will fail with real API (invalid encrypted data), but tests the method signature
        await Assert.ThrowsAsync<NotFoundException>(() => 
            Api.CheckoutUtilities().GetCardDetailsAsync(request));
    }

    [Fact]
    public async Task ShouldValidateUpiVpa()
    {
        var request = new Dictionary<string, object?>
        {
            ["upi_id"] = "test@upi"
        };

        var response = await Api.CheckoutUtilities().ValidateUpiVpaAsync(request);
        Assert.True(response.ValueKind == JsonValueKind.Object || response.ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task ShouldGetUpiAppDetails()
    {
        var request = new Dictionary<string, object?>
        {
            ["order_id"] = "test_order_id"
        };

        // This will fail with real API (missing required params), but tests the method signature
        var ex2 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.CheckoutUtilities().GetUpiAppDetailsAsync(request));
        Assert.NotNull(ex2);
    }

    private static Dictionary<string, object?> CreateTestOrderRequest()
    {
        return new Dictionary<string, object?>
        {
            ["amount_before_tax"] = 100,
            ["currency"] = "INR",
            ["invoice_id"] = $"test_inv_{System.Guid.NewGuid()}",
            ["order_date"] = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["tax"] = 0,
            ["total_amount"] = 100,
            ["user"] = new Dictionary<string, object?>
            {
                ["email"] = "test@example.com",
                ["first_name"] = "Test",
                ["last_name"] = "User",
                ["country_code"] = "+91",
                ["mobile_number"] = "9876543210"
            }
        };
    }
}
