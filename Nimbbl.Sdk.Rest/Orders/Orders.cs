using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest;

public class Orders
{
    private readonly ApiClient _apiClient;
    internal Orders(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<JsonElement> CreateOrderAsync(Dictionary<string, object?> orderRequest)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.OrderCreate, orderRequest);
    }

    public Task<JsonElement> GetOrderByIdAsync(string orderId)
    {
        return _apiClient.GetWithAuth<JsonElement>($"{ApiConstants.OrderGet}?order_id={orderId}");
    }

    public Task<JsonElement> GetOrderByInvoiceIdAsync(string invoiceId)
    {
        return _apiClient.GetWithAuth<JsonElement>($"{ApiConstants.OrderGet}?invoice_id={invoiceId}");
    }

}