using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest;

public class Transactions
{
    private readonly ApiClient _apiClient;
    internal Transactions(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Transaction enquiry
    /// </summary>
    public Task<JsonElement> TransactionEnquiryAsync(Dictionary<string, object?>? attributes = null)
    {
        attributes ??= new Dictionary<string, object?>();
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.TransactionEnquiry, attributes);
    }
}