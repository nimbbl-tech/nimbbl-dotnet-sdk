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
    /// <param name="enableLogging">Enable SDK logging (optional, defaults to true)</param>
    /// <param name="debugLogging">Enable debug logging (optional, defaults to false)</param>
    /// <param name="logFilePath">Log file path (optional)</param>
    /// <param name="logAction">Custom log action (optional)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddNimbbl(
        this IServiceCollection services,
        string accessKey,
        string accessSecret,
        string? apiHost = null,
        bool? enableLogging = null,
        bool? debugLogging = null,
        string? logFilePath = null,
        Action<string, string>? logAction = null)
    {
        // Initialize NimbblApi from provided parameters
        var api = NimbblApi.Initialize(
            accessKey: accessKey,
            accessSecret: accessSecret,
            apiHost: apiHost,
            enableLogging: enableLogging,
            debugLogging: debugLogging,
            logFilePath: logFilePath,
            logAction: logAction);
        
        // Register NimbblApi as singleton
        services.AddSingleton(api);
        
        return services;
    }
}

