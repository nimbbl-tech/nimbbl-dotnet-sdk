using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest.Orders;

public class Orders : BaseService
{
    internal Orders(ApiClient apiClient) : base(apiClient)
    {
    }

    /// <summary>
    /// Create a new order.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="orderRequest">Order creation request parameters</param>
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/create-an-order-v-3/">Create Order API</see> for more details.</remarks>
    public Task<JsonElement> CreateOrderAsync(Dictionary<string, object?> orderRequest)
    {
        var isEncryptEnabled = ApiClient.IsEncryptPayloadEnabled();
        Logger.DebugWithCaller($"CreateOrderAsync - Encryption enabled: {isEncryptEnabled}");
        
        // Encrypt payload if encryption is enabled
        if (isEncryptEnabled)
        {
            try
            {
                Logger.DebugWithCaller($"CreateOrderAsync - {ErrorMessages.LogStartingPayloadEncryption}");
                var encryption = new Encryption(ApiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(orderRequest);
                
                // Wrap encrypted payload in the format expected by API
                // According to API docs: https://nimbbl.biz/docs/api-reference/create-an-order-v-3/
                // The API accepts either a regular request or an encrypted payload
                orderRequest = new Dictionary<string, object?>
                {
                    [JsonKeys.EncryptedPayload] = encryptedPayload
                };
                
                Logger.InfoWithCaller($"Order request {ErrorMessages.LogPayloadEncryptedSuccessfully}");
            }
            catch (System.Exception ex)
            {
                Logger.ExceptionWithCaller(string.Format(ErrorMessages.EncryptionErrorFormat, "order", ex.Message), ex);
                throw new NimbblException(string.Format(ErrorMessages.EncryptionErrorFormat, "order", ex.Message), HttpStatusCodes.Unknown, ErrorCodes.EncryptionError);
            }
        }
        else
        {
            Logger.DebugWithCaller($"CreateOrderAsync - {ErrorMessages.LogEncryptionDisabled}");
        }
        
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.OrderCreate, orderRequest);
    }

    /// <summary>
    /// Get order details by order ID.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-order-v-3/">Get Order API</see> for more details.</remarks>
    public Task<JsonElement> GetOrderByIdAsync(string orderId)
    {
        return ApiClient.Get<JsonElement>($"{ApiConstants.OrderGet}?order_id={orderId}");
    }

    /// <summary>
    /// Get order details by invoice ID.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-order-v-3/">Get Order API</see> for more details.</remarks>
    public Task<JsonElement> GetOrderByInvoiceIdAsync(string invoiceId)
    {
        return ApiClient.Get<JsonElement>($"{ApiConstants.OrderGet}?invoice_id={invoiceId}");
    }

}
