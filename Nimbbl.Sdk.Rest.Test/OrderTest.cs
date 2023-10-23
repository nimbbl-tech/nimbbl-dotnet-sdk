using System;
using System.Text;
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
        var orderCreatedResponse = await _orders.CreateAsync(orderRequest);
        Assert.NotNull(orderCreatedResponse);
        AssertOrderResponseMatches(GivenOrderCreatedResponseWithInvoiceId(orderRequest.InvoiceId), orderCreatedResponse);
        Assert.Empty(orderCreatedResponse.OrderLineItem);
    }

    [Fact]
    public async Task ShouldCreateAndRetrieveOrder()
    {
        var orderRequest = GivenNewOrderRequest();
        var response = await _orders.CreateAsync(orderRequest);
        var orderResponse = await _orders.GetByIdAsync(response.OrderId);
        Assert.Equal(response.OrderId, orderResponse.OrderId);
        AssertOrderResponseMatches(GivenOrderCreatedResponseWithInvoiceId(orderRequest.InvoiceId), orderResponse);
    }


    private void AssertOrderResponseMatches(Order expected, Order response)
    {
        Assert.NotNull(response.OrderId);
        Assert.Equal(expected.SubMerchantId, response.SubMerchantId);
        Assert.True((DateTime.UtcNow - DateTime.Parse(response.OrderDate)).TotalMinutes < 1);
        Assert.Equal(expected.AmountBeforeTax, response.AmountBeforeTax);
        Assert.Equal(expected.Tax, response.Tax);
        Assert.Equal(expected.TotalAmount, response.TotalAmount);
        Assert.Equal(expected.ReferrerPlatform, response.ReferrerPlatform);
        Assert.Equal(expected.ReferrerPlatformVersion, response.ReferrerPlatformVersion);
        Assert.Equal(expected.InvoiceId, response.InvoiceId);
        Assert.Equal(expected.MerchantShopfrontDomain, response.MerchantShopfrontDomain);
        Assert.Equal(expected.Attempts, response.Attempts);
        Assert.Equal(expected.DeviceUserAgent, response.DeviceUserAgent);
        Assert.Equal(expected.Status, response.Status);
        Assert.Equal(expected.Currency, response.Currency);
        Assert.Equal(expected.MaxRetries, response.MaxRetries);
        Assert.Equal(expected.AdditionalCharges, response.AdditionalCharges);
        Assert.Equal(expected.GrandTotalAmount, response.GrandTotalAmount);
        AssertSubMerchantMatches(expected.SubMerchant, response.SubMerchant);
    }

    private void AssertSubMerchantMatches(SubMerchant expected, SubMerchant response)
    {
        Assert.Equal(expected.SubMerchantId, response.SubMerchantId);
        Assert.Equal(expected.Sandbox, response.Sandbox);
        Assert.Equal(expected.Description, response.Description);
    }

    private static OrderRequest GivenNewOrderRequest()
    {
        return new()
        {
            AmountBeforeTax = 2,
            Currency = "INR",
            InvoiceId = UniqueId(),
            Tax = 0,
            TotalAmount = 2,
            ReferrerPlatform = "woocommerce",
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
                    TotalAmount = 2,
                    ImageUrl = "https=//cdn.pixabay.com/photo/2015/12/09/01/02/mandalas-1084082_960_720.jpg"
                }
            },
            Description = "Test order"
        };
    }

    private static Order GivenOrderCreatedResponseWithInvoiceId(string invoiceId)
    {
        return new()
        {
            OrderId = "o_NYP0EVOZy5b6R7GD",
            SubMerchantId = 2,
            AmountBeforeTax = 2,
            Tax = 0,
            TotalAmount = 2,
            InvoiceId = invoiceId,
            MerchantShopfrontDomain = "http://example.com",
            Attempts = 0,
            Status = "new",
            Currency = "INR",
            MaxRetries = 15,
            BrowserName = "Other",
            DeviceName = "Other",
            OsName = "Other",
            AdditionalCharges = 0,
            GrandTotalAmount = 2,
            ReferrerPlatform = "woocommerce",
            OrderLineItem = Array.Empty<OrderLineItem>(),
            SubMerchant = new()
            {
                SubMerchantId = "303151",
                Sandbox = "N",
                Description = "Nimbbl (Razorpay)"
            }
        };
    }
    private static string UniqueId()
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("n")));
    }
}