using System.Text.Json;
using System;
using Nimbbl.Sdk.Rest;
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
        
        Console.WriteLine("\nFor more information, see: https://nimbbl.biz/docs/api-reference/introduction/\n");
    }

    public static async Task CreatePaymentLinkExample(NimbblApi api)
    {
        try
        {
            Helpers.PrintInfo("Enter payment link details:\n");
            var invoiceId = Helpers.GetInput("Enter Invoice ID (optional, auto-generated if blank): ", false);
            if (string.IsNullOrWhiteSpace(invoiceId))
            {
                // Auto-generate a unique invoice ID if user leaves it blank
                invoiceId = Helpers.GenerateInvoiceId("INV");
                Helpers.PrintInfo($"Generated Invoice ID: {invoiceId}\n");
            }
            
            var totalAmountStr = Helpers.GetInput("Enter Total Amount: ");
            if (string.IsNullOrWhiteSpace(totalAmountStr))
            {
                Helpers.PrintError("Total Amount is required.\n");
                return;
            }

            if (!decimal.TryParse(totalAmountStr, out var totalAmount))
            {
                Helpers.PrintError("Total Amount must be a valid number.\n");
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

            // Order line items (required)
            Helpers.PrintInfo("\nOrder Line Items (at least one required):\n");
            var orderLineItems = new List<Dictionary<string, object?>>();
            var itemNum = 1;
            while (true)
            {
                Helpers.PrintInfo($"Item {itemNum}:\n");
                var skuId = Helpers.GetInput("  SKU ID: ");
                var title = Helpers.GetInput("  Title: ");
                var description = Helpers.GetInput("  Description (optional): ", false);
                var rateStr = Helpers.GetInput("  Rate: ");
                var quantityStr = Helpers.GetInput("  Quantity: ");
                var amountBeforeTaxStr = Helpers.GetInput("  Amount Before Tax: ");
                var taxStr = Helpers.GetInput("  Tax (optional, default: 0): ", false) ?? "0";
                var itemTotalAmountStr = Helpers.GetInput("  Total Amount: ");
                var imageUrl = Helpers.GetInput("  Image URL (optional): ", false);

                if (!decimal.TryParse(rateStr, out var rate))
                {
                    Helpers.PrintError("  Rate must be a valid number.\n");
                    return;
                }
                if (!int.TryParse(quantityStr, out var quantity))
                {
                    Helpers.PrintError("  Quantity must be a valid integer.\n");
                    return;
                }
                if (!decimal.TryParse(amountBeforeTaxStr, out var amountBeforeTax))
                {
                    Helpers.PrintError("  Amount Before Tax must be a valid number.\n");
                    return;
                }
                if (!decimal.TryParse(taxStr, out var tax))
                {
                    Helpers.PrintError("  Tax must be a valid number.\n");
                    return;
                }
                if (!decimal.TryParse(itemTotalAmountStr, out var itemTotalAmount))
                {
                    Helpers.PrintError("  Total Amount must be a valid number.\n");
                    return;
                }

                var item = new Dictionary<string, object?>
                {
                    ["sku_id"] = skuId,
                    ["title"] = title,
                    ["rate"] = rate,
                    ["quantity"] = quantity,
                    ["amount_before_tax"] = amountBeforeTax,
                    ["tax"] = tax,
                    ["total_amount"] = itemTotalAmount
                };
                if (!string.IsNullOrWhiteSpace(description)) item["description"] = description;
                if (!string.IsNullOrWhiteSpace(imageUrl)) item["image_url"] = imageUrl;

                var addSerialNumbers = Helpers.GetInput("  Add Serial Numbers? (y/N): ", false);
                if (!string.IsNullOrWhiteSpace(addSerialNumbers) && addSerialNumbers.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    var serialNumbers = new List<string>();
                    while (true)
                    {
                        var serial = Helpers.GetInput("    Serial Number (or press Enter to finish): ", false);
                        if (string.IsNullOrWhiteSpace(serial)) break;
                        serialNumbers.Add(serial);
                    }
                    if (serialNumbers.Count > 0)
                    {
                        item["serial_numbers"] = serialNumbers;
                    }
                }

                orderLineItems.Add(item);
                itemNum++;

                var addMore = Helpers.GetInput("Add another item? (y/N): ", false);
                if (string.IsNullOrWhiteSpace(addMore) || !addMore.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
            
            // Required field: expires_at
            Helpers.PrintInfo("\nRequired Field:\n");
            var defaultExpiresAtUtc = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            var expiresAt = Helpers.GetInput($"Expires At (YYYY-MM-DD HH:MM:SS, UTC) [default: {defaultExpiresAtUtc}]: ", false);
            if (string.IsNullOrWhiteSpace(expiresAt))
            {
                expiresAt = defaultExpiresAtUtc;
                Helpers.PrintInfo($"Using default expires_at (UTC): {expiresAt}\n");
            }

            // Optional fields
            var useDefaults = Helpers.GetInput("\nUse default values for optional fields? (y/N): ", false);
            var useDefaultsFlag = !string.IsNullOrWhiteSpace(useDefaults) && useDefaults.Trim().Equals("y", StringComparison.OrdinalIgnoreCase);

            string? topDescription = null;
            string? termsAndConditions = null;
            string? callbackUrl = null;
            bool sendSms = true;
            bool sendEmail = false;
            string? notificationScheduledAt = null;
            string? emailTemplateId = null;
            string? smsTemplateId = null;
            Dictionary<string, object?>? bankAccount = null;
            Dictionary<string, object?>? customAttributes = null;
            List<string>? topLevelSerialNumbers = null;

            if (useDefaultsFlag)
            {
                topDescription = "xxx";
                termsAndConditions = "xxx";
                callbackUrl = "https://www.google.com";
                sendSms = true;
                sendEmail = true;
                bankAccount = new Dictionary<string, object?>
                {
                    ["account_number"] = "037801513988",
                    ["name"] = "Vasudha Maini",
                    ["ifsc"] = "ICIC0000378"
                };
                customAttributes = new Dictionary<string, object?>
                {
                    ["Name"] = "Vasudha",
                    ["Place"] = "Delhi",
                    ["Animal"] = "Tiger",
                    ["Thing"] = "Pen"
                };
                topLevelSerialNumbers = new List<string>
                {
                    "359043372654548",
                    "359043371395481"
                };

                Helpers.PrintInfo("Using default values:\n");
                Helpers.PrintInfo("  description='xxx', terms_and_conditions='xxx', callback_url='https://www.google.com'\n");
                Helpers.PrintInfo("  send_sms=true, send_email=true\n");
                Helpers.PrintInfo("  bank_account: account_number='037801513988', name='Vasudha Maini', ifsc='ICIC0000378'\n");
                Helpers.PrintInfo("  custom_attributes: Name='Vasudha', Place='Delhi', Animal='Tiger', Thing='Pen'\n");
                Helpers.PrintInfo("  top-level serial_numbers: ['359043372654548', '359043371395481']\n");
            }
            else
            {
                Helpers.PrintInfo("\nOptional Fields (press Enter to skip any):\n");
                topDescription = Helpers.GetInput("Description (optional): ", false);
                termsAndConditions = Helpers.GetInput("Terms and Conditions (optional): ", false);
                callbackUrl = Helpers.GetInput("Callback URL (optional): ", false);

                var sendSmsInput = Helpers.GetInput("Send SMS? (Y/n, default: Y): ", false);
                if (string.IsNullOrWhiteSpace(sendSmsInput))
                    sendSms = true;
                else
                    sendSms = sendSmsInput.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) || sendSmsInput.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase);

                var sendEmailInput = Helpers.GetInput("Send Email? (y/N, default: N): ", false);
                if (string.IsNullOrWhiteSpace(sendEmailInput))
                    sendEmail = false;
                else
                    sendEmail = sendEmailInput.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) || sendEmailInput.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase);

                notificationScheduledAt = Helpers.GetInput("Notification Scheduled At (YYYY-MM-DD HH:MM:SS, optional): ", false);
                emailTemplateId = Helpers.GetInput("Email Template ID (optional): ", false);
                smsTemplateId = Helpers.GetInput("SMS Template ID (optional): ", false);

                var addBankAccount = Helpers.GetInput("Add Bank Account? (y/N): ", false);
                if (!string.IsNullOrWhiteSpace(addBankAccount) && addBankAccount.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    var accountNumber = Helpers.GetInput("  Account Number: ");
                    var accountName = Helpers.GetInput("  Account Name: ");
                    var ifsc = Helpers.GetInput("  IFSC: ");
                    bankAccount = new Dictionary<string, object?>
                    {
                        ["account_number"] = accountNumber,
                        ["name"] = accountName,
                        ["ifsc"] = ifsc
                    };
                }

                var addCustomAttributes = Helpers.GetInput("Add Custom Attributes? (y/N): ", false);
                if (!string.IsNullOrWhiteSpace(addCustomAttributes) && addCustomAttributes.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    customAttributes = new Dictionary<string, object?>();
                    while (true)
                    {
                        var key = Helpers.GetInput("  Attribute Key (or press Enter to finish): ", false);
                        if (string.IsNullOrWhiteSpace(key)) break;
                        var value = Helpers.GetInput("  Attribute Value: ");
                        customAttributes[key] = value;
                    }
                    if (customAttributes.Count == 0)
                        customAttributes = null;
                }

                var addTopLevelSerials = Helpers.GetInput("Add Top-Level Serial Numbers? (y/N): ", false);
                if (!string.IsNullOrWhiteSpace(addTopLevelSerials) && addTopLevelSerials.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    topLevelSerialNumbers = new List<string>();
                    while (true)
                    {
                        var serial = Helpers.GetInput("  Serial Number (or press Enter to finish): ", false);
                        if (string.IsNullOrWhiteSpace(serial)) break;
                        topLevelSerialNumbers.Add(serial);
                    }
                    if (topLevelSerialNumbers.Count == 0)
                        topLevelSerialNumbers = null;
                }
            }
            
            // Build request data
            var data = new Dictionary<string, object?>
            {
                ["invoice_id"] = invoiceId,
                ["total_amount"] = totalAmount,
                ["currency"] = currency,
                ["expires_at"] = expiresAt,
                ["order_line_items"] = orderLineItems,
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

            // Optional fields
            data["send_sms"] = sendSms;
            data["send_email"] = sendEmail;
            if (!string.IsNullOrWhiteSpace(topDescription)) data["description"] = topDescription;
            if (!string.IsNullOrWhiteSpace(termsAndConditions)) data["terms_and_conditions"] = termsAndConditions;
            if (!string.IsNullOrWhiteSpace(callbackUrl)) data["callback_url"] = callbackUrl;
            if (!string.IsNullOrWhiteSpace(notificationScheduledAt)) data["notification_scheduled_at"] = notificationScheduledAt;
            if (!string.IsNullOrWhiteSpace(emailTemplateId)) data["email_template_id"] = emailTemplateId;
            if (!string.IsNullOrWhiteSpace(smsTemplateId)) data["sms_template_id"] = smsTemplateId;
            if (bankAccount != null) data["bank_account"] = bankAccount;
            if (customAttributes != null) data["custom_attributes"] = customAttributes;
            if (topLevelSerialNumbers != null) data["serial_numbers"] = topLevelSerialNumbers;

            // Merchant token is automatically generated and used for authentication
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
            // Merchant token is automatically generated and used for authentication
            Helpers.PrintInfo("Identify payment link using:\n");
            Console.WriteLine("1. Invoice ID\n");
            Console.WriteLine("2. Payment Link ID\n");
            var identifierType = Helpers.GetInput("Choose (1 or 2): ");
            
            Dictionary<string, object?> data = [];
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
                Dictionary<string, object?> user = [];
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
            // Merchant token is automatically generated and used for authentication
            Helpers.PrintInfo("Identify payment link using:\n");
            Console.WriteLine("1. Invoice ID\n");
            Console.WriteLine("2. Payment Link ID\n");
            var identifierType = Helpers.GetInput("Choose (1 or 2): ");
            
            Dictionary<string, object?> data = [];
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
            // Merchant token is automatically generated and used for authentication
            Helpers.PrintInfo("Identify payment link using:\n");
            Console.WriteLine("1. Invoice ID\n");
            Console.WriteLine("2. Payment Link ID\n");
            var identifierType = Helpers.GetInput("Choose (1 or 2): ");
            
            Dictionary<string, object?> data = [];
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
