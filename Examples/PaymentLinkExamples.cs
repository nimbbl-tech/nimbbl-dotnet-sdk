using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Exception;

namespace Examples;

/// <summary>
/// Payment Link Examples
/// </summary>
public static class PaymentLinkExamples
{
    /// <summary>
    /// Create Payment Link - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task RunAllExamples(NimbblApi api)
    {
        Helpers.PrintHeader("=== Payment Links API Examples ===");
        
        Helpers.PrintStep(1, "Create Payment Link");
        await CreatePaymentLinkExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(2, "Update Payment Link");
        await UpdatePaymentLinkExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(3, "Payment Link Enquiry");
        await EnquiryPaymentLinkExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(4, "Payment Link Actions");
        await PerformPaymentLinkActionsExample(api);
        
        Console.WriteLine("\nFor more information, see: https://nimbbl.biz/docs/category/api-reference/payment-link/\n");
    }

    public static async Task CreatePaymentLinkExample(NimbblApi api)
    {
        try
        {
            // Generate merchant token
            var tokenResponse = await api.Auth().GenerateTokenAsync();
            var merchantToken = tokenResponse.TryGetProperty("token", out var tokenProp) 
                ? tokenProp.GetString() 
                : null;
            
            if (string.IsNullOrWhiteSpace(merchantToken))
            {
                Helpers.PrintError("Failed to generate merchant token.\n");
                return;
            }
            
            api.SetBearerToken(merchantToken);
            
            Helpers.PrintInfo("Enter payment link details:\n");
            var invoiceId = Helpers.GetInput("Enter Invoice ID: ");
            if (string.IsNullOrWhiteSpace(invoiceId))
            {
                Helpers.PrintError("Invoice ID is required.\n");
                return;
            }
            
            var totalAmountStr = Helpers.GetInput("Enter Total Amount: ");
            if (string.IsNullOrWhiteSpace(totalAmountStr))
            {
                Helpers.PrintError("Total Amount is required.\n");
                return;
            }
            
            var currency = Helpers.GetInput("Enter Currency (INR/USD/etc): ", false) ?? "INR";
            
            // User details
            Helpers.PrintInfo("\nUser Details:\n");
            var email = Helpers.GetInput("Enter User Email: ");
            var firstName = Helpers.GetInput("Enter User First Name: ");
            var lastName = Helpers.GetInput("Enter User Last Name: ", false);
            var countryCode = Helpers.GetInput("Enter Country Code (e.g., +91): ", false) ?? "+91";
            var mobileNumber = Helpers.GetInput("Enter Mobile Number: ");
            
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName) || 
                string.IsNullOrWhiteSpace(mobileNumber))
            {
                Helpers.PrintError("Email, First Name, and Mobile Number are required.\n");
                return;
            }
            
            // Required field: expires_at
            Helpers.PrintInfo("\nRequired Field:\n");
            var expiresAt = Helpers.GetInput("Expires At (YYYY-MM-DD HH:MM:SS, required, UTC): ");
            if (string.IsNullOrWhiteSpace(expiresAt))
            {
                Helpers.PrintError("Expires At is required.\n");
                return;
            }
            
            // Build request data
            var data = new Dictionary<string, object?>
            {
                ["invoice_id"] = invoiceId,
                ["total_amount"] = decimal.Parse(totalAmountStr),
                ["currency"] = currency,
                ["expires_at"] = expiresAt,
                ["user"] = new Dictionary<string, object?>
                {
                    ["email"] = email,
                    ["first_name"] = firstName,
                    ["country_code"] = countryCode,
                    ["mobile_number"] = mobileNumber
                }
            };
            
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                ((Dictionary<string, object?>)data["user"]!)["last_name"] = lastName;
            }
            
            var result = await api.PaymentLinks().CreatePaymentLinkAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Payment link created successfully!\n");
                if (result.TryGetProperty("payment_link_id", out var plid))
                {
                    Console.WriteLine($"  Payment Link ID: {plid.GetString()}");
                }
                if (result.TryGetProperty("short_url", out var url))
                {
                    Console.WriteLine($"  Short URL: {url.GetString()}");
                }
                if (!result.TryGetProperty("success", out var success) || !success.GetBoolean())
                {
                    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task UpdatePaymentLinkExample(NimbblApi api)
    {
        try
        {
            // Generate merchant token
            var tokenResponse = await api.Auth().GenerateTokenAsync();
            var merchantToken = tokenResponse.TryGetProperty("token", out var tokenProp) 
                ? tokenProp.GetString() 
                : null;
            
            if (string.IsNullOrWhiteSpace(merchantToken))
            {
                Helpers.PrintError("Failed to generate merchant token.\n");
                return;
            }
            
            api.SetBearerToken(merchantToken);
            
            Helpers.PrintInfo("Identify payment link using:\n");
            Console.WriteLine("1. Invoice ID\n");
            Console.WriteLine("2. Payment Link ID\n");
            var identifierType = Helpers.GetInput("Choose (1 or 2): ");
            
            var data = new Dictionary<string, object?>();
            if (identifierType == "1")
            {
                var invoiceId = Helpers.GetInput("Enter Invoice ID: ");
                if (string.IsNullOrWhiteSpace(invoiceId))
                {
                    Helpers.PrintError("Invoice ID is required.\n");
                    return;
                }
                data["invoice_id"] = invoiceId;
            }
            else if (identifierType == "2")
            {
                var paymentLinkId = Helpers.GetInput("Enter Payment Link ID: ");
                if (string.IsNullOrWhiteSpace(paymentLinkId))
                {
                    Helpers.PrintError("Payment Link ID is required.\n");
                    return;
                }
                data["payment_link_id"] = paymentLinkId;
            }
            else
            {
                Helpers.PrintError("Invalid choice. Please choose 1 or 2.\n");
                return;
            }
            
            Helpers.PrintInfo("\nNote: Payment links can only be updated when status is 'created'.\n");
            Helpers.PrintInfo("Enter fields to update (at least one field is required):\n");
            
            // User information
            var firstName = Helpers.GetInput("User First Name (optional): ", false);
            var lastName = Helpers.GetInput("User Last Name (optional): ", false);
            if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
            {
                var user = new Dictionary<string, object?>();
                if (!string.IsNullOrWhiteSpace(firstName)) user["first_name"] = firstName;
                if (!string.IsNullOrWhiteSpace(lastName)) user["last_name"] = lastName;
                data["user"] = user;
            }
            
            // Total amount
            var totalAmountStr = Helpers.GetInput("Total Amount (optional): ", false);
            if (!string.IsNullOrWhiteSpace(totalAmountStr) && decimal.TryParse(totalAmountStr, out var totalAmount))
            {
                data["total_amount"] = totalAmount;
            }
            
            // Expires at
            var expiresAt = Helpers.GetInput("Expires At (YYYY-MM-DD HH:MM:SS, UTC, optional): ", false);
            if (!string.IsNullOrWhiteSpace(expiresAt))
            {
                data["expires_at"] = expiresAt;
            }
            
            // Currency
            var currency = Helpers.GetInput("Currency (ISO-4217, optional): ", false);
            if (!string.IsNullOrWhiteSpace(currency))
            {
                data["currency"] = currency;
            }
            
            // Validate that at least one field (besides identifier) is provided
            var updateFields = new Dictionary<string, object?>(data);
            updateFields.Remove("invoice_id");
            updateFields.Remove("payment_link_id");
            
            if (updateFields.Count == 0)
            {
                Helpers.PrintError("Error: At least one field must be provided for update.\n");
                Helpers.PrintInfo("Allowed fields: user.first_name, user.last_name, total_amount, expires_at, currency, order_line_items, bank_account, custom_attributes\n");
                return;
            }
            
            var result = await api.PaymentLinks().UpdatePaymentLinkAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                var error = errorProp;
                var errorCode = error.TryGetProperty("nimbbl_error_code", out var ec) ? ec.GetString() : "UNKNOWN_ERROR";
                var merchantMsg = error.TryGetProperty("nimbbl_merchant_message", out var mm) 
                    ? mm.GetString() 
                    : (error.TryGetProperty("nimbbl_consumer_message", out var cm) ? cm.GetString() : "Unknown error");
                
                Helpers.PrintError($"Error ({errorCode}): {merchantMsg}\n");
                
                if (errorCode == "LINK_CANNOT_BE_UPDATED")
                {
                    Helpers.PrintInfo("Note: Payment links can only be updated when status is 'created'.\n");
                    Helpers.PrintInfo("Use Payment Link Enquiry to check the current status of your payment link.\n");
                }
            }
            else
            {
                Helpers.PrintSuccess("Payment link updated successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task EnquiryPaymentLinkExample(NimbblApi api)
    {
        try
        {
            // Generate merchant token
            var tokenResponse = await api.Auth().GenerateTokenAsync();
            var merchantToken = tokenResponse.TryGetProperty("token", out var tokenProp) 
                ? tokenProp.GetString() 
                : null;
            
            if (string.IsNullOrWhiteSpace(merchantToken))
            {
                Helpers.PrintError("Failed to generate merchant token.\n");
                return;
            }
            
            api.SetBearerToken(merchantToken);
            
            Helpers.PrintInfo("Identify payment link using:\n");
            Console.WriteLine("1. Invoice ID\n");
            Console.WriteLine("2. Payment Link ID\n");
            var identifierType = Helpers.GetInput("Choose (1 or 2): ");
            
            var data = new Dictionary<string, object?>();
            if (identifierType == "1")
            {
                var invoiceId = Helpers.GetInput("Enter Invoice ID: ");
                if (string.IsNullOrWhiteSpace(invoiceId))
                {
                    Helpers.PrintError("Invoice ID is required.\n");
                    return;
                }
                data["invoice_id"] = invoiceId;
            }
            else if (identifierType == "2")
            {
                var paymentLinkId = Helpers.GetInput("Enter Payment Link ID: ");
                if (string.IsNullOrWhiteSpace(paymentLinkId))
                {
                    Helpers.PrintError("Payment Link ID is required.\n");
                    return;
                }
                data["payment_link_id"] = paymentLinkId;
            }
            else
            {
                Helpers.PrintError("Invalid choice. Please choose 1 or 2.\n");
                return;
            }
            
            var result = await api.PaymentLinks().EnquiryPaymentLinkAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                var error = errorProp;
                var errorCode = error.TryGetProperty("nimbbl_error_code", out var ec) ? ec.GetString() : "UNKNOWN_ERROR";
                var merchantMsg = error.TryGetProperty("nimbbl_merchant_message", out var mm) 
                    ? mm.GetString() 
                    : (error.TryGetProperty("nimbbl_consumer_message", out var cm) ? cm.GetString() : "Unknown error");
                
                Helpers.PrintError($"Error ({errorCode}): {merchantMsg}\n");
            }
            else
            {
                Helpers.PrintSuccess("Payment link enquiry successful!\n");
                if (result.TryGetProperty("payment_link_id", out var plid))
                {
                    Console.WriteLine($"  Payment Link ID: {plid.GetString()}");
                }
                if (result.TryGetProperty("status", out var stat))
                {
                    Console.WriteLine($"  Status: {stat.GetString()}");
                }
                if (result.TryGetProperty("total_amount", out var amt))
                {
                    var curr = result.TryGetProperty("currency", out var c) ? c.GetString() : "";
                    Console.WriteLine($"  Total Amount: {amt.GetDecimal()} {curr}");
                }
                if (result.TryGetProperty("payment_link_amount_paid", out var paid))
                {
                    Console.WriteLine($"  Amount Paid: {paid.GetDecimal()}");
                }
                if (result.TryGetProperty("orders_with_transactions", out var orders) && 
                    orders.ValueKind == JsonValueKind.Array)
                {
                    Console.WriteLine($"  Orders with Transactions: {orders.GetArrayLength()}");
                }
                Console.WriteLine("\nFull Response:\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task PerformPaymentLinkActionsExample(NimbblApi api)
    {
        try
        {
            // Generate merchant token
            var tokenResponse = await api.Auth().GenerateTokenAsync();
            var merchantToken = tokenResponse.TryGetProperty("token", out var tokenProp) 
                ? tokenProp.GetString() 
                : null;
            
            if (string.IsNullOrWhiteSpace(merchantToken))
            {
                Helpers.PrintError("Failed to generate merchant token.\n");
                return;
            }
            
            api.SetBearerToken(merchantToken);
            
            Helpers.PrintInfo("Identify payment link using:\n");
            Console.WriteLine("1. Invoice ID\n");
            Console.WriteLine("2. Payment Link ID\n");
            var identifierType = Helpers.GetInput("Choose (1 or 2): ");
            
            var data = new Dictionary<string, object?>();
            if (identifierType == "1")
            {
                var invoiceId = Helpers.GetInput("Enter Invoice ID: ");
                if (string.IsNullOrWhiteSpace(invoiceId))
                {
                    Helpers.PrintError("Invoice ID is required.\n");
                    return;
                }
                data["invoice_id"] = invoiceId;
            }
            else if (identifierType == "2")
            {
                var paymentLinkId = Helpers.GetInput("Enter Payment Link ID: ");
                if (string.IsNullOrWhiteSpace(paymentLinkId))
                {
                    Helpers.PrintError("Payment Link ID is required.\n");
                    return;
                }
                data["payment_link_id"] = paymentLinkId;
            }
            else
            {
                Helpers.PrintError("Invalid choice. Please choose 1 or 2.\n");
                return;
            }
            
            Helpers.PrintInfo("\nAvailable actions:\n");
            Console.WriteLine("1. send - Send the payment link\n");
            Console.WriteLine("2. cancel - Cancel the payment link\n");
            var actionChoice = Helpers.GetInput("Choose action (1 or 2): ");
            
            if (actionChoice == "1")
            {
                data["action"] = "send";
            }
            else if (actionChoice == "2")
            {
                data["action"] = "cancel";
            }
            else
            {
                Helpers.PrintError("Invalid choice. Please choose 1 or 2.\n");
                return;
            }
            
            var result = await api.PaymentLinks().PerformPaymentLinkActionsAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                var error = errorProp;
                var errorCode = error.TryGetProperty("nimbbl_error_code", out var ec) ? ec.GetString() : "UNKNOWN_ERROR";
                var merchantMsg = error.TryGetProperty("nimbbl_merchant_message", out var mm) 
                    ? mm.GetString() 
                    : (error.TryGetProperty("nimbbl_consumer_message", out var cm) ? cm.GetString() : "Unknown error");
                
                Helpers.PrintError($"Error ({errorCode}): {merchantMsg}\n");
            }
            else
            {
                Helpers.PrintSuccess("Payment link action executed successfully!\n");
                if (result.TryGetProperty("status", out var stat))
                {
                    Console.WriteLine($"  Status: {stat.GetString()}");
                }
                if (result.TryGetProperty("payment_link_url", out var url))
                {
                    Console.WriteLine($"  Payment Link URL: {url.GetString()}");
                }
                if (result.TryGetProperty("payment_link_id", out var plid))
                {
                    Console.WriteLine($"  Payment Link ID: {plid.GetString()}");
                }
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }
}
