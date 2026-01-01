using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.Common;

namespace Examples;

/// <summary>
/// Checkout Utilities Examples
/// </summary>
public static class CheckoutUtilitiesExamples
{
    public static async Task RunAllExamples(NimbblApi api)
    {
        Helpers.PrintHeader("=== Checkout Utilities API Examples ===");
        
        Helpers.PrintStep(1, "List Payment Modes");
        await ListPaymentModesExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(2, "List Banks");
        await ListBanksExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(3, "List Wallets");
        await ListWalletsExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(4, "List EMIs");
        await ListEMIsExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(5, "Get Offers");
        await GetOffersExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(6, "Get Card BIN Data");
        await GetCardBinDataExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(7, "Get Card Details");
        await GetCardDetailsExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(8, "Validate UPI VPA");
        await ValidateUpiVpaExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(9, "Get UPI App Details");
        await GetUpiAppDetailsExample(api);
        
        Console.WriteLine("\nFor more information, see: https://nimbbl.biz/docs/category/api-reference/checkout-utilities/\n");
    }

    public static async Task ListPaymentModesExample(NimbblApi api)
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
            
            var orderId = Helpers.GetInput("Enter Order ID: ");
            if (string.IsNullOrWhiteSpace(orderId))
            {
                Helpers.PrintError("Order ID is required.\n");
                return;
            }
            
            Dictionary<string, object?> data = [];
            data["order_id"] = orderId;
            
            // Optional: OS type (e.g., "android", "ios")
            var os = Helpers.GetInput("Enter OS (optional, e.g., android/ios): ", false);
            if (!string.IsNullOrWhiteSpace(os))
            {
                data["os"] = os;
            }
            
            // Optional: UPI app package names
            var upiApps = Helpers.GetInput("Enter UPI app package names (comma-separated, optional): ", false);
            if (!string.IsNullOrWhiteSpace(upiApps))
            {
                var packageNames = upiApps.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToArray();
                if (packageNames.Length > 0)
                {
                    data["upi_app_package_names"] = packageNames;
                }
            }
            
            var result = await api.CheckoutUtilities().ListPaymentModesAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Payment modes retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task ListBanksExample(NimbblApi api)
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
            
            var orderId = Helpers.GetInput("Enter Order ID: ");
            if (string.IsNullOrWhiteSpace(orderId))
            {
                Helpers.PrintError("Order ID is required.\n");
                return;
            }
            
            var data = new Dictionary<string, object?> { ["order_id"] = orderId };
            
            var result = await api.CheckoutUtilities().ListBanksAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Banks retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task ListWalletsExample(NimbblApi api)
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
            
            var orderId = Helpers.GetInput("Enter Order ID: ");
            if (string.IsNullOrWhiteSpace(orderId))
            {
                Helpers.PrintError("Order ID is required.\n");
                return;
            }
            
            var data = new Dictionary<string, object?> { ["order_id"] = orderId };
            
            var result = await api.CheckoutUtilities().ListWalletsAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Wallets retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task ListEMIsExample(NimbblApi api)
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
            
            var orderId = Helpers.GetInput("Enter Order ID: ");
            if (string.IsNullOrWhiteSpace(orderId))
            {
                Helpers.PrintError("Order ID is required.\n");
                return;
            }
            
            var data = new Dictionary<string, object?> { ["order_id"] = orderId };
            
            var result = await api.CheckoutUtilities().ListEmisAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("EMIs retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task GetOffersExample(NimbblApi api)
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
            
            var orderId = Helpers.GetInput("Enter Order ID: ");
            if (string.IsNullOrWhiteSpace(orderId))
            {
                Helpers.PrintError("Order ID is required.\n");
                return;
            }
            
            var paymentModeCode = Helpers.GetInput("Enter Payment Mode Code (card/net_banking/wallet/upi/pay_later/all): ");
            if (string.IsNullOrWhiteSpace(paymentModeCode))
            {
                Helpers.PrintError("Payment Mode Code is required.\n");
                return;
            }
            
            var data = new Dictionary<string, object?>
            {
                ["order_id"] = orderId,
                [CheckoutConstants.OptionKeyPaymentModeCode] = paymentModeCode
            };
            
            // Optional fields
            var currency = Helpers.GetInput("Enter Currency (optional, press Enter to skip): ", false);
            if (!string.IsNullOrWhiteSpace(currency))
            {
                data["currency"] = currency;
            }
            
            // Additional fields based on payment mode
            if (paymentModeCode == "card")
            {
                var cardInputType = Helpers.GetInput("Enter Card Input Type (card_pan/merchant_network_token/nimbbl_token_id, optional): ", false);
                if (!string.IsNullOrWhiteSpace(cardInputType))
                {
                    data["card_input_type"] = cardInputType;
                    Dictionary<string, object?> card = [];
                    
                    if (cardInputType == "card_pan")
                    {
                        var cardNo = Helpers.GetInput("Enter Card Number (optional): ", false);
                        if (!string.IsNullOrWhiteSpace(cardNo))
                        {
                            card["card_no"] = cardNo;
                        }
                    }
                    else if (cardInputType == "merchant_network_token")
                    {
                        var networkToken = Helpers.GetInput("Enter Network Token (optional): ", false);
                        if (!string.IsNullOrWhiteSpace(networkToken))
                        {
                            card["network_token"] = networkToken;
                        }
                    }
                    else if (cardInputType == "nimbbl_token_id")
                    {
                        var nimbblTokenId = Helpers.GetInput("Enter Nimbbl Token ID (optional): ", false);
                        if (!string.IsNullOrWhiteSpace(nimbblTokenId))
                        {
                            card["nimbbl_token_id"] = nimbblTokenId;
                        }
                    }
                    
                    // Add currency to card object if provided
                    if (!string.IsNullOrWhiteSpace(currency))
                    {
                        card["currency"] = currency;
                    }
                    
                    if (card.Count > 0)
                    {
                        data["card"] = card;
                    }
                }
            }
            else if (paymentModeCode == CheckoutConstants.PaymentModeNetBanking)
            {
                Helpers.PrintInfo("Bank Code is required for net_banking payment mode.\n");
                Console.WriteLine("   Common bank codes: HDFC, ICICI, SBI, AXIS, KOTAK, etc.\n");
                var bankCode = Helpers.GetInput("Enter Bank Code (default: HDFC): ", false) ?? "HDFC";
                data[CheckoutConstants.OptionKeyBankCode] = bankCode;
            }
            else if (paymentModeCode == CheckoutConstants.PaymentModeWallet)
            {
                var walletCode = Helpers.GetInput("Enter Wallet Code (optional, press Enter to skip): ", false);
                if (!string.IsNullOrWhiteSpace(walletCode))
                {
                    data[CheckoutConstants.OptionKeyWalletCode] = walletCode;
                }
            }
            else if (paymentModeCode == CheckoutConstants.PaymentModeUpi)
            {
                var upiId = Helpers.GetInput("Enter UPI ID (optional, press Enter to skip): ", false);
                if (!string.IsNullOrWhiteSpace(upiId))
                {
                    data["upi_id"] = upiId;
                }
            }
            
            var result = await api.CheckoutUtilities().GetOffersAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Offers retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task GetCardBinDataExample(NimbblApi api)
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
            
            var cardBin = Helpers.GetInput("Enter Card BIN (first 6 digits): ");
            if (string.IsNullOrWhiteSpace(cardBin))
            {
                Helpers.PrintError("Card BIN is required.\n");
                return;
            }
            
            var orderId = Helpers.GetInput("Enter Order ID (optional, press Enter to skip): ", false);
            var data = new Dictionary<string, object?> { ["card_bin"] = cardBin };
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                data["order_id"] = orderId;
            }
            
            var result = await api.CheckoutUtilities().GetCardBinDataAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Card BIN data retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task GetCardDetailsExample(NimbblApi api)
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
            
            Helpers.PrintInfo("This API requires RSA-encrypted card details.\n");
            Helpers.PrintInfo("You need to:\n");
            Helpers.PrintInfo("1. Get the Nimbbl public key for encryption (contact help@nimbbl.biz)\n");
            Helpers.PrintInfo("2. Format card details as: {\"card_no\":\"4111111111111111\",\"cvv\":\"123\",\"card_holder_name\":\"test\",\"expiry\":\"11/22\"}\n");
            Helpers.PrintInfo("3. Encrypt using RSA encryption\n");
            
            var action = Helpers.GetInput("Enter action (default: PAR): ", false) ?? "PAR";
            var cardInputType = Helpers.GetInput("Enter card_input_type (default: card_pan): ", false) ?? "card_pan";
            var cardDetails = Helpers.GetInput("Enter RSA-encrypted card_details: ");
            if (string.IsNullOrWhiteSpace(cardDetails))
            {
                Helpers.PrintError("RSA-encrypted card details are required.\n");
                return;
            }
            
            var data = new Dictionary<string, object?>
            {
                ["action"] = action,
                ["card_input_type"] = cardInputType,
                ["card_details"] = cardDetails
            };
            
            // Optional device details
            var includeDevice = Helpers.GetInput("Include device details? (y/N): ", false);
            if (includeDevice?.ToLower() == "y")
            {
                Dictionary<string, object?> device = [];
                var acceptHeader = Helpers.GetInput("Accept Header (optional): ", false);
                if (!string.IsNullOrWhiteSpace(acceptHeader)) device["accept_header"] = acceptHeader;
                
                var userAgent = Helpers.GetInput("User Agent (optional): ", false);
                if (!string.IsNullOrWhiteSpace(userAgent)) device["user_agent"] = userAgent;
                
                var browserLanguage = Helpers.GetInput("Browser Language (optional): ", false);
                if (!string.IsNullOrWhiteSpace(browserLanguage)) device["browser_language"] = browserLanguage;
                
                var jsEnabled = Helpers.GetInput("JavaScript Enabled (true/false, optional): ", false);
                if (!string.IsNullOrWhiteSpace(jsEnabled) && bool.TryParse(jsEnabled, out var js))
                {
                    device["browser_javascript_enabled"] = js;
                }
                
                var javaEnabled = Helpers.GetInput("Java Enabled (true/false, optional): ", false);
                if (!string.IsNullOrWhiteSpace(javaEnabled) && bool.TryParse(javaEnabled, out var java))
                {
                    device["browser_java_enabled"] = java;
                }
                
                var browser = Helpers.GetInput("Browser Name (optional): ", false);
                if (!string.IsNullOrWhiteSpace(browser)) device["browser"] = browser;
                
                var browserTz = Helpers.GetInput("Browser Timezone (optional): ", false);
                if (!string.IsNullOrWhiteSpace(browserTz)) device["browser_tz"] = browserTz;
                
                var colorDepth = Helpers.GetInput("Color Depth (optional): ", false);
                if (!string.IsNullOrWhiteSpace(colorDepth)) device["browser_color_depth"] = colorDepth;
                
                var screenHeight = Helpers.GetInput("Screen Height (optional): ", false);
                if (!string.IsNullOrWhiteSpace(screenHeight)) device["browser_screen_height"] = screenHeight;
                
                var screenWidth = Helpers.GetInput("Screen Width (optional): ", false);
                if (!string.IsNullOrWhiteSpace(screenWidth)) device["browser_screen_width"] = screenWidth;
                
                var fingerprint = Helpers.GetInput("Device Fingerprint (optional): ", false);
                if (!string.IsNullOrWhiteSpace(fingerprint)) device["fingerprint"] = fingerprint;
                
                var ipAddress = Helpers.GetInput("IP Address (optional): ", false);
                if (!string.IsNullOrWhiteSpace(ipAddress)) device["ip_address"] = ipAddress;
                
                if (device.Count > 0)
                {
                    data["device"] = device;
                }
            }
            
            var result = await api.CheckoutUtilities().GetCardDetailsAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Card details retrieved successfully!\n");
                if (result.TryGetProperty("bin", out var binProp) && binProp.ValueKind == JsonValueKind.Object)
                {
                    var bin = binProp;
                    var cardBin = bin.TryGetProperty("card_bin", out var cb) ? cb.GetString() : "N/A";
                    var scheme = bin.TryGetProperty("scheme_name", out var sn) ? sn.GetString() : "N/A";
                    var issuer = bin.TryGetProperty("issuer_name", out var inProp) ? inProp.GetString() : "N/A";
                    var cardType = bin.TryGetProperty("payment_mode", out var pm) ? pm.GetString() : "N/A";
                    Console.WriteLine($"  Card BIN: {cardBin}\n");
                    Console.WriteLine($"  Scheme: {scheme}\n");
                    Console.WriteLine($"  Issuer: {issuer}\n");
                    Console.WriteLine($"  Card Type: {cardType}\n");
                }
                if (result.TryGetProperty("tokenization", out var tokenProp) && tokenProp.ValueKind == JsonValueKind.Object)
                {
                    var tokenization = tokenProp;
                    var par = tokenization.TryGetProperty("par", out var parProp) ? parProp.GetString() : "N/A";
                    var nimbblTokenId = tokenization.TryGetProperty("nimbbl_token_id", out var tid) ? tid.GetString() : "N/A";
                    Console.WriteLine($"  PAR: {par}\n");
                    Console.WriteLine($"  Nimbbl Token ID: {nimbblTokenId}\n");
                }
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task ValidateUpiVpaExample(NimbblApi api)
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
            
            var upiId = Helpers.GetInput("Enter UPI ID (e.g., user@paytm): ");
            if (string.IsNullOrWhiteSpace(upiId))
            {
                Helpers.PrintError("UPI ID is required.\n");
                return;
            }
            
            var data = new Dictionary<string, object?> { ["upi_id"] = upiId };
            
            var result = await api.CheckoutUtilities().ValidateUpiVpaAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("UPI VPA validated!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task GetUpiAppDetailsExample(NimbblApi api)
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
            
            var platform = Helpers.GetInput("Enter Platform (ios/android): ");
            if (string.IsNullOrWhiteSpace(platform))
            {
                Helpers.PrintError("Platform is required.\n");
                return;
            }
            
            // Validate platform value
            if (platform.ToLower() != "ios" && platform.ToLower() != "android")
            {
                Helpers.PrintError("Platform must be 'ios' or 'android'.\n");
                return;
            }
            
            var data = new Dictionary<string, object?> { ["platform"] = platform.ToLower() };
            
            var result = await api.CheckoutUtilities().GetUpiAppDetailsAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("UPI app details retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }
}
