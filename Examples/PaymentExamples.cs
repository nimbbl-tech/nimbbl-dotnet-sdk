using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.Common;

namespace Examples;

/// <summary>
/// Payment Examples
/// </summary>
public static class PaymentExamples
{
    /// <summary>
    /// Initiate Payment - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task InitiatePaymentExample(NimbblApi api)
    {
        var token = Helpers.GetInput("Enter Order Token: ", false);
        if (string.IsNullOrWhiteSpace(token))
        {
            Helpers.PrintError("Order Token is required.\n");
            return;
        }
        
        api.SetBearerToken(token);
        
        var orderId = Helpers.GetInput("Enter Order ID: ");
        if (string.IsNullOrWhiteSpace(orderId))
        {
            Helpers.PrintError("Order ID is required.\n");
            return;
        }
        
        var callbackUrl = Helpers.GetInput("Enter Callback URL (mandatory): ");
        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            Helpers.PrintError("Callback URL is required.\n");
            return;
        }
        
        var paymentMode = Helpers.GetInput("Enter Payment Mode Code (net_banking/credit_card/etc): ", false) ?? CheckoutConstants.PaymentModeNetBanking;
        
        var data = new Dictionary<string, object?>
        {
            ["order_id"] = orderId,
            [CheckoutConstants.OptionKeyPaymentModeCode] = paymentMode,
            ["callback_url"] = callbackUrl
        };
        
        // Bank code is mandatory for net_banking
        if (paymentMode.Equals(CheckoutConstants.PaymentModeNetBanking, StringComparison.OrdinalIgnoreCase))
        {
            Helpers.PrintInfo("Bank Code is required for net_banking payment mode.\n");
            var bankCode = Helpers.GetInput("Enter Bank Code (default: HDFC): ", false) ?? "HDFC";
            data[CheckoutConstants.OptionKeyBankCode] = bankCode;
        }
        else
        {
            var bankCode = Helpers.GetInput("Enter Bank Code (optional, for net_banking only): ", false);
            if (!string.IsNullOrWhiteSpace(bankCode))
            {
                data[CheckoutConstants.OptionKeyBankCode] = bankCode;
            }
        }
        
        try
        {
            var result = await api.Payments().InitiatePaymentAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Payment initiated successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    /// <summary>
    /// Complete Payment - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task CompletePaymentExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            var transactionId = Helpers.GetInput("Enter Transaction ID: ");
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                Helpers.PrintError("Transaction ID is required.\n");
                return;
            }
            
            var paymentFlow = Helpers.GetInput("Enter Payment Flow (auto_debit/otp): ", false) ?? "auto_debit";
            var data = new Dictionary<string, object?>
            {
                ["transaction_id"] = transactionId,
                [CheckoutConstants.OptionKeyPaymentFlow] = paymentFlow
            };
            
            if (paymentFlow.Equals("otp", StringComparison.OrdinalIgnoreCase))
            {
                var otp = Helpers.GetInput("Enter OTP: ", false);
                if (!string.IsNullOrWhiteSpace(otp))
                {
                    data["otp"] = otp;
                }
            }
            
            var result = await api.Payments().CompletePaymentAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Payment completed successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    /// <summary>
    /// Resend OTP - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task ResendOtpExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            var transactionId = Helpers.GetInput("Enter Transaction ID: ");
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                Helpers.PrintError("Transaction ID is required.\n");
                return;
            }
            
            var data = new Dictionary<string, object?>
            {
                ["transaction_id"] = transactionId
            };
            
            var result = await api.Payments().ResendPaymentOtpAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("OTP resent successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }
}
