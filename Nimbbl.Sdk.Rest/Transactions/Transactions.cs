using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest.Transactions;

public class Transactions
{
    private readonly ApiClient _apiClient;
    internal Transactions(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Get transaction status and details.
    /// </summary>
    /// <param name="attributes">Transaction enquiry request parameters</param>
    /// <returns>JSON response containing transaction details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/transaction-enquiry-v-3/">Transaction Enquiry API</see> for more details.</remarks>
    public Task<JsonElement> TransactionEnquiryAsync(Dictionary<string, object?>? attributes = null)
    {
        attributes ??= [];
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.TransactionEnquiry, attributes);
    }
}