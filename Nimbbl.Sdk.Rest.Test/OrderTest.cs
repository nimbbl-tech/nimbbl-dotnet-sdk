using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class OrderTest
{
    private readonly IOrders _orders;

    public OrderTest()
    {
        var config = new Config("https://api.nimbbl.tech/api/", "access_key_1MwvMkKkweorz0ry", "access_secret_81x7ByYkRpB4g05N");
        _orders = new NimbblClient(config).Orders;
    }

    [Fact]
    public async Task ShouldCreateOrder()
    {
        var orderRequest = GivenNewOrderRequest();
        var orderCreatedResponse = await _orders.CreateOrderAsync(orderRequest);
        Assert.True(orderCreatedResponse.ValueKind == JsonValueKind.Object);
        
        var orderId = orderCreatedResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);
        
        var invoiceId = orderCreatedResponse.TryGetProperty("invoice_id", out var iid) ? iid.GetString() : null;
        Assert.Equal(orderRequest.InvoiceId, invoiceId);
        
        var orderLineItems = orderCreatedResponse.TryGetProperty("order_line_items", out var items) && items.ValueKind == JsonValueKind.Array 
            ? items.GetArrayLength() : 0;
        Assert.Equal(1, orderLineItems); // We created one item
    }

    [Fact]
    public async Task ShouldCreateAndRetrieveOrder()
    {
        var orderRequest = GivenNewOrderRequest();
        var response = await _orders.CreateOrderAsync(orderRequest);
        var orderId = response.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);
        
        var orderResponse = await _orders.GetOrderByIdAsync(orderId!);
        var retrievedOrderId = orderResponse.TryGetProperty("order_id", out var roid) ? roid.GetString() : null;
        Assert.Equal(orderId, retrievedOrderId);
        
        var invoiceId = orderResponse.TryGetProperty("invoice_id", out var iid) ? iid.GetString() : null;
        Assert.Equal(orderRequest.InvoiceId, invoiceId);
    }

    private static OrderRequest GivenNewOrderRequest()
    {
        return new()
        {
            AmountBeforeTax = 2,
            Currency = "INR",
            InvoiceId = UniqueId(),
            OrderDate = DateTime.UtcNow,
            Tax = 0,
            TotalAmount = 2,
            ReferrerPlatform = "woocommerce",
            ReferrerPlatformIdentifier = "test-sdk",
            MerchantShopfrontDomain = "http://example.com",
            OrderLineItems = new[]
            {
                new OrderLineItem()
                {
                    SkuId = "sku1",
                    Title = "Colourful Mandalas",
                    Description = "Convert your dreary device into a bright happy place with this wallpaper.",
                    Quantity = 1,
                    Rate = 2,
                    AmountBeforeTax = 2,
                    Tax = 0,
                    TotalAmount = 2,
                    ImageUrl = "https=//cdn.pixabay.com/photo/2015/12/09/01/02/mandalas-1084082_960_720.jpg"
                }
            },
            User = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                CountryCode = "+91",
                MobileNumber = "9876543210"
            },
            ShippingAddress = new ShippingAddress
            {
                Address1 = "123 Test St",
                Street = "Test Street",
                Landmark = "",
                Area = "Test Area",
                City = "Mumbai",
                State = "Maharashtra",
                Pincode = "400001",
                AddressType = "home"
            },
            Description = "Test order"
        };
    }

    private static string UniqueId()
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("n")));
    }
}