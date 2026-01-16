using System;
using Nimbbl.Sdk.Rest.Api;

namespace Nimbbl.Sdk.Rest.Test;

/// <summary>
/// Base class for all tests that provides API initialization from environment variables
/// </summary>
public abstract class TestBase
{
    protected readonly NimbblApi Api;
    protected readonly bool IsEncryptionEnabled;

    protected TestBase(bool? encryptPayloadOverride = null)
    {
        Api = InitializeApi(encryptPayloadOverride);
        
        var encryptPayloadStr = Environment.GetEnvironmentVariable("ENCRYPT_PAYLOAD");
        IsEncryptionEnabled = encryptPayloadOverride ?? 
                             (!string.IsNullOrWhiteSpace(encryptPayloadStr) && 
                              bool.TryParse(encryptPayloadStr, out var parsed) && parsed);
    }

    /// <summary>
    /// Static helper to initialize NimbblApi from environment variables
    /// </summary>
    /// <param name="encryptPayloadOverride">Optional override for encryption setting</param>
    /// <returns>Initialized NimbblApi instance</returns>
    protected static NimbblApi InitializeApi(bool? encryptPayloadOverride = null)
    {
        // Load .env file if it exists (only loads once, even if called multiple times)
        EnvLoader.LoadEnvFile();
        
        // Read from environment variables (required)
        var accessKey = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY");
        var accessSecret = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET");
        var apiHost = Environment.GetEnvironmentVariable("NIMBBL_API_HOST");
        
        // Read encrypt payload setting (optional, defaults to false)
        var encryptPayload = false;
        if (encryptPayloadOverride.HasValue)
        {
            encryptPayload = encryptPayloadOverride.Value;
        }
        else
        {
            var encryptPayloadStr = Environment.GetEnvironmentVariable("ENCRYPT_PAYLOAD");
            if (!string.IsNullOrWhiteSpace(encryptPayloadStr) && bool.TryParse(encryptPayloadStr, out var parsedEncrypt))
            {
                encryptPayload = parsedEncrypt;
            }
        }

        // Read logging settings (optional)
        var logFilePath = Environment.GetEnvironmentVariable("NIMBBL_LOG_FILE");
        var debugLoggingStr = Environment.GetEnvironmentVariable("NIMBBL_DEBUG_LOGGING");
        bool? debugLogging = null;
        if (!string.IsNullOrWhiteSpace(debugLoggingStr) && bool.TryParse(debugLoggingStr, out var parsedDebug))
        {
            debugLogging = parsedDebug;
        }
        
        if (string.IsNullOrWhiteSpace(accessKey))
            throw new InvalidOperationException("NIMBBL_ACCESS_KEY environment variable is required. Set it in your environment or create a .env file.");
        if (string.IsNullOrWhiteSpace(accessSecret))
            throw new InvalidOperationException("NIMBBL_ACCESS_SECRET environment variable is required. Set it in your environment or create a .env file.");
        
        return NimbblApi.Initialize(
            accessKey: accessKey!,
            accessSecret: accessSecret!,
            apiHost: apiHost,
            debugLogging: debugLogging,
            logFilePath: logFilePath,
            encryptPayload: encryptPayload
        );
    }
}
