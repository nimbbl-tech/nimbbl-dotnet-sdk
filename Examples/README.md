# Nimbbl .NET SDK - Sample Application

This directory contains comprehensive examples demonstrating how to use the Nimbbl .NET SDK.

**Requires .NET 6.0+**

## Framework-Agnostic Design

**Important:** These plain .NET examples work in **ALL .NET frameworks**!

[OK] **Works directly in:**

- ASP.NET Core
- .NET Console Applications
- .NET MAUI
- Blazor
- WPF
- WinForms
- **Any .NET application**

The SDK is 100% framework-agnostic. The examples here demonstrate core SDK usage patterns that work everywhere. You can use these examples as-is in any framework, or adapt them to follow framework-specific best practices (Dependency Injection, Service Registration, etc.).

## Structure

```
Examples/
├── README.md                      # This file
├── Program.cs                     # Main entry point (interactive CLI menu)
├── .env                           # Configuration file (create from .env.example)
├── .env.example                   # Configuration template
├── Helpers.cs                     # Helper functions for console I/O
├── GenerateTokenExample.cs        # Token generation example
├── OrderExamples.cs               # Order management examples
├── PaymentExamples.cs             # Payment processing examples
├── PaymentLinkExamples.cs         # Payment link examples
├── AddressExamples.cs             # Address management examples
├── RefundExamples.cs              # Refund processing examples
├── TransactionStatusExamples.cs   # Transaction status enquiry
├── CheckoutUtilitiesExamples.cs   # Checkout utilities examples
├── EncryptionExamples.cs          # Encryption/decryption examples
├── WebhookExamples.cs             # Webhook handler example
└── ExceptionHandlingExamples.cs  # Exception handling examples
```

## Quick Start

### 1. Setup Configuration

```bash
# Copy the example .env file
cp .env.example .env

# Edit .env with your credentials
# Use your preferred editor
```

### 2. Install Dependencies

```bash
# From the SDK root directory
dotnet restore
```

### 3. Run Examples

```bash
# Run main examples (interactive CLI menu)
dotnet run --project Examples

# Or run specific examples by modifying Program.cs
```

## Configuration

Edit `.env` file with your Nimbbl credentials:

```env
# Required: API Credentials
NIMBBL_ACCESS_KEY=your_access_key_here
NIMBBL_ACCESS_SECRET=your_access_secret_here

# Optional: API Configuration
NIMBBL_API_HOST=https://api.nimbbl.tech

# Optional: Logging Configuration
NIMBBL_ENABLE_LOGGING=false
NIMBBL_LOG_FILE=logs/nimbbl_debug.log
NIMBBL_DEBUG_LOGGING=false
```

**Note:** 
- For UAT/Sandbox, use `NIMBBL_API_HOST=https://apipp.nimbbl.tech`
- Webhook verification uses `NIMBBL_ACCESS_SECRET` automatically
- Set `NIMBBL_ENABLE_LOGGING=true` to see detailed request/response logs
- The SDK does NOT load `.env` files - sample apps must read environment variables and pass them as parameters to `NimbblApi.Initialize()`

## Complete Example Coverage

This sample application includes comprehensive examples for:

### Core APIs
- [OK] **Orders API** - Create, retrieve orders
- [OK] **Payments API** - Initiate, complete payments, resend OTP
- [OK] **Payment Links API** - Create, update, manage payment links
- [OK] **Addresses API** - Manage customer addresses
- [OK] **Refunds API** - Process refunds
- [OK] **Transactions API** - Transaction enquiry (by order_id, invoice_id, or transaction_id)
- [OK] **Checkout Utilities API** - Payment modes, banks, wallets, EMIs, offers

### Advanced Features
- [OK] **Webhook Handling** - Signature verification and event processing
- [OK] **Event Logging** - Automatic and custom event logging
- [OK] **Error Handling** - Comprehensive exception handling examples
- [OK] **Encryption/Decryption** - Data encryption examples (AES-GCM implementation pending)

## Examples

### Quick Example: Order Creation

**Note:** Orders API uses **Order Token** (obtained from order creation response). Initial order creation uses **Merchant Token**.

```csharp
using Nimbbl.Sdk.Rest.Api;
using System.Text.Json;

// Load .env file (sample app responsibility)
EnvLoader.LoadEnvFile();

// Read environment variables and pass to SDK
var accessKey = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY");
var accessSecret = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET");
var apiHost = Environment.GetEnvironmentVariable("NIMBBL_API_HOST");

// Initialize API with parameters from environment variables
var api = NimbblApi.Initialize(
    accessKey: accessKey!,
    accessSecret: accessSecret!,
    apiHost: apiHost
);

// Or initialize with explicit credentials
var api = new NimbblApi(
    key: "your_access_key",
    secret: "your_access_secret",
    baseUrl: "https://api.nimbbl.tech/api/"
);

// Generate merchant token
var tokenResponse = await api.Auth().GenerateTokenAsync();
var merchantToken = tokenResponse.TryGetProperty("token", out var tokenProp) 
    ? tokenProp.GetString() 
    : null;

// Set bearer token
api.SetBearerToken(merchantToken);

// Create order
var orderData = new Dictionary<string, object?>
{
    ["invoice_id"] = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
    ["total_amount"] = 100.00m,
    ["amount_before_tax"] = 90.00m,
    ["tax"] = 10.00m,
    ["currency"] = "INR",
    ["user"] = new Dictionary<string, object?>
    {
        ["email"] = "customer@example.com",
        ["first_name"] = "John",
        ["last_name"] = "Doe",
        ["country_code"] = "+91",
        ["mobile_number"] = "9876543210"
    }
};

var order = await api.Orders().CreateOrderAsync(orderData);

// Extract order token for subsequent operations
if (!order.TryGetProperty("error", out _))
{
    var orderToken = order.TryGetProperty("token", out var ot) ? ot.GetString() : null;
    var orderId = order.TryGetProperty("order_id", out var oid) 
        ? oid.GetString() 
        : (order.TryGetProperty("nimbbl_order_id", out var noid) ? noid.GetString() : null);
    
    Console.WriteLine($"Order created! Order ID: {orderId}");
    Console.WriteLine($"Order Token: {orderToken}");
}
```

For complete examples, see:
- `OrderExamples.cs` - Order creation and retrieval
- `PaymentExamples.cs` - Payment processing
- `PaymentLinkExamples.cs` - Payment link management
- `AddressExamples.cs` - Address management
- `RefundExamples.cs` - Refund processing
- `WebhookExamples.cs` - Webhook handling
- `EncryptionExamples.cs` - Encryption/decryption

All examples are self-contained and can be run standalone or called from `Program.cs`.

## Security Notes

1. **Never commit `.env`** - It contains sensitive credentials
2. **Use environment variables** in production
3. **Verify webhook signatures** before processing
4. **Validate all user input** before sending to API
5. **Use HTTPS** for all API communications

## Documentation

- [Nimbbl API Documentation](https://nimbbl.biz/docs/api-reference/introduction/)
- [SDK README](../README.md)

## Troubleshooting

### Common Issues

1. **Build errors**: Run `dotnet restore` from SDK root
2. **API errors**: Check your credentials in `.env` file
3. **Webhook verification fails**: Ensure `NIMBBL_ACCESS_SECRET` is correct (used for webhook verification)
4. **Configuration not found**: Ensure `.env` file exists (copy from `.env.example`)

## Using the SDK in Your Application

The SDK is designed to be framework-agnostic and can be used in any .NET application:

### ASP.NET Core

```csharp
// In Startup.cs or Program.cs
using Nimbbl.Sdk.Rest.Extensions;

// Read environment variables (sample app responsibility)
var accessKey = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY");
var accessSecret = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET");
var apiHost = Environment.GetEnvironmentVariable("NIMBBL_API_HOST");

// Register Nimbbl SDK services with parameters
services.AddNimbbl(
    accessKey: accessKey!,
    accessSecret: accessSecret!,
    apiHost: apiHost
);

// In your controller
public class PaymentController : ControllerBase
{
    private readonly NimbblApi _api;
    
    public PaymentController(NimbblApi api)
    {
        _api = api;
    }
    
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] Dictionary<string, object?> orderData)
    {
        var order = await _api.Orders().CreateOrderAsync(orderData);
        return Ok(order);
    }
}
```

### Console Application

```csharp
// Load .env file (sample app responsibility)
EnvLoader.LoadEnvFile();

// Read environment variables and pass to SDK
var accessKey = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY");
var accessSecret = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET");
var apiHost = Environment.GetEnvironmentVariable("NIMBBL_API_HOST");

// Initialize with parameters from environment variables
var api = NimbblApi.Initialize(
    accessKey: accessKey!,
    accessSecret: accessSecret!,
    apiHost: apiHost
);

// Or with explicit credentials
var api = new NimbblApi(
    key: "your_access_key",
    secret: "your_access_secret",
    baseUrl: "https://api.nimbbl.tech/api/"
);

var order = await api.Orders().CreateOrderAsync(orderData);
```

## Support

For issues or questions:
- Check the [API Documentation](https://nimbbl.biz/docs/api-reference/introduction/)
- Review the [SDK Documentation](../README.md)
- Contact support: support@nimbbl.biz
