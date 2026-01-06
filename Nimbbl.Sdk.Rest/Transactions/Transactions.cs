using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.Log;
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
        
        var logger = Logger.GetInstance();
        var isEncryptEnabled = _apiClient.IsEncryptPayloadEnabled();
        logger.DebugWithCaller($"TransactionEnquiryAsync - Encryption enabled: {isEncryptEnabled}");
        
        // Encrypt payload if encryption is enabled
        if (isEncryptEnabled)
        {
            try
            {
                logger.DebugWithCaller("TransactionEnquiryAsync - Starting payload encryption");
                var encryption = new Encryption(_apiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(attributes);
                
                // Wrap encrypted payload in the format expected by API
                // According to API docs: https://nimbbl.biz/docs/api-reference/transaction-enquiry-v-3/
                // The API accepts either a regular request or an encrypted payload
                attributes = new Dictionary<string, object?>
                {
                    ["encrypted_payload"] = encryptedPayload
                };
                
                logger.InfoWithCaller("Transaction enquiry request payload encrypted successfully");
            }
            catch (System.Exception ex)
            {
                logger.ExceptionWithCaller($"Failed to encrypt transaction enquiry payload: {ex.Message}", ex);
                throw new NimbblException($"Failed to encrypt transaction enquiry payload: {ex.Message}", 500, "ENCRYPTION_ERROR");
            }
        }
        else
        {
            logger.DebugWithCaller("TransactionEnquiryAsync - Encryption disabled, sending plain payload");
        }
        
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.TransactionEnquiry, attributes);
    }
}