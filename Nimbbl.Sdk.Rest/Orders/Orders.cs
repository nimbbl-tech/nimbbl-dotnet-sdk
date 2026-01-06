using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.Log;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest.Orders;

public class Orders
{
    private readonly ApiClient _apiClient;
    internal Orders(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Create a new order.
    /// </summary>
    /// <param name="orderRequest">Order creation request parameters</param>
    /// <param name="token">Optional bearer token (takes priority over cached token)</param>
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/create-an-order-v-3/">Create Order API</see> for more details.</remarks>
    public Task<JsonElement> CreateOrderAsync(Dictionary<string, object?> orderRequest, string? token = null)
    {
        var logger = Logger.GetInstance();
        var isEncryptEnabled = _apiClient.IsEncryptPayloadEnabled();
        logger.DebugWithCaller($"CreateOrderAsync - Encryption enabled: {isEncryptEnabled}");
        
        // Encrypt payload if encryption is enabled
        if (isEncryptEnabled)
        {
            try
            {
                logger.DebugWithCaller("CreateOrderAsync - Starting payload encryption");
                var encryption = new Encryption(_apiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(orderRequest);
                
                // Wrap encrypted payload in the format expected by API
                // According to API docs: https://nimbbl.biz/docs/api-reference/create-an-order-v-3/
                // The API accepts either a regular request or an encrypted payload
                orderRequest = new Dictionary<string, object?>
                {
                    ["encrypted_payload"] = encryptedPayload
                };
                
                logger.InfoWithCaller("Order request payload encrypted successfully");
            }
            catch (System.Exception ex)
            {
                logger.ExceptionWithCaller($"Failed to encrypt order payload: {ex.Message}", ex);
                throw new NimbblException($"Failed to encrypt order payload: {ex.Message}", 500, "ENCRYPTION_ERROR");
            }
        }
        else
        {
            logger.DebugWithCaller("CreateOrderAsync - Encryption disabled, sending plain payload");
        }
        
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.OrderCreate, orderRequest, token);
    }

    /// <summary>
    /// Get order details by order ID.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="token">Optional bearer token (takes priority over cached token)</param>
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-order-v-3/">Get Order API</see> for more details.</remarks>
    public Task<JsonElement> GetOrderByIdAsync(string orderId, string? token = null)
    {
        return _apiClient.Get<JsonElement>($"{ApiConstants.OrderGet}?order_id={orderId}", token);
    }

    /// <summary>
    /// Get order details by invoice ID.
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <param name="token">Optional bearer token (takes priority over cached token)</param>
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-order-v-3/">Get Order API</see> for more details.</remarks>
    public Task<JsonElement> GetOrderByInvoiceIdAsync(string invoiceId, string? token = null)
    {
        return _apiClient.Get<JsonElement>($"{ApiConstants.OrderGet}?invoice_id={invoiceId}", token);
    }

}