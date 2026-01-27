# Nimbbl .NET SDK

Official .NET SDK for integrating with the Nimbbl Payment Gateway API. This SDK provides a comprehensive set of APIs for payment processing, order management, and checkout utilities.

## Features

- **Orders API** - Create and retrieve orders
- **Payments API** - Initiate, complete payments, and resend OTP
- **Payment Links API** - Create, update, and manage payment links
- **Addresses API** - Manage customer addresses
- **Refunds API** - Process refunds (full and partial)
- **Checkout Utilities API** - Payment modes, banks, wallets, EMIs, offers, card BIN, UPI validation
- **Transactions API** - Transaction enquiry
- **Global headers & explicit bearer token** - `AddHeader`, `SetBearerToken`
- **Masked logging** - Request/response headers and bodies masked for sensitive data
- **Typed exceptions** - Authentication, BadRequest, NotFound, RateLimit, Server, Api (namespace `Nimbbl.Sdk.Rest.Exception`)

## Prerequisites

- .NET 6.0 or later
- Visual Studio 2022 or Visual Studio Code with C# extension

## Installation

### Build from Source

```bash
cd nimbbl-dotnet-sdk
dotnet build
```

### Package Installation

After building, create a NuGet package:

```bash
dotnet pack --configuration Release
```

See [BUILD_RUN_PACKAGE.md](BUILD_RUN_PACKAGE.md) for detailed build, run, and packaging instructions.

## Configuration

The SDK uses environment variables for configuration. You can provide them via:

1. **`.env` file** (recommended for development) - Place in your project root directory
2. **Environment variables** (recommended for production)

### Required Environment Variables

- `NIMBBL_ACCESS_KEY` - Your Nimbbl access key
- `NIMBBL_ACCESS_SECRET` - Your Nimbbl access secret

### Optional Environment Variables

- `NIMBBL_DEBUG_LOGGING` - Enable/disable debug logging (defaults to `false`). Note: INFO/WARNING/ERROR logs are always emitted; debug logs show unmasked raw payloads.
- `NIMBBL_LOG_FILE` - Log file path (defaults to `logs/nimbbl_debug.log`)
- `NIMBBL_CHECKOUT_HOST` - Override checkout host (optional)

**Note:** The SDK uses the production API host (`https://api.nimbbl.tech`) by default. For testing environments, you can configure a custom base URL when initializing the SDK.

## Quick Start

### Option 1: Using NimbblApi.Initialize() (Recommended)

```c#
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;

// Load .env file (if using .env)
EnvLoader.LoadEnvFile();

// Initialize SDK from environment variables
var api = NimbblApi.Initialize(
    accessKey: Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY")!,
    accessSecret: Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET")!,
    debugLogging: false,   // Set to true to enable debug logging (unmasked raw JSON)
    logFilePath: "logs/nimbbl_debug.log"
);

// Optional: Add global headers
api.AddHeader("Nimbbl-API", "1");

// Optional: Set explicit bearer token (skip auth flow) with optional expiry
api.SetBearerToken("your_order_or_merchant_token", expiresAtUtc: DateTime.UtcNow.AddMinutes(20));

// Example: create order
var order = await api.Orders().CreateOrderAsync(new Dictionary<string, object?>
{
    ["invoice_id"] = "INV-12345",
    ["total_amount"] = 400.0,  // Amount as double (not decimal)
    ["currency"] = "INR"
});

// Handle errors
try
{
    var payment = await api.Payments().InitiatePaymentAsync(new Dictionary<string, object?>
    {
        ["order_id"] = "order_id",
        ["payment_mode"] = "netbanking",
        ["bank_code"] = "HDFC"
    });
}
catch (NimbblException ex)
{
    Console.WriteLine($"Nimbbl error: {ex.ErrorCode} - {ex.Message}");
}
```

### Option 2: Using Dependency Injection (ASP.NET Core)

```c#
using Nimbbl.Sdk.Rest.Extensions;

// In Program.cs
builder.Services.AddNimbbl(
    accessKey: Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY")!,
    accessSecret: Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET")!,
    debugLogging: false, // Set to true to enable debug logging (unmasked raw JSON)
    logFilePath: "logs/nimbbl_debug.log"
);

// In your controller/service
public class MyController : ControllerBase
{
    private readonly NimbblApi _api;
    
    public MyController(NimbblApi api)
    {
        _api = api;
    }
    
    public async Task<IActionResult> CreateOrder()
    {
        var order = await _api.Orders().CreateOrderAsync(new Dictionary<string, object?>
        {
            ["invoice_id"] = "INV-12345",
            ["total_amount"] = 400.0,  // Amount as double (not decimal)
            ["currency"] = "INR"
        });
        return Ok(order);
    }
}
```

## API Usage Examples

### Orders API (dictionary payloads)

```c#
// Create order
var order = await api.Orders().CreateOrderAsync(new Dictionary<string, object?>
{
    ["invoice_id"] = "INV-12345",
    ["total_amount"] = 400.0,  // Amount as double (not decimal)
    ["currency"] = "INR"
});

// Get order by ID
var order = await api.Orders().GetOrderByIdAsync("order_id");

// Get order by invoice ID
var order = await api.Orders().GetOrderByInvoiceIdAsync("invoice_id");
```

### Payments API (dictionary payloads)

```c#
// Initiate payment
var payment = await api.Payments().InitiatePaymentAsync(new Dictionary<string, object?>
{
    ["order_id"] = "order_id",
    ["payment_mode"] = "netbanking",
    ["bank_code"] = "HDFC"
});

// Complete payment
var result = await api.Payments().CompletePaymentAsync(new Dictionary<string, object?>
{
    ["order_id"] = "order_id",
    ["otp"] = "123456"
});

// Resend OTP
var otpResult = await api.Payments().ResendPaymentOtpAsync(new Dictionary<string, object?>
{
    ["order_id"] = "order_id"
});
```

### Payment Links API (dictionary payloads)

```c#
// Create payment link
var paymentLink = await api.PaymentLinks().CreatePaymentLinkAsync(new Dictionary<string, object?>
{
    ["invoice_id"] = "INV-123",
    ["total_amount"] = 1000.0,  // Amount as double (not decimal)
    ["currency"] = "INR",
    ["description"] = "Payment for order",
    ["expires_at"] = DateTime.Now.AddDays(7)
});

// Update payment link
var updated = await api.PaymentLinks().UpdatePaymentLinkAsync(new Dictionary<string, object?>
{
    ["total_amount"] = 1500.0  // Amount as double (not decimal)
});

// Enquiry
var enquiry = await api.PaymentLinks().EnquiryPaymentLinkAsync(new Dictionary<string, object?>
{
    ["payment_link_id"] = "link_id"
});
```

### Addresses API (dictionary payloads)

```c#
// List addresses
var addresses = await api.Addresses().ListAddressesAsync(new Dictionary<string, object?>
{
    ["user_id"] = "user_id",
    ["amount"] = 1000.0,  // Amount as double (not decimal)
    ["currency"] = "INR"
});

// Create address
var address = await api.Addresses().CreateAddressAsync(new Dictionary<string, object?>
{
    ["user_id"] = "user_id",
    ["address1"] = "123 Main St",
    ["city"] = "Mumbai",
    ["state"] = "Maharashtra",
    ["pincode"] = "400001",
    ["address_type"] = "home"
});

// Update address
var updated = await api.Addresses().UpdateAddressAsync("address_id", new Dictionary<string, object?>
{
    ["city"] = "Delhi"
});

// Delete address
var result = await api.Addresses().DeleteAddressAsync("address_id");
```

### Refunds API (dictionary payloads)

```c#
// Initiate refund (full)
var refund = await api.Refunds().InitiateRefundAsync(new Dictionary<string, object?>
{
    ["transaction_id"] = "transaction_id"
});

// Initiate partial refund
var partialRefund = await api.Refunds().InitiateRefundAsync(new Dictionary<string, object?>
{
    ["transaction_id"] = "transaction_id",
    ["refund_amount"] = 50.0,  // Amount as double (not decimal)
    ["comment"] = "Partial refund"
});
```

### Transactions API (enquiry)

```c#
// Enquiry by transaction ID
var txn = await api.Transactions().TransactionEnquiryAsync(new Dictionary<string, object?> { ["transaction_id"] = "transaction_id" });
// Enquiry by order ID or invoice_id
var txnByOrder = await api.Transactions().TransactionEnquiryAsync(new Dictionary<string, object?> { ["order_id"] = "order_id" });
```

### Checkout Utilities API (dictionary payloads)

```c#
// List payment modes
var modes = await api.CheckoutUtilities().ListPaymentModesAsync(new Dictionary<string, object?>
{
    ["order_id"] = "order_id"
});

// List banks
var banks = await api.CheckoutUtilities().ListBanksAsync(new Dictionary<string, object?>
{
    ["order_id"] = "order_id",
    ["amount"] = 1000.0,  // Amount as double (not decimal)
    ["currency"] = "INR"
});

// Validate UPI VPA
var vpa = await api.CheckoutUtilities().ValidateUpiVpaAsync(new Dictionary<string, object?>
{
    ["vpa"] = "user@paytm"
});

// Note: Pass the order token via SetBearerToken if not using automatic auth
api.SetBearerToken("order_token_here", expiresAtUtc: DateTime.UtcNow.AddMinutes(20));
```

### Webhook
Webhooks are supported. Use `Common/SignatureVerifier` for signature verification:
- `SignatureVerifier.VerifySignature(JsonElement attributes, string secret)` — verifies parsed JSON attributes.
 - `SignatureVerifier.VerifyWebhookSignature(string payload, string secret, out JsonElement parsed)` — parses and verifies raw webhook payload.

## Project Structure

```text
Nimbbl.Sdk.Rest/
├── Api/                  # SDK entrypoint (NimbblApi)
├── Common/               # Shared helpers, constants, signature verifier, encryption
├── Exception/            # Typed SDK exceptions (NimbblException, NotFoundException, etc.)
├── Extensions/           # DI extensions (AddNimbbl)
├── Log/                  # Logger implementation and helpers
├── RestClient/           # HTTP client and low-level Rest primitives (ApiClient, NimbblClient)
├── Services/             # High-level API services
│   ├── Orders.cs
│   ├── Payments.cs
│   ├── PaymentLinks.cs
│   ├── Addresses.cs
│   ├── Refunds.cs
│   ├── CheckoutUtilities.cs
│   └── Transactions.cs
└── Nimbbl.Sdk.Rest.csproj
```

## Documentation

- [BUILD_RUN_PACKAGE.md](BUILD_RUN_PACKAGE.md) - Build, run, and package guide
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - **Testing guide for testers** - How to test examples and sample app locally
- [MerchantSampleApp/README.md](MerchantSampleApp/README.md) - Sample application guide
- [Examples/README.md](Examples/README.md) - Examples and CLI menu guide
- [Nimbbl API Documentation](https://nimbbl.biz/docs/api-reference/introduction/) - Official API reference

## Version

Current Version: 1.3.5-rc6

## License

See LICENSE file for details.
