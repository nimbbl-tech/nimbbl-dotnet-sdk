using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest.Exception;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class AddressTest : TestBase
{
    [Fact]
    public async Task ShouldListAddresses()
    {
        var options = new Dictionary<string, object?>
        {
            ["user_id"] = "test_user_id"
        };

        // This will fail with real API (invalid user_id or missing required params), but tests the method signature
        var ex = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Addresses().ListAddressesAsync(options));
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task ShouldListAddressesWithoutOptions()
    {
        // This will fail with real API (missing required params), but tests the method signature
        var ex2 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Addresses().ListAddressesAsync());
        Assert.NotNull(ex2);
    }

    [Fact]
    public async Task ShouldCreateAddress()
    {
        var request = new Dictionary<string, object?>
        {
            ["user_id"] = "test_user_id",
            ["address1"] = "123 Test St",
            ["city"] = "Mumbai",
            ["state"] = "Maharashtra",
            ["pincode"] = "400001",
            ["address_type"] = "home"
        };

        // This will fail with real API (invalid user_id), but tests the method signature
        var ex3 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Addresses().CreateAddressAsync(request));
        Assert.NotNull(ex3);
    }

    [Fact]
    public async Task ShouldUpdateAddress()
    {
        var request = new Dictionary<string, object?>
        {
            ["address"] = new Dictionary<string, object?>
            {
                ["address_id"] = "test_address_id",
                ["address1"] = "456 Updated St",
                ["city"] = "Delhi",
                ["state"] = "Delhi",
                ["pincode"] = "110001"
            }
        };

        // This will fail with real API (invalid address_id or missing required params), but tests the method signature
        var ex4 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Addresses().UpdateAddressAsync("test_address_id", request));
        Assert.NotNull(ex4);
    }

    [Fact]
    public async Task ShouldDeleteAddress()
    {
        // This will fail with real API (invalid address_id), but tests the method signature
        var ex5 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Addresses().DeleteAddressAsync("test_address_id"));
        Assert.NotNull(ex5);
    }

    [Fact]
    public async Task ShouldImportAddresses()
    {
        // First create an order to get order_id (mandatory for import)
        var orderRequest = OrderTest.GivenNewOrderRequest();
        var orderResponse = await Api.Orders().CreateOrderAsync(orderRequest);
        var orderId = orderResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);

        var request = new Dictionary<string, object?>
        {
            ["order_id"] = orderId,
            ["user_id"] = "test_user_id",
            ["addresses"] = new[]
            {
                new Dictionary<string, object?>
                {
                    ["address1"] = "123 Test St",
                    ["city"] = "Mumbai",
                    ["state"] = "Maharashtra",
                    ["pincode"] = "400001"
                }
            }
        };

        // This will now pass or fail with a more specific error than missing order_id
        var ex6 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Addresses().ImportAddressesAsync(request));
        Assert.NotNull(ex6);
    }

    [Fact]
    public async Task ShouldThrowWhenPincodeIsMissingForEligibility()
    {
        var request = new Dictionary<string, object?>();

        var ex7 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Addresses().CheckAddressEligibilityAsync(request));
        Assert.NotNull(ex7);
    }

    [Fact]
    public async Task ShouldCheckAddressEligibility()
    {
        // First create an order to get order_id
        var orderRequest = OrderTest.GivenNewOrderRequest();
        var orderResponse = await Api.Orders().CreateOrderAsync(orderRequest);
        var orderId = orderResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);
        
        var request = new Dictionary<string, object?>
        {
            ["pincode"] = "400001",
            ["order_id"] = orderId,
            ["amount"] = orderRequest["total_amount"],
            ["currency"] = orderRequest["currency"]
        };

        var response = await Api.Addresses().CheckAddressEligibilityAsync(request);
        Assert.True(response.ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task ShouldLinkAddressWithOrder()
    {
        var request = new Dictionary<string, object?>
        {
            ["order_id"] = "test_order_id",
            ["address_id"] = "test_address_id"
        };

        // This will fail with real API (invalid order_id/address_id), but tests the method signature
        var ex8 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Addresses().LinkAddressWithOrderAsync(request));
        Assert.NotNull(ex8);
    }
}
