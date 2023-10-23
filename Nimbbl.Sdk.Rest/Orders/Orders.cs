using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest;

internal class Orders : IOrders
{
    private const string Version = "v2/";
    private readonly ApiClient _apiClient;
    internal Orders(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<Order> CreateAsync(OrderRequest orderRequest)
    {
        return _apiClient.PostWithAuth<OrderRequest, Order>($"{Version}create-order", orderRequest);
    }

    public Task<Order> GetByIdAsync(string orderId)
    {
        return _apiClient.GetWithAuth<Order>($"{Version}get-order/{orderId}");
    }
}