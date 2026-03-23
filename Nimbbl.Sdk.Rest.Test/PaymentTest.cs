using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest.Exception;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class PaymentTest : TestBase
{
    [Fact]
    public async Task ShouldInitiatePayment()
    {
        // First create an order
        var orderRequest = CreateTestOrderRequest();
        var orderResponse = await Api.Orders().CreateOrderAsync(orderRequest);
        var orderId = orderResponse.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
        Assert.NotNull(orderId);

        var banksResponse = await Api.CheckoutUtilities().ListBanksAsync(new Dictionary<string, object?>
        {
            ["order_id"] = orderId
        });
        var bankCode = "hdfc";
        if (banksResponse.TryGetProperty("bank_list", out var bankList) && bankList.ValueKind == JsonValueKind.Array)
        {
            foreach (var bank in bankList.EnumerateArray())
            {
                if (bank.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == JsonValueKind.String)
                {
                    var codeValue = codeProp.GetString();
                    if (!string.IsNullOrWhiteSpace(codeValue))
                    {
                        bankCode = codeValue;
                        break;
                    }
                }
            }
        }

        // Then initiate payment
        var paymentRequest = new Dictionary<string, object?>
        {
            ["order_id"] = orderId,
            ["payment_mode_code"] = "net_banking",
            ["bank_code"] = bankCode,
            ["callback_url"] = "https://example.com/callback"
        };

        var response = await Api.Payments().InitiatePaymentAsync(paymentRequest);
        Assert.True(response.ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task ShouldCompletePayment()
    {
        var request = new Dictionary<string, object?>
        {
            ["order_id"] = "test_order_id",
            ["transaction_id"] = "test_transaction_id"
        };

        // This will fail with real API (invalid order_id), but tests the method signature and error handling
        // NotFoundException inherits from NimbblException, so this should work
        var exception = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Payments().CompletePaymentAsync(request));
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task ShouldResendPaymentOtp()
    {
        var request = new Dictionary<string, object?>
        {
            ["order_id"] = "test_order_id",
            ["transaction_id"] = "test_transaction_id"
        };

        // This will fail with real API (invalid order_id), but tests the method signature and error handling
        // NotFoundException inherits from NimbblException, so this should work
        var exception = await Assert.ThrowsAnyAsync<NimbblException>(() => 
            Api.Payments().ResendPaymentOtpAsync(request));
        Assert.NotNull(exception);
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
