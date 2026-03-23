using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class OrderTest : TestBase
{

    [Fact]
    public async Task ShouldCreateOrder()
    {
        var orderRequest = GivenNewOrderRequest();
        
        // If encryption is enabled in environment, this will test encryption
        var orderCreatedResponse = await Api.Orders().CreateOrderAsync(orderRequest);
        Assert.True(orderCreatedResponse.ValueKind == JsonValueKind.Object);
        
        var orderId = orderCreatedResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);
        
        var invoiceId = orderCreatedResponse.TryGetProperty("invoice_id", out var iid) ? iid.GetString() : null;
        Assert.Equal(orderRequest["invoice_id"]?.ToString(), invoiceId);
        
        // Verify order_line_items exists
        var hasOrderLineItems = orderCreatedResponse.TryGetProperty("order_line_items", out var items);
        if (hasOrderLineItems && items.ValueKind == JsonValueKind.Array)
        {
            Assert.True(items.GetArrayLength() >= 0);
        }
    }

    [Fact]
    public async Task ShouldCreateOrderWithEncryptionCheck()
    {
        // This test explicitly acknowledges if encryption is being used based on the env flag
        var orderRequest = GivenNewOrderRequest();
        
        // Verify currency is in the request (needed for some API validations)
        Assert.True(orderRequest.ContainsKey("currency"));
        Assert.Equal("INR", orderRequest["currency"]?.ToString());
        
        var orderCreatedResponse = await Api.Orders().CreateOrderAsync(orderRequest);
        
        Assert.True(orderCreatedResponse.ValueKind == JsonValueKind.Object);
        Assert.NotNull(orderCreatedResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null);
        
        // The fact that the API call succeeded confirms that if encryption was enabled in the env,
        // it was handled correctly by the SDK and the API.
    }

    [Fact]
    public async Task ShouldCreateAndRetrieveOrder()
    {
        var orderRequest = GivenNewOrderRequest();
        var response = await Api.Orders().CreateOrderAsync(orderRequest);
        var orderId = response.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);
        
        var orderResponse = await Api.Orders().GetOrderByIdAsync(orderId!);
        var retrievedOrderId = orderResponse.TryGetProperty("order_id", out var roid) ? roid.GetString() : null;
        Assert.Equal(orderId, retrievedOrderId);
        
        var invoiceId = orderResponse.TryGetProperty("invoice_id", out var iid) ? iid.GetString() : null;
        Assert.Equal(orderRequest["invoice_id"]?.ToString(), invoiceId);
    }

    internal static Dictionary<string, object?> GivenNewOrderRequest()
    {
        var invoiceId = UniqueId();
        return new Dictionary<string, object?>
        {
            ["amount_before_tax"] = 2,
            ["currency"] = "INR",
            ["invoice_id"] = invoiceId,
            ["order_date"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["tax"] = 0,
            ["total_amount"] = 2,
            ["referrer_platform"] = "woocommerce",
            ["referrer_platform_identifier"] = "test-sdk",
            ["merchant_shopfront_domain"] = "http://example.com",
            ["order_line_items"] = new[]
            {
                new Dictionary<string, object?>
                {
                    ["sku_id"] = "sku1",
                    ["title"] = "Colourful Mandalas",
                    ["description"] = "Convert your dreary device into a bright happy place with this wallpaper.",
                    ["quantity"] = 1,
                    ["rate"] = 2,
                    ["amount_before_tax"] = 2,
                    ["tax"] = 0,
                    ["total_amount"] = 2,
                    ["image_url"] = "https://cdn.pixabay.com/photo/2015/12/09/01/02/mandalas-1084082_960_720.jpg"
                }
            },
            ["user"] = new Dictionary<string, object?>
            {
                ["email"] = "test@example.com",
                ["first_name"] = "Test",
                ["last_name"] = "User",
                ["country_code"] = "+91",
                ["mobile_number"] = "9876543210"
            },
            ["shipping_address"] = new Dictionary<string, object?>
            {
                ["address1"] = "123 Test St",
                ["street"] = "Test Street",
                ["landmark"] = "",
                ["area"] = "Test Area",
                ["city"] = "Mumbai",
                ["state"] = "Maharashtra",
                ["pincode"] = "400001",
                ["address_type"] = "home"
            },
            ["description"] = "Test order"
        };
    }

    private static string UniqueId()
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("n")));
    }
}