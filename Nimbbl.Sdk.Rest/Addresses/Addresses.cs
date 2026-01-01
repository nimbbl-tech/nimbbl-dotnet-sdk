using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
using Nimbbl.Sdk.Rest.Exception;

namespace Nimbbl.Sdk.Rest.Addresses;

public class Addresses
{
    private readonly ApiClient _apiClient;
    
    internal Addresses(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// List addresses for a user.
    /// </summary>
    /// <param name="options">Optional query parameters (user_id, amount, currency, etc.)</param>
    /// <returns>JSON response containing list of addresses</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/list-addresses-v-3/">List Addresses API</see> for more details.</remarks>
    public Task<JsonElement> ListAddressesAsync(Dictionary<string, object?>? options = null)
    {
        // Pass options directly - ApiClient will build query string automatically for GET requests
        return _apiClient.Get<JsonElement>(ApiConstants.AddressList, options);
    }

    /// <summary>
    /// Create a new address.
    /// </summary>
    /// <param name="request">Address creation request parameters</param>
    /// <returns>JSON response containing created address details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/create-an-address-v-3/">Create Address API</see> for more details.</remarks>
    public Task<JsonElement> CreateAddressAsync(Dictionary<string, object?> request)
    {
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressCreate, request);
    }

    /// <summary>
    /// Get address details by address ID.
    /// </summary>
    /// <param name="addressId">Address ID</param>
    /// <returns>JSON response containing address details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-an-address-v-3/">Get Address API</see> for more details.</remarks>
    public Task<JsonElement> GetAddressByIdAsync(string addressId)
    {
        return _apiClient.Get<JsonElement>($"{ApiConstants.AddressGet}/{addressId}");
    }

    /// <summary>
    /// Update an existing address.
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="request">Address update request parameters</param>
    /// <returns>JSON response containing updated address details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/update-an-address-v-3/">Update Address API</see> for more details.</remarks>
    public Task<JsonElement> UpdateAddressAsync(string id, Dictionary<string, object?> request)
    {
        var endpoint = $"{ApiConstants.AddressUpdate}/{id}";
        return _apiClient.Patch<Dictionary<string, object?>, JsonElement>(endpoint, request);
    }

    /// <summary>
    /// Delete an address.
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <returns>JSON response containing deletion status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/delete-an-address-v-3/">Delete Address API</see> for more details.</remarks>
    public Task<JsonElement> DeleteAddressAsync(string id)
    {
        // Use query parameter as per API documentation
        var request = new Dictionary<string, object?> { ["address_id"] = id };
        return _apiClient.Delete<JsonElement>(ApiConstants.AddressDelete, request);
    }

    /// <summary>
    /// Import multiple addresses in bulk.
    /// </summary>
    /// <param name="request">Address import request parameters</param>
    /// <returns>JSON response containing import status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/import-addresses-v-3/">Import Addresses API</see> for more details.</remarks>
    public Task<JsonElement> ImportAddressesAsync(Dictionary<string, object?> request)
    {
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressImport, request);
    }

    /// <summary>
    /// Check address eligibility for delivery.
    /// </summary>
    /// <param name="request">Eligibility check request parameters (pincode is required)</param>
    /// <returns>JSON response containing eligibility status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/check-address-eligibility-v-3/">Check Address Eligibility API</see> for more details.</remarks>
    public Task<JsonElement> CheckAddressEligibilityAsync(Dictionary<string, object?> request)
    {
        // Validate required parameter
        if (!request.ContainsKey("pincode") || string.IsNullOrWhiteSpace(request["pincode"]?.ToString()))
        {
            throw new NimbblException(ErrorMessages.PincodeRequired, 400, ErrorCodes.PincodeRequired);
        }
        
        // Use GET with query parameters as per API documentation
        return _apiClient.Get<JsonElement>(ApiConstants.AddressCheckEligibility, request);
    }

    /// <summary>
    /// Link an address with an order.
    /// </summary>
    /// <param name="request">Link request parameters</param>
    /// <returns>JSON response containing link status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/link-address-with-order-v-3/">Link Address with Order API</see> for more details.</remarks>
    public Task<JsonElement> LinkAddressWithOrderAsync(Dictionary<string, object?> request)
    {
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressLinkOrder, request);
    }
}

