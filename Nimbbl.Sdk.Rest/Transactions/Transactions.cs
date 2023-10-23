using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest;

public interface ITransactions
{
    Task<TransactionResponse> GetAsync(string transactionId);
    Task<TransactionByOrderIdResponse> GetByOrderIdAsync(string orderId);
}

internal class Transactions : ITransactions
{
    private const string Version = "v2/";
    private readonly ApiClient _apiClient;
    internal Transactions(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<TransactionResponse> GetAsync(string transactionId)
    {
        return _apiClient.GetWithAuth<TransactionResponse>($"{Version}fetch-transaction/{transactionId}");
    }

    public Task<TransactionByOrderIdResponse> GetByOrderIdAsync(string orderId)
    {
        return _apiClient.GetWithAuth<TransactionByOrderIdResponse>($"{Version}order/fetch-transactions/{orderId}");
    }
}