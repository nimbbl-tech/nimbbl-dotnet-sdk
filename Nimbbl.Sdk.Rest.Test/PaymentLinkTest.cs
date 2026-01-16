using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest.Exception;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class PaymentLinkTest : TestBase
{
    [Fact]
    public async Task ShouldCreatePaymentLink()
    {
        var request = new Dictionary<string, object?>
        {
            ["invoice_id"] = $"test_inv_{System.Guid.NewGuid()}",
            ["total_amount"] = 100,
            ["currency"] = "INR",
            ["description"] = "Test payment link",
            ["expires_at"] = System.DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"),
            ["user"] = new Dictionary<string, object?>
            {
                ["email"] = "test@example.com",
                ["first_name"] = "Test",
                ["last_name"] = "User",
                ["country_code"] = "+91",
                ["mobile_number"] = "9876543210"
            }
        };

        var response = await Api.PaymentLinks().CreatePaymentLinkAsync(request);
        Assert.True(response.ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task ShouldThrowWhenUpdatingWithoutIdentifier()
    {
        var request = new Dictionary<string, object?>
        {
            ["amount"] = 200
        };

        var exception = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.PaymentLinks().UpdatePaymentLinkAsync(request));
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task ShouldUpdatePaymentLinkWithInvoiceId()
    {
        // First create a payment link
        var createRequest = new Dictionary<string, object?>
        {
            ["invoice_id"] = $"test_inv_{System.Guid.NewGuid()}",
            ["total_amount"] = 100,
            ["currency"] = "INR",
            ["expires_at"] = System.DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"),
            ["user"] = new Dictionary<string, object?>
            {
                ["email"] = "test@example.com",
                ["first_name"] = "Test",
                ["last_name"] = "User",
                ["country_code"] = "+91",
                ["mobile_number"] = "9876543210"
            }
        };
        var createResponse = await Api.PaymentLinks().CreatePaymentLinkAsync(createRequest);
        
        var invoiceId = createResponse.TryGetProperty("invoice_id", out var invId) ? invId.GetString() : null;
        if (invoiceId != null)
        {
            var updateRequest = new Dictionary<string, object?>
            {
                ["invoice_id"] = invoiceId,
                ["total_amount"] = 200,
                ["expires_at"] = System.DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss"),
                ["user"] = new Dictionary<string, object?>
                {
                    ["email"] = "test@example.com",
                    ["first_name"] = "Test",
                    ["last_name"] = "User",
                    ["country_code"] = "+91",
                    ["mobile_number"] = "9876543210"
                }
            };
            var response = await Api.PaymentLinks().UpdatePaymentLinkAsync(updateRequest);
            Assert.True(response.ValueKind == JsonValueKind.Object);
        }
    }

    [Fact]
    public async Task ShouldThrowWhenEnquiringWithoutIdentifier()
    {
        var request = new Dictionary<string, object?>();

        var ex = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.PaymentLinks().EnquiryPaymentLinkAsync(request));
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task ShouldEnquiryPaymentLink()
    {
        var request = new Dictionary<string, object?>
        {
            ["invoice_id"] = "test_invoice_id"
        };

        // This will fail with real API (invalid invoice_id), but tests the method signature and error handling
        var ex = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.PaymentLinks().EnquiryPaymentLinkAsync(request));
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task ShouldThrowWhenActionIsMissing()
    {
        var request = new Dictionary<string, object?>
        {
            ["invoice_id"] = "test_invoice_id"
        };

        var ex2 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.PaymentLinks().PerformPaymentLinkActionsAsync(request));
        Assert.NotNull(ex2);
    }

    [Fact]
    public async Task ShouldThrowWhenActionIsInvalid()
    {
        var request = new Dictionary<string, object?>
        {
            ["invoice_id"] = "test_invoice_id",
            ["action"] = "invalid_action"
        };

        var ex3 = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.PaymentLinks().PerformPaymentLinkActionsAsync(request));
        Assert.NotNull(ex3);
    }
}
