using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
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
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/create-an-order-v-3/">Create Order API</see> for more details.</remarks>
    public Task<JsonElement> CreateOrderAsync(Dictionary<string, object?> orderRequest)
    {
        return _apiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.OrderCreate, orderRequest);
    }

    /// <summary>
    /// Get order details by order ID.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-order-v-3/">Get Order API</see> for more details.</remarks>
    public Task<JsonElement> GetOrderByIdAsync(string orderId)
    {
        return _apiClient.Get<JsonElement>($"{ApiConstants.OrderGet}?order_id={orderId}");
    }

    /// <summary>
    /// Get order details by invoice ID.
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <returns>JSON response containing order details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-order-v-3/">Get Order API</see> for more details.</remarks>
    public Task<JsonElement> GetOrderByInvoiceIdAsync(string invoiceId)
    {
        return _apiClient.Get<JsonElement>($"{ApiConstants.OrderGet}?invoice_id={invoiceId}");
    }

}