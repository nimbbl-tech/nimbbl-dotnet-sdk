using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Log;
using NimbblAuth = Nimbbl.Sdk.Rest.Auth.Auth;
using NimbblOrders = Nimbbl.Sdk.Rest.Orders.Orders;
using NimbblPaymentLinks = Nimbbl.Sdk.Rest.PaymentLinks.PaymentLinks;
using NimbblTransactions = Nimbbl.Sdk.Rest.Transactions.Transactions;
using NimbblRefunds = Nimbbl.Sdk.Rest.Refunds.Refunds;
using NimbblAddresses = Nimbbl.Sdk.Rest.Addresses.Addresses;
using NimbblPayments = Nimbbl.Sdk.Rest.Payments.Payments;
using NimbblCheckoutUtilities = Nimbbl.Sdk.Rest.CheckoutUtilities.CheckoutUtilities;

namespace Nimbbl.Sdk.Rest.Api;

/// <summary>
/// Main entry point for the Nimbbl .NET SDK.
/// Provides access to all API resources (Orders, Transactions, Payments, etc.)
/// and handles initialization from provided parameters.
/// </summary>
public class NimbblApi : IDisposable
{
    private readonly NimbblClient _client;

    public NimbblApi(string key, string secret, string? baseUrl = null, string? logFilePath = null, bool encryptPayload = false)
    {
        var url = string.IsNullOrWhiteSpace(baseUrl) ? ApiConstants.BaseUrl : baseUrl!;
        
        // Initialize Logger with default log file path if not provided
        var defaultLogPath = string.IsNullOrWhiteSpace(logFilePath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "logs", "nimbbl_debug.log")
            : logFilePath;
        
        if (!string.IsNullOrWhiteSpace(defaultLogPath))
        {
            Logger.GetInstance(defaultLogPath);
        }
        
        _client = new NimbblClient(key, secret, url, encryptPayload);
    }

    /// <summary>
    /// Initialize API instance from provided parameters.
    /// Caller is responsible for loading .env files and passing values as parameters.
    /// </summary>
    /// <param name="accessKey">Nimbbl access key (required)</param>
    /// <param name="accessSecret">Nimbbl access secret (required)</param>
    /// <param name="apiHost">API host URL (optional, defaults to production)</param>
    /// <param name="enableLogging">Enable SDK logging (optional, defaults to false)</param>
    /// <param name="debugLogging">Enable debug logging (optional, defaults to false)</param>
    /// <param name="logFilePath">Log file path (optional)</param>
    /// <param name="encryptPayload">Enable encryption for request payloads (optional, defaults to false)</param>
    public static NimbblApi Initialize(
        string accessKey,
        string accessSecret,
        string? apiHost = null,
        bool? enableLogging = null,
        bool? debugLogging = null,
        string? logFilePath = null,
        bool encryptPayload = false)
    {
        // Configure SDK logging
        var enableLog = enableLogging ?? false;
        var debugLog = debugLogging ?? false;
        
        if (enableLog)
        {
            Logger.EnableLogging();
        }
        
        if (debugLog)
        {
            Logger.EnableDebug();
        }
        
        // Build base URL from apiHost or use default
        var baseUrl = string.IsNullOrWhiteSpace(apiHost)
            ? ApiConstants.BaseUrl
            : $"{apiHost.TrimEnd('/')}{ApiConstants.ApiPath}";
        
        // Create NimbblApi instance from provided parameters
        return new NimbblApi(accessKey, accessSecret, baseUrl, logFilePath, encryptPayload);
    }

    public NimbblOrders Orders() => _client.Orders;
    public NimbblTransactions Transactions() => _client.Transactions;
    public NimbblRefunds Refunds() => _client.Refunds;
    public NimbblAddresses Addresses() => _client.Addresses;
    public NimbblPayments Payments() => _client.Payments;
    public NimbblPaymentLinks PaymentLinks() => _client.PaymentLinks;
    public NimbblCheckoutUtilities CheckoutUtilities() => _client.CheckoutUtilities;
    public NimbblAuth Auth() => _client.Auth;

    public void AddHeader(string key, string value) => _client.AddHeader(key, value);
    public void SetBearerToken(string token, DateTime? expiresAtUtc = null) => _client.SetBearerToken(token, expiresAtUtc);

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _client?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }
}

