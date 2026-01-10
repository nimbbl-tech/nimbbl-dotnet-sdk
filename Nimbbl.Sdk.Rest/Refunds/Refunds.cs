using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest.Refunds;

public class Refunds : BaseService
{
    internal Refunds(ApiClient apiClient) : base(apiClient)
    {
    }

    /// <summary>
    /// Initiate a refund for a payment transaction.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Refund request parameters</param>
    /// <returns>JSON response containing refund details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/refund-a-payment-v-3/">Refund Payment API</see> for more details.</remarks>
    public Task<JsonElement> InitiateRefundAsync(Dictionary<string, object?> request)
    {
        var isEncryptEnabled = ApiClient.IsEncryptPayloadEnabled();
        Logger.DebugWithCaller($"InitiateRefundAsync - Encryption enabled: {isEncryptEnabled}");
        
        // Encrypt payload if encryption is enabled
        if (isEncryptEnabled)
        {
            try
            {
                Logger.DebugWithCaller($"InitiateRefundAsync - {ErrorMessages.LogStartingPayloadEncryption}");
                var encryption = new Encryption(ApiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(request);
                
                // Wrap encrypted payload in the format expected by API
                // The API accepts either a regular request or an encrypted payload
                request = new Dictionary<string, object?>
                {
                    [JsonKeys.EncryptedPayload] = encryptedPayload
                };
                
                Logger.InfoWithCaller($"Refund request {ErrorMessages.LogPayloadEncryptedSuccessfully}");
            }
            catch (System.Exception ex)
            {
                Logger.ExceptionWithCaller(string.Format(ErrorMessages.EncryptionErrorFormat, "refund", ex.Message), ex);
                throw new NimbblException(string.Format(ErrorMessages.EncryptionErrorFormat, "refund", ex.Message), 500, ErrorCodes.EncryptionError);
            }
        }
        else
        {
            Logger.DebugWithCaller($"InitiateRefundAsync - {ErrorMessages.LogEncryptionDisabled}");
        }
        
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.RefundInitiate, request);
    }
}

