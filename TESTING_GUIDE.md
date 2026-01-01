# Testing Guide for Nimbbl .NET SDK

This guide provides step-by-step instructions for testers to test the Nimbbl .NET SDK **Examples** and **Merchant Sample App** locally on their system using both **published NuGet package** and **local source code**.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Testing Examples App](#testing-examples-app)
3. [Testing Merchant Sample App](#testing-merchant-sample-app)
4. [Testing Checklist](#testing-checklist)
5. [Troubleshooting](#troubleshooting)

## Prerequisites

Before you begin testing, ensure you have the following installed:

### Required Software

1. **.NET 8.0 SDK or later**
   - Download from: [.NET Downloads](https://dotnet.microsoft.com/download)
   - Verify installation: `dotnet --version` (should show 8.0.x or later)

2. **Code Editor** (choose one)
   - Visual Studio 2022 (recommended for Windows)
   - Visual Studio Code with C# extension

3. **Git** (for cloning repository, if testing with local source)
   - Download from: [Git Downloads](https://git-scm.com/downloads)
   - Verify installation: `git --version`

### Required Credentials

- **Nimbbl Access Key** - Your Nimbbl merchant access key
- **Nimbbl Access Secret** - Your Nimbbl merchant access secret

**Note:** For testing, you can use:

- **Production**: `https://api.nimbbl.tech`
- **preprod**: `https://apipp.nimbbl.tech`
- **QA**: `https://<servername>api.nimbbl.tech`

## Testing Examples App

The `Examples` directory contains a comprehensive console application with interactive menu for testing all SDK features.

### Option 1: Testing with Published NuGet Package (Examples App)

#### Step 1: Get the Repository (Examples - NuGet)

```bash
# Clone the repository
git clone https://github.com/nimbbl-tech/nimbbl-dotnet-sdk.git
cd nimbbl-dotnet-sdk

# Or if you already have it, navigate to the directory
cd /path/to/nimbbl-dotnet-sdk
```

#### Step 2: Switch Examples to Use NuGet Package

1. **Navigate to Examples directory:**

   ```bash
   cd Examples
   ```

2. **Edit `Examples.csproj` file:**

   **Remove the project reference:**

   ```xml
   <!-- Remove or comment out this line -->
   <!-- <ProjectReference Include="..\Nimbbl.Sdk.Rest\Nimbbl.Sdk.Rest.csproj" /> -->
   ```

   **Add NuGet package reference:**

   ```xml
   <ItemGroup>
     <PackageReference Include="Nimbbl.Sdk.Rest" Version="1.3.5-rc2" />
   </ItemGroup>
   ```

3. **Restore packages:**

   ```bash
   dotnet restore
   ```

#### Step 3: Configure Credentials (Examples - NuGet)

1. **Create `.env` file:**

   ```bash
   # Create .env file manually (or copy from .env.example if it exists)
   # If .env.example doesn't exist, create .env file directly
   touch .env
   # Or on Windows: type nul > .env
   ```

2. **Edit `.env` file with your credentials:**

   ```env
   NIMBBL_ACCESS_KEY=your_access_key_here
   NIMBBL_ACCESS_SECRET=your_access_secret_here
   NIMBBL_ENABLE_LOGGING=true
   NIMBBL_DEBUG_LOGGING=false
   NIMBBL_LOG_FILE=logs/nimbbl_debug.log
   ```

#### Step 4: Run Examples

```bash
dotnet run
```

**Expected Output:**

```text
=== Nimbbl .NET SDK Examples ===
1. Generate Merchant Token
2. Create Order
3. Get Order by ID
4. Initiate Payment
5. Complete Payment
6. Create Payment Link
7. List Addresses
8. Initiate Refund
9. Transaction Enquiry
10. List Payment Modes
11. Webhook Handler Example
12. Exception Handling Examples
0. Exit

Select an option:
```

### Option 2: Testing with Local Source Code (Examples App)

#### Step 1: Get the Repository (Examples - Local Source)

```bash
# Clone the repository
git clone https://github.com/nimbbl-tech/nimbbl-dotnet-sdk.git
cd nimbbl-dotnet-sdk

# Or if you already have it, navigate to the directory
cd /path/to/nimbbl-dotnet-sdk
```

#### Step 2: Build the SDK (Examples - Local Source)

```bash
# Navigate to SDK directory
cd Nimbbl.Sdk.Rest

# Restore dependencies
dotnet restore

# Build the SDK
dotnet build

# Or build in Release mode
dotnet build --configuration Release
```

**Expected Output:**

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
   ```

#### Step 3: Configure Examples to Use Local Source (Examples - Local Source)

1. **Navigate to Examples directory:**

   ```bash
   cd Examples
   ```

2. **Verify the project file** (`Examples.csproj`) has project reference:

   ```xml
   <ItemGroup>
     <ProjectReference Include="..\Nimbbl.Sdk.Rest\Nimbbl.Sdk.Rest.csproj" />
   </ItemGroup>
   ```

3. **Restore and build:**

   ```bash
   dotnet restore
   dotnet build
   ```

#### Step 4: Configure Credentials (Examples - Local Source)

1. **Create `.env` file:**

   ```bash
   # Create .env file manually (or copy from .env.example if it exists)
   # If .env.example doesn't exist, create .env file directly
   touch .env
   # Or on Windows: type nul > .env
   ```

2. **Edit `.env` file with your credentials:**

   ```env
   NIMBBL_ACCESS_KEY=your_access_key_here
   NIMBBL_ACCESS_SECRET=your_access_secret_here
   NIMBBL_ENABLE_LOGGING=true
   NIMBBL_DEBUG_LOGGING=false
   NIMBBL_LOG_FILE=logs/nimbbl_debug.log
   ```

#### Step 5: Run Examples

```bash
dotnet run
```

### Testing Individual Examples

You can test specific examples by selecting them from the interactive menu, or by modifying `Program.cs`:

```csharp
// In Program.cs, you can call specific examples directly
// Note: 'api' is the NimbblApi instance initialized in Program.cs
await GenerateTokenExample.GenerateTokenExampleAsync(api);
await OrderExamples.CreateOrderExample(api);
await PaymentExamples.InitiatePaymentExample(api);
await AddressExamples.RunAllExamples(api);  // Run all address examples
```

### What to Test in Examples App

- ✅ Generate merchant token
- ✅ Create order
- ✅ Get order by ID
- ✅ Get order by invoice ID
- ✅ Initiate payment
- ✅ Complete payment
- ✅ Resend OTP
- ✅ Create payment link
- ✅ Update payment link
- ✅ List addresses
- ✅ Create address
- ✅ Update address
- ✅ Delete address
- ✅ Initiate refund (full and partial)
- ✅ Transaction enquiry
- ✅ List payment modes
- ✅ List banks
- ✅ List wallets
- ✅ Validate UPI VPA
- ✅ Webhook handling
- ✅ Exception handling

## Testing Merchant Sample App

The `MerchantSampleApp` is a complete ASP.NET Core MVC application demonstrating real-world integration with checkout flow.

### Option 1: Testing with Published NuGet Package (Merchant Sample App)

#### Step 1: Get the Repository (Merchant Sample App - NuGet)

```bash
# Clone the repository
git clone https://github.com/nimbbl-tech/nimbbl-dotnet-sdk.git
cd nimbbl-dotnet-sdk

# Or if you already have it, navigate to the directory
cd /path/to/nimbbl-dotnet-sdk
```

#### Step 2: Switch MerchantSampleApp to Use NuGet Package

1. **Navigate to MerchantSampleApp directory:**

   ```bash
   cd MerchantSampleApp
   ```

2. **Edit `MerchantSampleApp.csproj` file:**

   **Remove the project reference:**

   ```xml
   <!-- Remove or comment out this line -->
   <!-- <ProjectReference Include="..\Nimbbl.Sdk.Rest\Nimbbl.Sdk.Rest.csproj" /> -->
   ```

   **Add NuGet package reference:**

   ```xml
   <ItemGroup>
     <PackageReference Include="Nimbbl.Sdk.Rest" Version="1.3.5-rc2" />
   </ItemGroup>
   ```

3. **Restore packages:**

   ```bash
   dotnet restore
   ```

#### Step 3: Configure Credentials (Merchant Sample App - NuGet)

##### Option A: Using .env file (Recommended)

1. **Create `.env` file in MerchantSampleApp directory:**

   ```bash
   cd MerchantSampleApp
   # Create .env file
   touch .env
   # Or on Windows: type nul > .env
   ```

2. **Edit `.env` file with your credentials:**

   ```env
   NIMBBL_ACCESS_KEY=your_access_key_here
   NIMBBL_ACCESS_SECRET=your_access_secret_here
   NIMBBL_ENABLE_LOGGING=true
   NIMBBL_DEBUG_LOGGING=false
   NIMBBL_LOG_FILE=logs/nimbbl_debug.log
   ```

##### Option B: Using Environment Variables

###### For Windows (PowerShell)

```powershell
$env:NIMBBL_ACCESS_KEY="your_access_key_here"
$env:NIMBBL_ACCESS_SECRET="your_access_secret_here"
$env:NIMBBL_ENABLE_LOGGING="true"
```

###### For Linux/Mac (Bash)

```bash
export NIMBBL_ACCESS_KEY="your_access_key_here"
export NIMBBL_ACCESS_SECRET="your_access_secret_here"
export NIMBBL_ENABLE_LOGGING="true"
```

**Note:** MerchantSampleApp reads credentials from environment variables (loaded from `.env` file via `EnvLoader.LoadEnvFile()` in `Program.cs`). The `.env` file is automatically loaded if it exists in the project directory or parent directories.

#### Step 4: Run the Application

```bash
dotnet run
```

**Expected Output:**

```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

#### Step 5: Test the Application

1. **Open browser:** Navigate to `https://localhost:5001` or `http://localhost:5000`

2. **Test Order Creation:**
   - Fill in the order form (amount, currency, customer details)
   - Click "Create Order"
   - Verify order is created successfully
   - Check for order ID and token in the response

3. **Test Checkout - Popup Mode:**
   - After order creation, click "Pay Now" (popup mode)
   - Verify checkout popup opens
   - Test payment flow
   - Verify callback handling

4. **Test Checkout - Redirect Mode:**
   - Create order with callback URL
   - Click "Pay Now" (redirect mode)
   - Verify redirect to Nimbbl checkout
   - Complete payment
   - Verify redirect back to callback URL

5. **Test Webhook:**
   - Use the webhook endpoint: `/webhook`
   - Send test webhook payloads (see `WEBHOOK_TESTING.md`)
   - Verify signature verification
   - Check webhook event processing

### Option 2: Testing with Local Source Code (Merchant Sample App)

#### Step 1: Get the Repository (Merchant Sample App - Local Source)

```bash
# Clone the repository
git clone https://github.com/nimbbl-tech/nimbbl-dotnet-sdk.git
cd nimbbl-dotnet-sdk

# Or if you already have it, navigate to the directory
cd /path/to/nimbbl-dotnet-sdk
```

#### Step 2: Build the SDK (Merchant Sample App - Local Source)

```bash
# Navigate to SDK directory
cd Nimbbl.Sdk.Rest

# Restore dependencies
dotnet restore

# Build the SDK
dotnet build

# Or build in Release mode
dotnet build --configuration Release
```

#### Step 3: Configure MerchantSampleApp to Use Local Source

1. **Navigate to MerchantSampleApp directory:**

   ```bash
   cd MerchantSampleApp
   ```

2. **Verify the project file** (`MerchantSampleApp.csproj`) has project reference:

   ```xml
   <ItemGroup>
     <ProjectReference Include="..\Nimbbl.Sdk.Rest\Nimbbl.Sdk.Rest.csproj" />
   </ItemGroup>
   ```

3. **Restore dependencies:**

   ```bash
   dotnet restore
   dotnet build
   ```

#### Step 4: Configure Credentials (Merchant Sample App - Local Source)

Follow the same configuration steps as in Option 1 (Step 3).

#### Step 5: Run the Application

```bash
dotnet run
```

#### Step 6: Test the Application

Follow the same testing steps as in Option 1 (Step 5).

### Check Logs

Logs are written to `MerchantSampleApp/logs/nimbbl_debug_YYYYMMDD.log`:

```bash
# View latest log file
tail -f logs/nimbbl_debug_*.log

# Or open in editor
cat logs/nimbbl_debug_*.log

# Windows PowerShell
Get-Content logs/nimbbl_debug_*.log -Tail 50
```

### What to Test in Merchant Sample App

- ✅ **Order Creation**
  - Create order via form
  - Order validation
  - Error handling
  - Order with line items
  - Order with callback URL

- ✅ **Checkout Flow - Popup Mode**
  - Open checkout in popup
  - Payment processing
  - Callback handling
  - Success/failure pages

- ✅ **Checkout Flow - Redirect Mode**
  - Redirect to Nimbbl checkout
  - Payment processing
  - Callback URL handling
  - Success/failure pages

- ✅ **Webhook Handling**
  - Webhook endpoint accessibility
  - Signature verification
  - Event processing
  - Error handling

- ✅ **Logging**
  - Log file creation
  - Request/response logging
  - Sensitive data masking
  - Debug logging

## Testing Checklist

Use this checklist to ensure comprehensive testing:

### Examples App Testing

- [ ] **Authentication**
  - [ ] Generate merchant token
  - [ ] Set bearer token
  - [ ] Token expiry handling

- [ ] **Orders API**
  - [ ] Create order
  - [ ] Get order by ID
  - [ ] Get order by invoice ID
  - [ ] Order with line items
  - [ ] Order with callback URL

- [ ] **Payments API**
  - [ ] Initiate payment
  - [ ] Complete payment
  - [ ] Resend OTP
  - [ ] Payment status check

- [ ] **Payment Links API**
  - [ ] Create payment link
  - [ ] Update payment link
  - [ ] Enquiry payment link

- [ ] **Addresses API**
  - [ ] List addresses
  - [ ] Create address
  - [ ] Update address
  - [ ] Delete address

- [ ] **Refunds API**
  - [ ] Full refund
  - [ ] Partial refund
  - [ ] Refund status check

- [ ] **Transactions API**
  - [ ] Transaction enquiry by transaction ID
  - [ ] Transaction enquiry by order ID

- [ ] **Checkout Utilities API**
  - [ ] List payment modes
  - [ ] List banks
  - [ ] List wallets
  - [ ] List EMIs
  - [ ] Validate UPI VPA

- [ ] **Error Handling**
  - [ ] Invalid credentials
  - [ ] Invalid order data
  - [ ] Network errors
  - [ ] Exception types (AuthenticationException, BadRequestException, etc.)

### Merchant Sample App Testing

- [ ] **Order Creation**
  - [ ] Create order via form
  - [ ] Order validation
  - [ ] Error handling
  - [ ] Order with line items
  - [ ] Order with callback URL

- [ ] **Checkout Flow - Popup Mode**
  - [ ] Open checkout in popup
  - [ ] Payment processing
  - [ ] Callback handling
  - [ ] Success page
  - [ ] Failure page

- [ ] **Checkout Flow - Redirect Mode**
  - [ ] Redirect to Nimbbl checkout
  - [ ] Payment processing
  - [ ] Callback URL handling
  - [ ] Success page
  - [ ] Failure page

- [ ] **Webhook Handling**
  - [ ] Webhook endpoint
  - [ ] Signature verification
  - [ ] Event processing
  - [ ] Error handling

- [ ] **Logging**
  - [ ] Log file creation
  - [ ] Request/response logging
  - [ ] Sensitive data masking
  - [ ] Debug logging
  - [ ] Caller information in logs

### Both Applications

- [ ] **NuGet Package Testing**
  - [ ] Examples app works with NuGet package
  - [ ] MerchantSampleApp works with NuGet package
  - [ ] All features work correctly

- [ ] **Local Source Testing**
  - [ ] Examples app works with local source
  - [ ] MerchantSampleApp works with local source
  - [ ] All features work correctly

## Troubleshooting

### Common Issues and Solutions

#### 1. Build Errors

**Error:** `The type or namespace name 'Nimbbl' could not be found`

**Solution:**

```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

#### 2. Missing Credentials

**Error:** `NIMBBL_ACCESS_KEY is required`

**Solution:**

- For Examples: Ensure `.env` file exists and contains credentials
- For MerchantSampleApp: Set in `.env` file or environment variables (the app loads `.env` automatically via `EnvLoader`)
- Verify credentials are correct

#### 3. API Authentication Errors

**Error:** `401 Unauthorized` or `Authentication failed`

**Solution:**

- Verify access key and secret are correct
- Check if credentials are for the correct environment (production/UAT/QA)
- Ensure credentials are not expired

#### 4. Logging Not Working

**Issue:** No log files created

**Solution:**

- Enable logging: Set `NIMBBL_ENABLE_LOGGING=true` in `.env` file or environment variables
- Check if log directory exists: `logs/`
- Verify write permissions for log directory
- Check log file path configuration

#### 5. Port Already in Use

**Error:** `Address already in use` when running MerchantSampleApp

**Solution:**

```bash
# Find process using the port
# Windows
netstat -ano | findstr :5000

# Linux/Mac
lsof -i :5000

# Kill the process or change port in launchSettings.json
```

#### 6. Project Reference Issues

**Error:** `Project reference not found`

**Solution:**

```bash
# Ensure you're in the correct directory
cd Examples  # or MerchantSampleApp

# Restore dependencies
dotnet restore

# Verify project reference in .csproj file
# For local source: Should have <ProjectReference Include="..\Nimbbl.Sdk.Rest\Nimbbl.Sdk.Rest.csproj" />
# For NuGet: Should have <PackageReference Include="Nimbbl.Sdk.Rest" Version="1.3.5-rc2" />
```

#### 7. NuGet Package Not Found

**Error:** `Package 'Nimbbl.Sdk.Rest' is not found`

**Solution:**

- Check NuGet.org for package availability
- Verify package version exists
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Check NuGet source configuration

#### 8. SSL Certificate Errors

**Error:** `The SSL connection could not be established`

**Solution:**

- Ensure system certificates are up to date
- Check if corporate proxy is blocking connections
- Verify API host URL is correct

#### 9. Examples Menu Not Showing

**Issue:** Examples app runs but menu doesn't appear

**Solution:**

- Check if `.env` file exists and has valid credentials
- Verify console output for error messages
- Check log files for detailed errors

#### 10. Checkout Not Opening

**Issue:** Checkout popup/redirect not working

**Solution:**

- Check browser console for JavaScript errors
- Verify order token is valid
- Check network requests in browser DevTools
- Verify callback URL is publicly accessible (for redirect mode)

### Getting Help

If you encounter issues not covered here:

1. **Check Logs:**
   - Review log files in `logs/` directory
   - Enable debug logging for detailed information
   - Check both Examples and MerchantSampleApp logs

2. **Verify Configuration:**
   - Double-check credentials
   - Verify API host URL
   - Check environment variables
   - Review `.env` file and environment variables

3. **Test with Simple Example:**
   - Start with token generation
   - Then test order creation
   - Gradually add complexity

4. **Contact Support:**
   - Check SDK documentation
   - Review API documentation: [Nimbbl API Documentation](https://nimbbl.biz/docs/api-reference/introduction/)
   - Contact Nimbbl support with error details and logs

## Additional Resources

- **SDK README:** [README.md](README.md)
- **Integration Guide:** [MerchantSampleApp/INTEGRATION.md](MerchantSampleApp/INTEGRATION.md)
- **Examples Guide:** [Examples/README.md](Examples/README.md)
- **Build Guide:** [BUILD_RUN_PACKAGE.md](BUILD_RUN_PACKAGE.md)
- **Webhook Testing:** [MerchantSampleApp/WEBHOOK_TESTING.md](MerchantSampleApp/WEBHOOK_TESTING.md)
- **API Documentation:** [Nimbbl API Documentation](https://nimbbl.biz/docs/api-reference/introduction/)

## Notes for Testers

1. **Test Environment:** Use UAT/Sandbox environment for testing to avoid affecting production data
2. **Logging:** Enable logging during testing to capture request/response details
3. **Sensitive Data:** Be careful with logs containing sensitive information
4. **Test Data:** Use test credentials and test amounts
5. **Cleanup:** Clean up test orders/payments after testing if possible
6. **Documentation:** Report any issues or inconsistencies found during testing
7. **Both Methods:** Test with both NuGet package and local source to ensure consistency
8. **Cross-Platform:** Test on different operating systems if possible (Windows, Linux, Mac)

---

**Last Updated:** January 2025  
**SDK Version:** 1.3.5-rc2
