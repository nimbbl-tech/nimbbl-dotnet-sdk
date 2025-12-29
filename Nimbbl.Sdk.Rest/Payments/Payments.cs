using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest;

public class Payments
{
    private readonly ApiClient _apiClient;
    
    internal Payments(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<JsonElement> InitiatePaymentAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentInitiate, request);
    }

    public Task<JsonElement> CompletePaymentAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentComplete, request);
    }

    public Task<JsonElement> ResendPaymentOtpAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentResendOtp, request);
    }
}

