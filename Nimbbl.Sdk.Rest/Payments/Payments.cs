using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest.Payments;

public class Payments
{
    private readonly ApiClient _apiClient;
    
    internal Payments(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Initiate a payment for an order.
    /// </summary>
    /// <param name="request">Payment initiation request parameters</param>
    /// <returns>JSON response containing payment details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/initiate-a-payment-v-3/">Initiate Payment API</see> for more details.</remarks>
    public Task<JsonElement> InitiatePaymentAsync(Dictionary<string, object?> request)
    {
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentInitiate, request);
    }

    /// <summary>
    /// Complete a payment transaction.
    /// </summary>
    /// <param name="request">Payment completion request parameters</param>
    /// <returns>JSON response containing payment completion details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/complete-payment-v-3/">Complete Payment API</see> for more details.</remarks>
    public Task<JsonElement> CompletePaymentAsync(Dictionary<string, object?> request)
    {
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentComplete, request);
    }

    /// <summary>
    /// Resend OTP for payment verification.
    /// </summary>
    /// <param name="request">Resend OTP request parameters</param>
    /// <returns>JSON response containing OTP resend status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/resend-otp-v-3/">Resend OTP API</see> for more details.</remarks>
    public Task<JsonElement> ResendPaymentOtpAsync(Dictionary<string, object?> request)
    {
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentResendOtp, request);
    }
}

