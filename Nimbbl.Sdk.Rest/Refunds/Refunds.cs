using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest;

public class Refunds
{
    private readonly ApiClient _apiClient;
    
    internal Refunds(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<JsonElement> InitiateRefundAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.RefundInitiate, request);
    }
}

