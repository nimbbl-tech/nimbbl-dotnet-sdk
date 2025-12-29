using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Log;
using NimbblAuth = Nimbbl.Sdk.Rest.Auth.Auth;

namespace Nimbbl.Sdk.Rest.Api;

/// <summary>
/// Main entry point for the Nimbbl .NET SDK.
/// Provides access to all API resources (Orders, Transactions, Payments, etc.)
/// and handles initialization from provided parameters.
/// </summary>
public class NimbblApi : IDisposable
{
    private readonly NimbblClient _client;

    public NimbblApi(string key, string secret, string? baseUrl = null, Action<string, string>? logAction = null, string? logFilePath = null)
    {
        var url = string.IsNullOrWhiteSpace(baseUrl) ? ApiConstants.BaseUrl : baseUrl!;
        
        // Initialize Logger with log file path if provided
        if (!string.IsNullOrWhiteSpace(logFilePath))
        {
            Log.Logger.GetInstance(logFilePath, logAction);
        }
        else if (logAction != null)
        {
            // If only logAction is provided, initialize Logger with it
            Log.Logger.GetInstance(null, logAction);
        }
        
        _client = new NimbblClient(key, secret, url, logAction);
    }

    /// <summary>
    /// Initialize API instance from provided parameters.
    /// Caller is responsible for loading .env files and passing values as parameters.
    /// </summary>
    /// <param name="accessKey">Nimbbl access key (required)</param>
    /// <param name="accessSecret">Nimbbl access secret (required)</param>
    /// <param name="apiHost">API host URL (optional, defaults to production)</param>
    /// <param name="enableLogging">Enable SDK logging (optional, defaults to true)</param>
    /// <param name="debugLogging">Enable debug logging (optional, defaults to false)</param>
    /// <param name="logFilePath">Log file path (optional)</param>
    /// <param name="logAction">Custom log action (optional)</param>
    public static NimbblApi Initialize(
        string accessKey,
        string accessSecret,
        string? apiHost = null,
        bool? enableLogging = null,
        bool? debugLogging = null,
        string? logFilePath = null,
        Action<string, string>? logAction = null)
    {
        // Configure SDK logging
        var enableLog = enableLogging ?? true;
        var debugLog = debugLogging ?? false;
        
        if (enableLog)
        {
            Logger.EnableLogging();
        }
        else
        {
            Logger.DisableLogging();
        }
        
        if (debugLog)
        {
            Logger.EnableDebug();
        }
        else
        {
            Logger.DisableDebug();
        }
        
        // Build base URL from apiHost or use default
        var baseUrl = string.IsNullOrWhiteSpace(apiHost) 
            ? ApiConstants.BaseUrl 
            : $"{apiHost.TrimEnd('/')}/api/";
        
        // Create NimbblApi instance from provided parameters
        return new NimbblApi(accessKey, accessSecret, baseUrl, logAction, logFilePath);
    }

    public Orders Orders() => _client.Orders;
    public Transactions Transactions() => _client.Transactions;
    public Refunds Refunds() => _client.Refunds;
    public Addresses Addresses() => _client.Addresses;
    public Payments Payments() => _client.Payments;
    public PaymentLinks PaymentLinks() => _client.PaymentLinks;
    public CheckoutUtilities CheckoutUtilities() => _client.CheckoutUtilities;
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
        GC.SuppressFinalize(this);
    }
}

