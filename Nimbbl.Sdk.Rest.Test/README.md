# Nimbbl .NET SDK Tests

This directory contains unit tests for the Nimbbl .NET SDK.

## Prerequisites

- .NET 8.0 SDK or later
- Access to Nimbbl test API credentials

## Test Credentials

All tests read credentials from environment variables. You can provide them in two ways:

### Option 1: Using .env File (Recommended)

1. Copy the example file:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` file with your credentials:
   ```bash
   NIMBBL_ACCESS_KEY=your_access_key
   NIMBBL_ACCESS_SECRET=your_access_secret
   NIMBBL_API_HOST=https://api.nimbbl.tech  # Optional
   ENCRYPT_PAYLOAD=false  # Optional, defaults to false
   ```

3. Run tests - the `.env` file will be automatically loaded:
   ```bash
   dotnet test
   ```

**Note:** The `.env` file is automatically searched in the test project directory and parent directories (up to 5 levels).

### Option 2: Using Environment Variables

Set environment variables before running tests:

```bash
export NIMBBL_ACCESS_KEY="your_access_key"
export NIMBBL_ACCESS_SECRET="your_access_secret"
export NIMBBL_API_HOST="https://api.nimbbl.tech"  # Optional
export ENCRYPT_PAYLOAD="false"  # Optional, defaults to false
dotnet test
```

**Environment Variables:**
- `NIMBBL_ACCESS_KEY` - Your Nimbbl access key (required)
- `NIMBBL_ACCESS_SECRET` - Your Nimbbl access secret (required)
- `NIMBBL_API_HOST` - API host URL (optional, defaults to production if not set)
- `ENCRYPT_PAYLOAD` - Enable payload encryption (optional, defaults to false)

## Running Tests

### Using .NET CLI (Recommended)

Navigate to the test project directory:

```bash
cd Nimbbl.Sdk.Rest.Test
```

Run all tests:

```bash
dotnet test
```

Run tests with verbose output:

```bash
dotnet test --verbosity normal
```

Run a specific test class:

```bash
dotnet test --filter "FullyQualifiedName~OrderTest"
```

Run a specific test method:

```bash
dotnet test --filter "FullyQualifiedName~OrderTest.ShouldCreateOrder"
```

Run tests with code coverage:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### From Solution Root

You can also run tests from the solution root directory:

```bash
cd nimbbl-dotnet-sdk
dotnet test Nimbbl.Sdk.Rest.Test/Nimbbl.Sdk.Rest.Test.csproj
```

### Using Visual Studio

1. Open the solution (`nimbbl-dotnet-sdk.sln` or open the folder)
2. Open Test Explorer (Test → Test Explorer)
3. Click "Run All Tests" or select specific tests to run

### Using Visual Studio Code

1. Install the C# extension
2. Open the test project folder
3. Use the command palette (Cmd+Shift+P / Ctrl+Shift+P)
4. Run: "Test: Run All Tests"

## Test Files

The test suite includes the following test files:

- **`ApiClientTest.cs`** - API client initialization and token generation
- **`AuthTest.cs`** - Authentication service (token generation, refresh)
- **`OrderTest.cs`** - Order creation and retrieval
- **`TransactionTest.cs`** - Transaction enquiry operations
- **`PaymentTest.cs`** - Payment initiation, completion, and OTP resend
- **`PaymentLinkTest.cs`** - Payment link CRUD operations and actions
- **`RefundTest.cs`** - Refund initiation (full and partial)
- **`AddressTest.cs`** - Address management operations
- **`CheckoutUtilitiesTest.cs`** - Checkout utilities (payment modes, banks, wallets, etc.)
- **`SignatureVerifierTest.cs`** - Signature verification for webhooks
- **`ExceptionTest.cs`** - Exception handling and types
- **`NimbblClientTest.cs`** - NimbblApi initialization and configuration
- **`EncryptionTest.cs`** - Tests for encryption-enabled scenarios (when `ENCRYPT_PAYLOAD=true`)

## Test Structure

Each test file follows this pattern:

- Inherits from `TestBase` which reads credentials from environment variables
- Uses `NimbblApi.Initialize()` for setup (via `TestBase`)
- Tests both success and error scenarios
- Uses xUnit testing framework
- Tests make actual API calls (integration tests)

### Encryption Tests

The `EncryptionTest.cs` file contains tests that verify encryption functionality when `ENCRYPT_PAYLOAD=true`. These tests:

- Initialize the API with `encryptPayload: true` (does not inherit from `TestBase`)
- Test all methods that support encryption:
  - `Orders.CreateOrderAsync()` - Order creation with encrypted payload
  - `Transactions.TransactionEnquiryAsync()` - Transaction enquiry with encrypted payload
  - `CheckoutUtilities.ListBanksAsync()` - List banks with encrypted payload
  - `CheckoutUtilities.ListWalletsAsync()` - List wallets with encrypted payload
  - `Refunds.InitiateRefundAsync()` - Refund initiation with encrypted payload

To run encryption tests specifically:

```bash
dotnet test --filter "FullyQualifiedName~EncryptionTest"
```

**Note:** Encryption tests require valid API credentials and will make real API calls with encrypted payloads.

## Notes

- **All tests require environment variables** - Tests will fail with clear error messages if `NIMBBL_ACCESS_KEY` or `NIMBBL_ACCESS_SECRET` are not set
- Tests make real API calls to the Nimbbl API (production or test environment based on `NIMBBL_API_HOST`)
- Some tests may fail if test data doesn't exist in the API
- Tests are designed to verify SDK functionality, not API behavior
- Ensure you have network access to your configured API host

## Troubleshooting

### Tests fail with "environment variable is required" errors

- **Option 1:** Create a `.env` file in the test project directory (copy from `.env.example`)
- **Option 2:** Set environment variables before running tests: `export NIMBBL_ACCESS_KEY="your_key"`
- Ensure `NIMBBL_ACCESS_KEY` and `NIMBBL_ACCESS_SECRET` are set correctly

### Tests fail with authentication errors

- Verify the environment variables are set correctly
- Check network connectivity to the API host

### Tests fail with "not found" errors

- Some tests use hardcoded test IDs that may not exist
- These tests verify error handling rather than successful operations

### Build errors

- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Restore packages: `dotnet restore`
