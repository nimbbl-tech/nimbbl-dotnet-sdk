# Nimbbl .NET SDK - Build, Run, and Package Guide

This guide provides comprehensive instructions for building, running, testing, and packaging the Nimbbl .NET SDK.

## 📋 Prerequisites

- **.NET SDK 6.0 or later** - [Download](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** (recommended) or **Visual Studio Code** with C# extension
- **NuGet** (comes with .NET SDK)

### Installing .NET SDK

#### macOS

**Option 1: Using Homebrew (Recommended)**
```bash
brew install --cask dotnet
```

**Option 2: Direct Download**
1. Visit [.NET Downloads](https://dotnet.microsoft.com/download)
2. Download the .NET SDK for macOS
3. Run the installer
4. Follow the installation wizard

**Option 3: Using Installer Script**
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0
```

After installation, add to PATH (if needed):
```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
```

#### Linux

**Ubuntu/Debian:**
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 6.0
```

**CentOS/RHEL:**
```bash
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install dotnet-sdk-6.0
```

#### Windows

1. Download the installer from [.NET Downloads](https://dotnet.microsoft.com/download)
2. Run the installer
3. Follow the installation wizard
4. Restart your terminal/command prompt

### Verify Installation

After installation, verify it works:
```bash
dotnet --version
```

You should see output like: `6.0.xxx` or higher.

If `dotnet` command is not found:
- **macOS/Linux**: Add to your `~/.zshrc` or `~/.bashrc`:
  ```bash
  export DOTNET_ROOT=$HOME/.dotnet
  export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
  ```
  Then run: `source ~/.zshrc` (or `source ~/.bashrc`)

- **Windows**: Restart your terminal or add to PATH manually

## 💻 Visual Studio Code Setup

Visual Studio Code is a lightweight, cross-platform code editor that works great for .NET development.

### Installing VS Code

1. Download VS Code from [https://code.visualstudio.com/](https://code.visualstudio.com/)
2. Install the application
3. Launch VS Code

### Required Extensions

Install the following extensions in VS Code:

#### 1. C# Extension (Required)

**Extension ID:** `ms-dotnettools.csharp`

**Installation:**
1. Open VS Code
2. Press `Cmd+Shift+X` (macOS) or `Ctrl+Shift+X` (Windows/Linux) to open Extensions
3. Search for "C#" by Microsoft
4. Click "Install"
5. Restart VS Code after installation

**Alternative: Install via Command Line**
```bash
code --install-extension ms-dotnettools.csharp
```

#### 2. .NET Extension Pack (Recommended)

**Extension ID:** `ms-dotnettools.vscode-dotnet-runtime`

This extension pack includes:
- C# for Visual Studio Code
- .NET Core Test Explorer
- NuGet Package Manager
- .NET Install Tool

**Installation:**
```bash
code --install-extension ms-dotnettools.vscode-dotnet-runtime
```

#### 3. Additional Recommended Extensions

- **C# Extensions** (`kreativ-software.csharpextensions`) - Additional C# features
- **.NET Core Test Explorer** (`formulahendry.dotnet-test-explorer`) - Test runner UI
- **NuGet Package Manager** (`jmrog.vscode-nuget-package-manager`) - Manage NuGet packages

### Opening the Project in VS Code

1. **Open the SDK folder:**
   ```bash
   cd /Users/sandeepyadav/Downloads/sdks/nimbbl-dotnet-sdk
   code .
   ```

2. **Or from VS Code:**
   - File → Open Folder
   - Navigate to the SDK directory
   - Click "Select Folder"

### Building in VS Code

#### Method 1: Using Terminal

1. Open the integrated terminal: `Ctrl+`` (backtick) or `View → Terminal`
2. Run build commands:
   ```bash
   dotnet build
   ```

#### Method 2: Using Tasks

VS Code can use tasks defined in `.vscode/tasks.json`. Create this file:

**`.vscode/tasks.json`:**
```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/Nimbbl.Sdk.Rest.sln",
                "/property:GenerateFullPaths=true",
                "/consoleloggerparameters:NoSummary"
            ],
            "problemMatcher": "$msCompile"
        },
        {
            "label": "build-release",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/Nimbbl.Sdk.Rest.sln",
                "--configuration",
                "Release",
                "/property:GenerateFullPaths=true",
                "/consoleloggerparameters:NoSummary"
            ],
            "problemMatcher": "$msCompile"
        },
        {
            "label": "clean",
            "command": "dotnet",
            "type": "process",
            "args": [
                "clean",
                "${workspaceFolder}/Nimbbl.Sdk.Rest.sln"
            ],
            "problemMatcher": "$msCompile"
        },
        {
            "label": "test",
            "command": "dotnet",
            "type": "process",
            "args": [
                "test",
                "${workspaceFolder}/Nimbbl.Sdk.Rest.sln"
            ],
            "problemMatcher": "$msCompile"
        },
        {
            "label": "pack",
            "command": "dotnet",
            "type": "process",
            "args": [
                "pack",
                "${workspaceFolder}/Nimbbl.Sdk.Rest/Nimbbl.Sdk.Rest.csproj",
                "--configuration",
                "Release"
            ],
            "problemMatcher": "$msCompile"
        }
    ]
}
```

**To use tasks:**
1. Press `Cmd+Shift+P` (macOS) or `Ctrl+Shift+P` (Windows/Linux)
2. Type "Tasks: Run Task"
3. Select the task (e.g., "build", "test", "pack")

### Running Tests in VS Code

#### Method 1: Using Terminal

```bash
dotnet test
```

#### Method 2: Using .NET Core Test Explorer

1. Install the **.NET Core Test Explorer** extension
2. Open the Test Explorer panel (Test beaker icon in sidebar)
3. Tests will be automatically discovered
4. Click the play button next to a test to run it
5. Click the play button at the top to run all tests

#### Method 3: Using CodeLens

1. Open a test file (e.g., `OrderTest.cs`)
2. You'll see "Run Test" and "Debug Test" links above test methods
3. Click to run or debug individual tests

### Debugging in VS Code

#### Setup Launch Configuration

Create `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (Tests)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/Nimbbl.Sdk.Rest.Test/bin/Debug/net6.0/Nimbbl.Sdk.Rest.Test.dll",
            "args": [],
            "cwd": "${workspaceFolder}/Nimbbl.Sdk.Rest.Test",
            "console": "internalConsole",
            "stopAtEntry": false
        },
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach"
        }
    ]
}
```

#### Debugging Tests

1. Set breakpoints in your test files
2. Press `F5` or go to Run → Start Debugging
3. Select ".NET Core Launch (Tests)" configuration
4. Tests will run and stop at breakpoints

#### Debugging Individual Tests

1. Open a test file
2. Click "Debug Test" above a test method (CodeLens)
3. Or right-click on a test and select "Debug Test"

### IntelliSense and Code Navigation

VS Code with the C# extension provides:
- **IntelliSense** - Code completion and suggestions
- **Go to Definition** - `F12` or `Cmd+Click` (macOS) / `Ctrl+Click` (Windows/Linux)
- **Find References** - `Shift+F12`
- **Rename Symbol** - `F2`
- **Quick Fix** - `Cmd+.` (macOS) / `Ctrl+.` (Windows/Linux)

### Package Management

#### Viewing NuGet Packages

1. Open a `.csproj` file
2. Right-click on a `<PackageReference>` element
3. Select "Manage NuGet Packages"

#### Installing Packages

1. Open the Command Palette: `Cmd+Shift+P` / `Ctrl+Shift+P`
2. Type "NuGet: Add Package"
3. Search for the package
4. Select version and install

### Recommended VS Code Settings

Create `.vscode/settings.json`:

```json
{
    "omnisharp.enableRoslynAnalyzers": true,
    "omnisharp.enableEditorConfigSupport": true,
    "omnisharp.organizeImportsOnFormat": true,
    "editor.formatOnSave": true,
    "editor.codeActionsOnSave": {
        "source.organizeImports": true
    },
    "files.exclude": {
        "**/bin": true,
        "**/obj": true
    },
    "search.exclude": {
        "**/bin": true,
        "**/obj": true
    }
}
```

### Keyboard Shortcuts

| Action | macOS | Windows/Linux |
|--------|-------|---------------|
| Open Command Palette | `Cmd+Shift+P` | `Ctrl+Shift+P` |
| Open Terminal | `Ctrl+`` | `Ctrl+`` |
| Go to Definition | `F12` | `F12` |
| Find References | `Shift+F12` | `Shift+F12` |
| Rename Symbol | `F2` | `F2` |
| Format Document | `Shift+Option+F` | `Shift+Alt+F` |
| Run Task | `Cmd+Shift+B` | `Ctrl+Shift+B` |
| Start Debugging | `F5` | `F5` |

### Troubleshooting VS Code

**Issue: IntelliSense not working**
- Ensure C# extension is installed and enabled
- Restart VS Code
- Run: `dotnet restore` in terminal
- Check OmniSharp log: `View → Output → OmniSharp Log`

**Issue: Tests not discovered**
- Install .NET Core Test Explorer extension
- Run `dotnet test` once in terminal
- Reload VS Code window: `Cmd+Shift+P` → "Developer: Reload Window"

**Issue: Build errors not showing**
- Check Problems panel: `View → Problems`
- Ensure C# extension is active
- Run `dotnet build` to see errors in terminal

## Building the SDK

### Build from Command Line

```bash
# Navigate to SDK directory
cd /Users/sandeepyadav/Downloads/sdks/nimbbl-dotnet-sdk

# Build the SDK
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Build without restoring packages
dotnet build --no-restore
```

### Build Output

The build process will:
- Restore NuGet packages
- Compile all C# source files
- Generate DLL files in `bin/Debug/net6.0/` or `bin/Release/net6.0/`

### Build Specific Project

```bash
# Build only the SDK library
dotnet build Nimbbl.Sdk.Rest/Nimbbl.Sdk.Rest.csproj

# Build only the test project
dotnet build Nimbbl.Sdk.Rest.Test/Nimbbl.Sdk.Rest.Test.csproj
```

## 🧪 Running Tests

### Run All Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests in Release mode
dotnet test --configuration Release
```

### Run Specific Test Class

```bash
# Run Order tests
dotnet test --filter "FullyQualifiedName~OrderTest"

# Run Transaction tests
dotnet test --filter "FullyQualifiedName~TransactionTest"
```

### Test Output

Tests will display:
- Passed tests
- Failed tests with error details
- Execution time
- 📊 Test summary

## 📦 Packaging the SDK

### Create NuGet Package

```bash
# Create NuGet package
dotnet pack

# Create package in Release mode
dotnet pack --configuration Release

# Create package with specific version
dotnet pack -p:Version=1.4.0

# Create package without building
dotnet pack --no-build
```

### Package Output

The package will be created in:
```
Nimbbl.Sdk.Rest/bin/Release/Nimbbl.Sdk.Rest.1.3.4.nupkg
```

### Package Contents

The `.nupkg` file contains:
- Compiled DLL (`Nimbbl.Sdk.Rest.dll`)
- XML documentation (if generated)
- Package metadata (`nuspec`)

## 📥 Installing the Package

### Install from Local Package

```bash
# Add local package source
dotnet nuget add source /Users/sandeepyadav/Downloads/sdks/nimbbl-dotnet-sdk/Nimbbl.Sdk.Rest/bin/Release --name LocalNimbbl

# Install package in your project
cd /path/to/your/project
dotnet add package Nimbbl.Sdk.Rest --source LocalNimbbl
```

### Install from NuGet.org (after publishing)

```bash
dotnet add package Nimbbl.Sdk.Rest
```

## 🔧 Project Structure

```
nimbbl-dotnet-sdk/
├── Nimbbl.Sdk.Rest/              # Main SDK library
│   ├── Api/                      # Main API entry point
│   │   └── NimbblApi.cs          # NimbblApi class (main facade)
│   ├── NimbblClient.cs           # Main client class
│   ├── Orders/                   # Orders API
│   │   └── Orders.cs
│   ├── Addresses/                # Addresses API
│   ├── Payments/                 # Payments API
│   ├── PaymentLinks/             # Payment Links API
│   ├── CheckoutUtilities/        # Checkout Utilities API
│   ├── Refunds/                  # Refunds API
│   ├── Transactions/             # Transactions API
│   ├── Auth/                     # Authentication
│   ├── NimbblCheckout/           # Checkout client
│   ├── Common/                   # Common utilities
│   │   ├── Encryption.cs
│   │   ├── Util.cs
│   │   └── EnvLoader.cs
│   ├── Exception/                 # Exception classes
│   ├── Log/                      # Logging
│   ├── Extensions/               # Extension methods
│   └── RestClient/               # HTTP client implementation
│       ├── ApiClient.cs
│       └── AuthenticationService.cs
├── Nimbbl.Sdk.Rest.Test/         # Test project
│   ├── OrderTest.cs
│   ├── TransactionTest.cs
│   └── ...
├── Examples/                     # Example CLI application
├── MerchantSampleApp/            # Sample web application
├── Nimbbl.Sdk.Rest.sln           # Solution file
└── README.md                      # SDK documentation
```

## 🚀 Quick Start

### 1. Build the SDK

```bash
cd /Users/sandeepyadav/Downloads/sdks/nimbbl-dotnet-sdk
dotnet build --configuration Release
```

### 2. Create Package

```bash
dotnet pack --configuration Release
```

### 3. Use in Your Project

**Option 1: Using NimbblApi.Initialize() (Recommended)**

```csharp
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Common;

// Load .env file (if using .env)
EnvLoader.LoadEnvFile();

// Initialize SDK from environment variables
var api = NimbblApi.Initialize(
    accessKey: Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY")!,
    accessSecret: Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET")!,
    apiHost: Environment.GetEnvironmentVariable("NIMBBL_API_HOST"),
    enableLogging: true,
    debugLogging: false,
    logFilePath: "logs/nimbbl_debug.log"
);

// Use APIs
var order = await api.Orders().CreateOrderAsync(new Dictionary<string, object?>
{
    ["invoice_id"] = "INV-123",
    ["total_amount"] = 40000,  // Amount in paise (₹400.00)
    ["currency"] = "INR",
    ["user"] = new Dictionary<string, object?>
    {
        ["email"] = "customer@example.com",
        ["first_name"] = "John",
        ["last_name"] = "Doe",
        ["mobile_number"] = "9876543210",
        ["country_code"] = "+91"
    }
});
```

**Option 2: Using Dependency Injection (ASP.NET Core)**

```csharp
using Nimbbl.Sdk.Rest.Extensions;

// In Program.cs
builder.Services.AddNimbbl(
    accessKey: Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY")!,
    accessSecret: Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET")!,
    apiHost: Environment.GetEnvironmentVariable("NIMBBL_API_HOST"),
    enableLogging: true,
    debugLogging: false,
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
            ["invoice_id"] = "INV-123",
            ["total_amount"] = 40000,
            ["currency"] = "INR"
        });
        return Ok(order);
    }
}
```

**Configuration via .env file:**

Create a `.env` file in your project root:
```env
NIMBBL_ACCESS_KEY=your_access_key
NIMBBL_ACCESS_SECRET=your_access_secret
NIMBBL_API_HOST=https://api.nimbbl.tech
NIMBBL_ENABLE_LOGGING=true
NIMBBL_DEBUG_LOGGING=false
NIMBBL_LOG_FILE=logs/nimbbl_debug.log
```

### 4. Run the Examples (single CLI)

From the SDK root:
```bash
dotnet run --project Examples/Examples.csproj
```

You will see a menu similar to the PHP `cli.php`:
```
Select an example to run:
1. Create Order
2. Get Order by ID
3. Get Order by Invoice ID
4. Payments API
5. Payment Links API
6. Addresses API
7. Refunds API
8. Transaction Status API
9. Checkout Utilities API
10. Webhook Handling
11. Run All Examples
0. Exit
```

Interactive prompts mirror the PHP CLI:
- **Payments > Initiate Payment**: prompts for payment mode (default `net_banking`), hints common bank codes (hdfc, icic, sbi, axis, kotak, pnb), defaults bank code to `hdfc`, asks for callback URL (default `https://example.com/callback`), and if OTP is required will prompt for OTP and complete the payment.
- **Checkout Utilities > List Banks**: supports three flows—`order_id` only, `total_amount` + `currency` without `order_id`, and empty request body. Prints `bank_list` with `bank_name`, `code`, `health_status`, and `additional_charges` when present.

## 🔍 Troubleshooting

### Build Errors

**Error: "Package restore failed"**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

**Error: "Target framework not found"**
- Ensure .NET 6.0 SDK is installed
- Check `TargetFramework` in `.csproj` file

### Test Errors

**Error: "Tests not found"**
```bash
# Rebuild test project
dotnet build Nimbbl.Sdk.Rest.Test/Nimbbl.Sdk.Rest.Test.csproj
dotnet test
```

**Error: "API credentials invalid"**
- Update `.env` file or environment variables with valid credentials
- Ensure `NIMBBL_ACCESS_KEY` and `NIMBBL_ACCESS_SECRET` are set correctly

### Package Errors

**Error: "Package already exists"**
```bash
# Delete existing package
rm Nimbbl.Sdk.Rest/bin/Release/*.nupkg

# Create new package
dotnet pack --configuration Release
```

## Version Management

### Update Version

Edit `Nimbbl.Sdk.Rest/Nimbbl.Sdk.Rest.csproj`:
```xml
<PropertyGroup>
    <Version>1.4.0</Version>
</PropertyGroup>
```

### Semantic Versioning

- **Major** (1.x.x): Breaking changes
- **Minor** (x.1.x): New features, backward compatible
- **Patch** (x.x.1): Bug fixes

## 🎯 Best Practices

1. **Always build in Release mode for production**
   ```bash
   dotnet build --configuration Release
   ```

2. **Run tests before packaging**
   ```bash
   dotnet test
   dotnet pack
   ```

3. **Use semantic versioning**
   - Update version in `.csproj` before packaging

4. **Document breaking changes**
   - Update CHANGELOG.md
   - Update README.md

5. **Test package installation**
   - Create a test project
   - Install the package
   - Verify all APIs work

## 📚 Additional Resources

- [.NET SDK Documentation](https://docs.microsoft.com/dotnet/core/)
- [NuGet Package Documentation](https://docs.microsoft.com/nuget/)
- [Nimbbl API Documentation](https://nimbbl.biz/docs/api-reference/introduction/)

## 🔗 Related Documentation

- [README.md](README.md) - SDK usage guide
- [API Reference](https://nimbbl.biz/docs/api-reference/introduction/) - Official API docs

---

**Last Updated**: 2025-01-26  
**SDK Version**: 1.3.4  
**Target Framework**: .NET 6.0

## Configuration

The SDK uses environment variables for configuration. You can provide them via:

1. **`.env` file** (recommended for development) - Place in your project root directory
2. **Environment variables** (recommended for production)

### Required Environment Variables

- `NIMBBL_ACCESS_KEY` - Your Nimbbl access key
- `NIMBBL_ACCESS_SECRET` - Your Nimbbl access secret

### Optional Environment Variables

- `NIMBBL_API_HOST` - API host URL (defaults to production: `https://api.nimbbl.tech`)
- `NIMBBL_ENABLE_LOGGING` - Enable/disable logging (defaults to `true`)
- `NIMBBL_DEBUG_LOGGING` - Enable/disable debug logging (defaults to `false`)
- `NIMBBL_LOG_FILE` - Log file path (defaults to `logs/nimbbl_debug.log`)
- `NIMBBL_CHECKOUT_HOST` - Override checkout host (optional)

See [README.md](README.md) for detailed configuration and usage examples.


