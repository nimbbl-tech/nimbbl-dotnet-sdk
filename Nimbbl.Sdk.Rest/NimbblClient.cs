using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest;

public class NimbblClient : INimbblClient
{
    public IOrders Orders { get; }
    public ITransactions Transactions { get; }
    public IUsers Users { get; }

    public NimbblClient(Config config)
    {
        var apiClient = new ApiClient(config);
        Orders = new Orders(apiClient);
        Transactions = new Transactions(apiClient);
        Users = new Users(apiClient);
    }
}