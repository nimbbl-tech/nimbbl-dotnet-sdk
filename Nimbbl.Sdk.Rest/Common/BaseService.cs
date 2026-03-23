using LoggerClass = Nimbbl.Sdk.Rest.Log.Logger;
using Nimbbl.Sdk.Rest.RestClient;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Base class for SDK services that provides common functionality like logging and API client access
/// </summary>
public abstract class BaseService
{
    /// <summary>
    /// API client instance for making HTTP requests
    /// </summary>
    internal readonly ApiClient ApiClient;

    /// <summary>
    /// Logger instance for this service
    /// </summary>
    protected readonly LoggerClass Logger;

    /// <summary>
    /// Initializes a new instance of the BaseService class
    /// </summary>
    /// <param name="apiClient">The API client instance</param>
    internal BaseService(ApiClient apiClient)
    {
        ApiClient = apiClient;
        Logger = LoggerClass.GetInstance();
    }
}
