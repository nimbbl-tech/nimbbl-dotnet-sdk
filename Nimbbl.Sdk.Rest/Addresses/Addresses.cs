using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
using Nimbbl.Sdk.Rest.Exception;

namespace Nimbbl.Sdk.Rest;

public class Addresses
{
    private readonly ApiClient _apiClient;
    
    internal Addresses(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<JsonElement> ListAddressesAsync(Dictionary<string, object?>? options = null)
    {
        // Pass options directly - ApiClient will build query string automatically for GET requests
        return _apiClient.GetWithAuth<JsonElement>(ApiConstants.AddressList, options);
    }

    public Task<JsonElement> CreateAddressAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressCreate, request);
    }

    public Task<JsonElement> GetAddressByIdAsync(string addressId)
    {
        return _apiClient.GetWithAuth<JsonElement>($"{ApiConstants.AddressGet}/{addressId}");
    }

    public Task<JsonElement> UpdateAddressAsync(string id, Dictionary<string, object?> request)
    {
        var endpoint = $"{ApiConstants.AddressUpdate}/{id}";
        return _apiClient.PatchWithAuth<Dictionary<string, object?>, JsonElement>(endpoint, request);
    }

    public Task<JsonElement> DeleteAddressAsync(string id)
    {
        // Use query parameter as per API documentation
        var request = new Dictionary<string, object?> { ["address_id"] = id };
        return _apiClient.DeleteWithAuth<JsonElement>(ApiConstants.AddressDelete, request);
    }

    public Task<JsonElement> ImportAddressesAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressImport, request);
    }

    public Task<JsonElement> CheckAddressEligibilityAsync(Dictionary<string, object?> request)
    {
        // Validate required parameter
        if (!request.ContainsKey("pincode") || string.IsNullOrWhiteSpace(request["pincode"]?.ToString()))
        {
            throw new NimbblException(ErrorMessages.PincodeRequired, 400, ErrorCodes.PincodeRequired);
        }
        
        // Use GET with query parameters as per API documentation
        return _apiClient.GetWithAuth<JsonElement>(ApiConstants.AddressCheckEligibility, request);
    }

    public Task<JsonElement> LinkAddressWithOrderAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressLinkOrder, request);
    }
}

