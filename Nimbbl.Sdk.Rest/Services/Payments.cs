using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest.Payments;

public class Payments : BaseService
{
    internal Payments(ApiClient apiClient) : base(apiClient)
    {
    }

    /// <summary>
    /// Initiate a payment for an order.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Payment initiation request parameters</param>
    /// <returns>JSON response containing payment details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/initiate-a-payment-v-3/">Initiate Payment API</see> for more details.</remarks>
    public Task<JsonElement> InitiatePaymentAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentInitiate, request);
    }

    /// <summary>
    /// Complete a payment transaction.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Payment completion request parameters</param>
    /// <returns>JSON response containing payment completion details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/complete-a-payment-v-3/">Complete Payment API</see> for more details.</remarks>
    public Task<JsonElement> CompletePaymentAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentComplete, request);
    }

    /// <summary>
    /// Resend OTP for payment verification.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Resend OTP request parameters</param>
    /// <returns>JSON response containing OTP resend status</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/resend-an-otp-v-3/">Resend OTP API</see> for more details.</remarks>
    public Task<JsonElement> ResendPaymentOtpAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentResendOtp, request);
    }

    /// <summary>
    /// Capture a pre-authorized payment (pre-auth), collecting the held funds.
    /// Full amount only; the sub-merchant must have capture_mode=manual enabled.
    /// Acts on a transaction in the 'authorized' status. Server-to-server.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Capture request parameters. Expects <c>transaction_id</c> (authorized txn); optional <c>comment</c>.</param>
    /// <returns>JSON response containing capture details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/introduction/">Capture Payment API</see> for more details.</remarks>
    public Task<JsonElement> CaptureAsync(Dictionary<string, object?> request)
    {
        return PostWithOptionalEncryptionAsync(ApiConstants.Capture, request, "capture");
    }

    /// <summary>
    /// Void (cancel) a pre-authorized payment (pre-auth), releasing the held funds without charging the customer.
    /// Acts on a transaction in the 'authorized' status. Server-to-server.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Void request parameters. Expects <c>transaction_id</c> (authorized txn); optional <c>comment</c>.</param>
    /// <returns>JSON response containing void details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/introduction/">Void Payment API</see> for more details.</remarks>
    public Task<JsonElement> VoidAsync(Dictionary<string, object?> request)
    {
        return PostWithOptionalEncryptionAsync(ApiConstants.Void, request, "void");
    }

    /// <summary>
    /// Posts a payload to the given endpoint, wrapping it as an encrypted payload when
    /// payload encryption is enabled (mirrors the order/refund encryption flow).
    /// </summary>
    private Task<JsonElement> PostWithOptionalEncryptionAsync(string endpoint, Dictionary<string, object?> request, string context)
    {
        var isEncryptEnabled = ApiClient.IsEncryptPayloadEnabled();
        Logger.DebugWithCaller($"{context} - Encryption enabled: {isEncryptEnabled}");

        // Encrypt payload if encryption is enabled
        if (isEncryptEnabled)
        {
            try
            {
                Logger.DebugWithCaller($"{context} - {ErrorMessages.LogStartingPayloadEncryption}");
                var encryption = new Encryption(ApiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(request);

                // Wrap encrypted payload in the format expected by API
                // The API accepts either a regular request or an encrypted payload
                request = new Dictionary<string, object?>
                {
                    [JsonKeys.EncryptedPayload] = encryptedPayload
                };

                Logger.InfoWithCaller($"{context} request {ErrorMessages.LogPayloadEncryptedSuccessfully}");
            }
            catch (System.Exception ex)
            {
                Logger.ExceptionWithCaller(string.Format(ErrorMessages.EncryptionErrorFormat, context, ex.Message), ex);
                throw new NimbblException(string.Format(ErrorMessages.EncryptionErrorFormat, context, ex.Message), HttpStatusCodes.Unknown, ErrorCodes.EncryptionError);
            }
        }
        else
        {
            Logger.DebugWithCaller($"{context} - {ErrorMessages.LogEncryptionDisabled}");
        }

        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(endpoint, request);
    }
}
