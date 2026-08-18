using Nimbbl.Sdk.Rest;
using Examples;

Helpers.PrintHeader("=== Nimbbl .NET SDK - Interactive CLI Menu ===");

try
{
    // Load .env file - sample app is responsible for loading environment variables
    EnvLoader.LoadEnvFile();
    
    // Read configuration from environment variables using utility methods
    var accessKey = Helpers.GetEnvString("NIMBBL_ACCESS_KEY");
    var accessSecret = Helpers.GetEnvString("NIMBBL_ACCESS_SECRET");
    var apiHost = Helpers.GetEnvString("NIMBBL_API_HOST");
    var debugLogging = Helpers.GetEnvBool("NIMBBL_DEBUG_LOGGING");
    var logFilePath = Helpers.GetEnvString("NIMBBL_LOG_FILE");
    
    // Read encryption flags (available in all environments)
    var encryptPayload = Helpers.GetEnvBool("ENCRYPT_PAYLOAD", defaultValue: false);
    
    // Validate required configuration
    if (string.IsNullOrEmpty(accessKey) || accessKey == "your_access_key_here")
    {
        Helpers.PrintError("Please update .env file with your Nimbbl credentials.\n");
        Helpers.PrintInfo("Copy .env.example to .env and update:\n");
        Helpers.PrintInfo("  - NIMBBL_ACCESS_KEY\n");
        Helpers.PrintInfo("  - NIMBBL_ACCESS_SECRET\n");
        Helpers.PrintInfo("  - NIMBBL_API_HOST (optional, defaults to production)\n");
        Helpers.PrintInfo("  - NIMBBL_DEBUG_LOGGING (optional, defaults to false)\n");
        Helpers.PrintInfo("  - ENCRYPT_PAYLOAD (optional, defaults to false)\n");
        return;
    }

    if (string.IsNullOrEmpty(accessSecret))
    {
        Helpers.PrintError("NIMBBL_ACCESS_SECRET is required in .env file.\n");
        return;
    }

    // Initialize Nimbbl API with parameters from environment variables
    // SDK accepts parameters - it doesn't load .env files itself
    // Note: Logging (INFO, WARNING, ERROR) is always enabled by default
    // Only DEBUG logs are controlled by NIMBBL_DEBUG_LOGGING flag
    var api = NimbblApi.Initialize(
        accessKey: accessKey!,
        accessSecret: accessSecret!,
        apiHost: apiHost,
        debugLogging: debugLogging,
        logFilePath: logFilePath,
        encryptPayload: encryptPayload);

    // Main menu loop
    while (true)
    {
        PrintMenu();
    var choice = Console.ReadLine();
    Console.WriteLine();

        if (string.IsNullOrWhiteSpace(choice) || choice.Trim() == "0")
        {
            Helpers.PrintInfo("Goodbye!\n");
            return;
        }
        
        Helpers.PrintSeparator();
        
        try
        {
            switch (choice.Trim())
    {
        case "1":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Generate Token");
                    Console.ResetColor();
                    await GenerateTokenExample.GenerateTokenExampleAsync(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/generate-token-v-3/", "Generate Token API");
            break;
                    
        case "2":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Create Order");
                    Console.ResetColor();
                    await OrderExamples.CreateOrderExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/create-an-order-v-3/", "Create Order API");
            break;
                    
        case "3":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Get Order by ID");
                    Console.ResetColor();
                    await OrderExamples.GetOrderByIdExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/get-order-v-3/", "Get Order API");
            break;
                    
        case "4":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Get Order by Invoice ID");
                    Console.ResetColor();
                    await OrderExamples.GetOrderByInvoiceIdExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/get-order-v-3/", "Get Order API");
            break;
                    
                // Payments API
        case "5":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Initiate Payment");
                    Console.ResetColor();
                    await PaymentExamples.InitiatePaymentExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/initiate-a-payment-v-3/", "Initiate Payment API");
            break;
                    
        case "6":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Complete Payment");
                    Console.ResetColor();
                    await PaymentExamples.CompletePaymentExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/complete-a-payment-v-3/", "Complete Payment API");
            break;
                    
        case "7":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Resend OTP");
                    Console.ResetColor();
                    await PaymentExamples.ResendOtpExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/resend-an-otp-v-3/", "Resend OTP API");
            break;
                    
                // Payment Links API
        case "10":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Create Payment Link");
                    Console.ResetColor();
                    await PaymentLinkExamples.CreatePaymentLinkExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/create-a-payment-link-v-3/", "Create Payment Link API");
            break;
                    
        case "11":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Update Payment Link");
                    Console.ResetColor();
                    await PaymentLinkExamples.UpdatePaymentLinkExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/update-a-payment-link-v-3/", "Update Payment Link API");
            break;
                    
        case "12":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Payment Link Enquiry");
                    Console.ResetColor();
                    await PaymentLinkExamples.EnquiryPaymentLinkExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/payment-link-enquiry-v-3/", "Payment Link Enquiry API");
            break;
                    
        case "13":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Payment Link Actions");
                    Console.ResetColor();
                    await PaymentLinkExamples.PerformPaymentLinkActionsExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/payment-link-actions-v-3/", "Payment Link Actions API");
                    break;
                    
                // Addresses API - cases 14-20
                case "14":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("List Addresses");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Query Parameters: user_id, amount, currency");
                    Console.ResetColor();
                    await AddressExamples.ListAddressesExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/list-addresses-v-3/", "List Addresses API");
                    break;
                    
                case "15":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Create Address");
                    Console.ResetColor();
                    await AddressExamples.CreateAddressExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/create-an-address-v-3/", "Create Address API");
                    break;
                    
                case "16":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Update Address");
                    Console.ResetColor();
                    await AddressExamples.UpdateAddressExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/update-an-address-v-3/", "Update Address API");
                    break;
                    
                case "17":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Delete Address");
                    Console.ResetColor();
                    await AddressExamples.DeleteAddressExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/delete-an-address-v-3/", "Delete Address API");
                    break;
                    
                case "18":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Import Addresses");
                    Console.ResetColor();
                    await AddressExamples.ImportAddressesExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/import-addresses-v-3/", "Import Addresses API");
                    break;
                    
                case "19":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Check Address Eligibility");
                    Console.ResetColor();
                    await AddressExamples.CheckAddressEligibilityExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/check-address-eligibility-v-3/", "Check Address Eligibility API");
                    break;
                    
                case "20":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Link Order to Address");
                    Console.ResetColor();
                    await AddressExamples.LinkAddressWithOrderExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/link-address-with-order-v-3/", "Link Address with Order API");
                    break;
                    
                // Refunds API
                case "21":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Initiate Refund");
                    Console.ResetColor();
                    await RefundExamples.InitiateRefundExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/refund-a-payment-v-3/", "Initiate Refund API");
                    break;
                    
                // Transactions API
                case "22":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Transaction Enquiry");
                    Console.ResetColor();
                    await TransactionStatusExamples.TransactionEnquiryExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/transaction-enquiry-v-3/", "Transaction Enquiry API");
                    break;
                    
                // Checkout Utilities API - cases 23-31
                case "23":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("List Payment Modes");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.ListPaymentModesExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/list-of-payment-modes-v-3/", "List Payment Modes API");
                    break;
                    
                case "24":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("List Banks");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.ListBanksExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/list-of-banks-v-3/", "List Banks API");
                    break;
                    
                case "25":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("List Wallets");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.ListWalletsExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/list-of-wallets-v-3/", "List Wallets API");
                    break;
                    
                case "26":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("List EMIs");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.ListEMIsExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/list-of-em-is-v-3/", "List EMIs API");
                    break;
                    
                case "27":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Get Offers");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.GetOffersExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/offers-v-3/", "Get Offers API");
                    break;
                    
                case "28":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Get Card BIN Data");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.GetCardBinDataExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/get-card-bin-data-v-3/", "Get Card BIN Data API");
                    break;
                    
                case "29":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Get Card Details");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.GetCardDetailsExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/get-card-details-v-3/", "Get Card Details API");
                    break;
                    
                case "30":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Validate UPI VPA");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.ValidateUpiVpaExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/validate-upi-vpa-v-3/", "Validate UPI VPA API");
                    break;
                    
                case "31":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Get UPI App Details");
                    Console.ResetColor();
                    await CheckoutUtilitiesExamples.GetUpiAppDetailsExample(api);
                    break;
                    
                // Webhooks
                case "32":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Webhook Handling");
                    Console.ResetColor();
                    WebhookExamples.DisplayWebhookInfo();
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/guides/integration/webhooks-and-transaction-enquiry/", "Webhooks Documentation");
                    break;
                    
                // Examples
                case "33":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Encryption Examples");
                    Console.ResetColor();
                    await EncryptionExamples.RunAllExamples(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/guides/integration/encrypt-decrypt/", "Encryption/Decryption Guide");
            break;

                // Pre-auth (capture / void)
                case "8":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Capture (pre-auth)");
                    Console.ResetColor();
                    await PaymentExamples.CaptureExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/introduction/", "Capture Payment API");
                    break;

                case "9":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Void (pre-auth)");
                    Console.ResetColor();
                    await PaymentExamples.VoidExample(api);
                    Helpers.PrintDocLink("https://nimbbl.biz/docs/api-reference/introduction/", "Void Payment API");
                    break;

        default:
                    Helpers.PrintError("Invalid choice. Please select a number from 0-33.\n");
            break;
    }
}
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
            // Logging is always enabled, so always show log file location
            Console.WriteLine("Check logs/nimbbl_debug.log for details.\n");
        }
        
        Helpers.PrintSeparator();
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
        Console.WriteLine();
    }
}
catch (FileNotFoundException ex)
{
    Helpers.PrintError($"{ex.Message}\n");
}
catch (Exception ex)
{
    Helpers.PrintError($"Unexpected error: {ex.Message}\n");
    Console.WriteLine($"Stack trace: {ex.StackTrace}\n");
}

static void PrintMenu()
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("\nSelect an API to test:\n");
    Console.ResetColor();
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Authentication ===");
    Console.ResetColor();
    Console.WriteLine("1.  Generate Token\n");
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Orders API ===");
    Console.ResetColor();
    Console.WriteLine("2.  Create Order");
    Console.WriteLine("3.  Get Order by ID");
    Console.WriteLine("4.  Get Order by Invoice ID\n");
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Payments API ===");
    Console.ResetColor();
    Console.WriteLine("5.  Initiate Payment");
    Console.WriteLine("6.  Complete Payment");
    Console.WriteLine("7.  Resend OTP");
    Console.WriteLine("8.  Capture (pre-auth)");
    Console.WriteLine("9.  Void (pre-auth)\n");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Payment Links API ===");
    Console.ResetColor();
    Console.WriteLine("10. Create Payment Link");
    Console.WriteLine("11. Update Payment Link");
    Console.WriteLine("12. Payment Link Enquiry");
    Console.WriteLine("13. Payment Link Actions\n");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Addresses API ===");
    Console.ResetColor();
    Console.WriteLine("14. List Addresses");
    Console.WriteLine("15. Create Address");
    Console.WriteLine("16. Update Address");
    Console.WriteLine("17. Delete Address");
    Console.WriteLine("18. Import Addresses");
    Console.WriteLine("19. Check Address Eligibility");
    Console.WriteLine("20. Link Order to Address\n");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Refunds API ===");
    Console.ResetColor();
    Console.WriteLine("21. Initiate Refund\n");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Transactions API ===");
    Console.ResetColor();
    Console.WriteLine("22. Transaction Enquiry\n");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Checkout Utilities API ===");
    Console.ResetColor();
    Console.WriteLine("23. List Payment Modes");
    Console.WriteLine("24. List Banks");
    Console.WriteLine("25. List Wallets");
    Console.WriteLine("26. List EMIs");
    Console.WriteLine("27. Get Offers");
    Console.WriteLine("28. Get Card BIN Data");
    Console.WriteLine("29. Get Card Details");
    Console.WriteLine("30. Validate UPI VPA");
    Console.WriteLine("31. Get UPI App Details\n");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Webhooks ===");
    Console.ResetColor();
    Console.WriteLine("32. Webhook Handling\n");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== Examples ===");
    Console.ResetColor();
    Console.WriteLine("33. Encryption Examples\n");
    
    Console.WriteLine("0.  Exit\n\n");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Enter your choice: ");
    Console.ResetColor();
}
