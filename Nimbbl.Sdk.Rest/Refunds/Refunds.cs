using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest.Refunds;

public class Refunds
{
    private readonly ApiClient _apiClient;
    
    internal Refunds(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Initiate a refund for a payment transaction.
    /// </summary>
    /// <param name="request">Refund request parameters</param>
    /// <returns>JSON response containing refund details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/refund-a-payment-v-3/">Refund Payment API</see> for more details.</remarks>
    public Task<JsonElement> InitiateRefundAsync(Dictionary<string, object?> request)
    {
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.RefundInitiate, request);
    }
}

