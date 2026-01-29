using System.Text.Json;
using Nimbbl.Sdk.Rest;
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
        // Merchant token is automatically generated and used for authentication
        var orderId = Helpers.GetInput("Enter Order ID: ");
        if (string.IsNullOrWhiteSpace(orderId))
        {
            Helpers.PrintError("Order ID is required.\n");
            return;
        }
        
        var callbackUrl = Helpers.GetInput("Enter Callback URL (optional): ", false);
        
        var paymentMode = Helpers.GetInput("Enter Payment Mode Code (net_banking/credit_card/etc): ", false) ?? "net_banking";
        
        var data = new Dictionary<string, object?>
        {
            ["order_id"] = orderId,
            [CheckoutConstants.OptionKeyPaymentModeCode] = paymentMode,
            ["callback_url"] = callbackUrl
        };
        
        // Payment mode specific data
        switch (paymentMode.ToLower())
        {
            case "net_banking":
                Helpers.PrintInfo("Bank Code is required for net_banking payment mode.\n");
                var bankCode = Helpers.GetInput("Enter Bank Code (default: HDFC): ", false) ?? "HDFC";
                data[CheckoutConstants.OptionKeyBankCode] = bankCode;
                break;
            case "upi":
                var paymentFlow = Helpers.GetInput("Enter UPI Payment Flow (intent/collect): ", false) ?? "intent";
                data["payment_flow"] = paymentFlow;
                
                var upiId = Helpers.GetInput("Enter UPI ID (optional): ", false);
                if (!string.IsNullOrWhiteSpace(upiId))
                {
                    data["upi_id"] = upiId;
                }
                
                var upiAppCode = Helpers.GetInput("Enter UPI App Code (gpay/phonepe/paytm - optional): ", false);
                if (!string.IsNullOrWhiteSpace(upiAppCode))
                {
                    data["upi_app_code"] = upiAppCode;
                }
                break;
            case "wallet":
                var walletCode = Helpers.GetInput("Enter Wallet Code (e.g., FREECHARGE): ");
                if (string.IsNullOrWhiteSpace(walletCode))
                {
                    Helpers.PrintError("Wallet Code is required for wallet payment mode.\n");
                    return;
                }
                data["wallet_code"] = walletCode;
                break;
            case "credit_card":
            case "debit_card": // Often share similar fields
                var cardNo = Helpers.GetInput("Enter Card Number: ");
                if (string.IsNullOrWhiteSpace(cardNo))
                {
                    Helpers.PrintError("Card Number is required.\n");
                    return;
                }
                data["card_no"] = cardNo;
                
                var cardInputType = Helpers.GetInput("Enter Card Input Type (card_pan/token - default: card_pan): ", false) ?? "card_pan";
                data["card_input_type"] = cardInputType;
                
                var cvv = Helpers.GetInput("Enter CVV: ");
                if (string.IsNullOrWhiteSpace(cvv))
                {
                    Helpers.PrintError("CVV is required.\n");
                    return;
                }
                data["cvv"] = cvv;
                
                var cardHolderName = Helpers.GetInput("Enter Card Holder Name: ");
                if (!string.IsNullOrWhiteSpace(cardHolderName))
                {
                    data["card_holder_name"] = cardHolderName;
                }
                
                var expiry = Helpers.GetInput("Enter Expiry (MM/YY): ");
                if (string.IsNullOrWhiteSpace(expiry))
                {
                    Helpers.PrintError("Expiry is required (MM/YY format).\n");
                    return;
                }
                data["expiry"] = expiry;
                break;
            default:
                Helpers.PrintInfo($"No specific additional data required for {paymentMode} mode.\n");
                break;
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
            // Merchant token is automatically generated and used for authentication
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
            // Merchant token is automatically generated and used for authentication
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
