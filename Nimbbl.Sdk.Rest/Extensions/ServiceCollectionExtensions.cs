using Microsoft.Extensions.DependencyInjection;
using Nimbbl.Sdk.Rest.Api;

namespace Nimbbl.Sdk.Rest.Extensions;

/// <summary>
/// Extension methods for registering Nimbbl SDK services in dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Nimbbl SDK services in the dependency injection container.
    /// Initializes API from provided parameters and registers as singleton.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="accessKey">Nimbbl access key (required)</param>
    /// <param name="accessSecret">Nimbbl access secret (required)</param>
    /// <param name="apiHost">API host URL (optional, defaults to production)</param>
    /// <param name="debugLogging">Enable debug logging (optional, defaults to false). Note: INFO, WARNING, ERROR logs are always enabled.</param>
    /// <param name="logFilePath">Log file path (optional, defaults to logs/nimbbl_debug.log)</param>
    /// <param name="encryptPayload">Enable encryption for request payloads (optional, defaults to false)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddNimbbl(
        this IServiceCollection services,
        string accessKey,
        string accessSecret,
        string? apiHost = null,
        bool? debugLogging = null,
        string? logFilePath = null,
        bool encryptPayload = false)
    {
        // Initialize NimbblApi from provided parameters
        // Note: Logging (INFO, WARNING, ERROR) is always enabled by default
        // Only DEBUG logs are controlled by debugLogging parameter
        var api = NimbblApi.Initialize(
            accessKey: accessKey,
            accessSecret: accessSecret,
            apiHost: apiHost,
            debugLogging: debugLogging,
            logFilePath: logFilePath,
            encryptPayload: encryptPayload);
        
        // Register NimbblApi as singleton
        services.AddSingleton(api);
        
        return services;
    }
}

