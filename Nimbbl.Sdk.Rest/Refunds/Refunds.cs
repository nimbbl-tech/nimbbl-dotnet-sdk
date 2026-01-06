using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.Log;
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
        var logger = Logger.GetInstance();
        var isEncryptEnabled = _apiClient.IsEncryptPayloadEnabled();
        logger.DebugWithCaller($"InitiateRefundAsync - Encryption enabled: {isEncryptEnabled}");
        
        // Encrypt payload if encryption is enabled
        if (isEncryptEnabled)
        {
            try
            {
                logger.DebugWithCaller("InitiateRefundAsync - Starting payload encryption");
                var encryption = new Encryption(_apiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(request);
                
                // Wrap encrypted payload in the format expected by API
                // The API accepts either a regular request or an encrypted payload
                request = new Dictionary<string, object?>
                {
                    ["encrypted_payload"] = encryptedPayload
                };
                
                logger.InfoWithCaller("Refund request payload encrypted successfully");
            }
            catch (System.Exception ex)
            {
                logger.ExceptionWithCaller($"Failed to encrypt refund payload: {ex.Message}", ex);
                throw new NimbblException($"Failed to encrypt refund payload: {ex.Message}", 500, "ENCRYPTION_ERROR");
            }
        }
        else
        {
            logger.DebugWithCaller("InitiateRefundAsync - Encryption disabled, sending plain payload");
        }
        
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.RefundInitiate, request);
    }
}

