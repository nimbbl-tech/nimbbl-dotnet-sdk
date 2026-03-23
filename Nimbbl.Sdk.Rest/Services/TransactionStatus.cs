using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest.Transactions;

public class Transactions : BaseService
{
    internal Transactions(ApiClient apiClient) : base(apiClient)
    {
    }

    /// <summary>
    /// Get transaction status and details.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="attributes">Transaction enquiry request parameters</param>
    /// <returns>JSON response containing transaction details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/transaction-enquiry-v-3/">Transaction Enquiry API</see> for more details.</remarks>
    public Task<JsonElement> TransactionEnquiryAsync(Dictionary<string, object?>? attributes = null)
    {
        attributes ??= [];
        
        var isEncryptEnabled = ApiClient.IsEncryptPayloadEnabled();
        Logger.DebugWithCaller($"TransactionEnquiryAsync - Encryption enabled: {isEncryptEnabled}");
        
        // Encrypt payload if encryption is enabled
        if (isEncryptEnabled)
        {
            try
            {
                Logger.DebugWithCaller($"TransactionEnquiryAsync - {ErrorMessages.LogStartingPayloadEncryption}");
                var encryption = new Encryption(ApiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(attributes);
                
                // Wrap encrypted payload in the format expected by API
                // According to API docs: https://nimbbl.biz/docs/api-reference/transaction-enquiry-v-3/
                // The API accepts either a regular request or an encrypted payload
                attributes = new Dictionary<string, object?>
                {
                    [JsonKeys.EncryptedPayload] = encryptedPayload
                };
                
                Logger.InfoWithCaller($"Transaction enquiry request {ErrorMessages.LogPayloadEncryptedSuccessfully}");
            }
            catch (System.Exception ex)
            {
                Logger.ExceptionWithCaller(string.Format(ErrorMessages.EncryptionErrorFormat, "transaction enquiry", ex.Message), ex);
                throw new NimbblException(string.Format(ErrorMessages.EncryptionErrorFormat, "transaction enquiry", ex.Message), HttpStatusCodes.Unknown, ErrorCodes.EncryptionError);
            }
        }
        else
        {
            Logger.DebugWithCaller($"TransactionEnquiryAsync - {ErrorMessages.LogEncryptionDisabled}");
        }
        
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.TransactionEnquiry, attributes);
    }
}
