using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
using Nimbbl.Sdk.Rest.Exception;

namespace Nimbbl.Sdk.Rest.Addresses;

public class Addresses : BaseService
{
    internal Addresses(ApiClient apiClient) : base(apiClient)
    {
    }

    /// <summary>
    /// List addresses for a user.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="options">Optional query parameters (user_id, amount, currency, etc.)</param>
    /// <returns>JSON response containing list of addresses</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/list-addresses-v-3/">List Addresses API</see> for more details.</remarks>
    public Task<JsonElement> ListAddressesAsync(Dictionary<string, object?>? options = null)
    {
        // Pass options directly - ApiClient will build query string automatically for GET requests
        return ApiClient.Get<JsonElement>(ApiConstants.AddressList, options);
    }

    /// <summary>
    /// Create a new address.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Address creation request parameters</param>
    /// <returns>JSON response containing created address details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/create-an-address-v-3/">Create Address API</see> for more details.</remarks>
    public Task<JsonElement> CreateAddressAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressCreate, request);
    }

    // NOTE: The public Addresses docs do not include a stable "Get Address by ID" endpoint.
    // Use ListAddressesAsync (with user_id) and filter by address_id on the client side instead.

    /// <summary>
    /// Update an existing address.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="request">Address update request parameters</param>
    /// <returns>JSON response containing updated address details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/update-an-address-v-3/">Update Address API</see> for more details.</remarks>
    public Task<JsonElement> UpdateAddressAsync(string id, Dictionary<string, object?> request)
    {
        // Per docs, Update Address expects an `address` object in the request body, including `address_id`.
        // Ref: https://nimbbl.biz/docs/api-reference/update-an-address-v-3/
        // Use request as-is - it should already have the correct structure with address_id inside
        
        // Endpoint is `v3/addresses` (no /{id})
        return ApiClient.Patch<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressUpdate, request);
    }

    /// <summary>
    /// Delete an address.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <returns>JSON response containing deletion status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/delete-an-address-v-3/">Delete Address API</see> for more details.</remarks>
    public Task<JsonElement> DeleteAddressAsync(string id)
    {
        // Use query parameter as per API documentation
        var request = new Dictionary<string, object?> { ["address_id"] = id };
        return ApiClient.Delete<JsonElement>(ApiConstants.AddressDelete, request);
    }

    /// <summary>
    /// Import multiple addresses in bulk.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Address import request parameters</param>
    /// <returns>JSON response containing import status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/import-addresses-v-3/">Import Addresses API</see> for more details.</remarks>
    public Task<JsonElement> ImportAddressesAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressImport, request);
    }

    /// <summary>
    /// Check address eligibility for delivery.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Eligibility check request parameters (pincode is required)</param>
    /// <returns>JSON response containing eligibility status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/check-address-eligibility-v-3/">Check Address Eligibility API</see> for more details.</remarks>
    public Task<JsonElement> CheckAddressEligibilityAsync(Dictionary<string, object?> request)
    {
        // Validate required parameter
        if (!request.ContainsKey(JsonKeys.Pincode) || string.IsNullOrWhiteSpace(request[JsonKeys.Pincode]?.ToString()))
        {
            throw new NimbblException(ErrorMessages.PincodeRequired, HttpStatusCodes.BadRequest, ErrorCodes.PincodeRequired);
        }
        
        // Use GET with query parameters as per API documentation
        return ApiClient.Get<JsonElement>(ApiConstants.AddressCheckEligibility, request);
    }

    /// <summary>
    /// Link an address with an order.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Link request parameters</param>
    /// <returns>JSON response containing link status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/link-address-with-order-v-3/">Link Address with Order API</see> for more details.</remarks>
    public Task<JsonElement> LinkAddressWithOrderAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.AddressLinkOrder, request);
    }
}
