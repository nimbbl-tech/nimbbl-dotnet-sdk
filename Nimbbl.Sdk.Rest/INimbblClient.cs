namespace Nimbbl.Sdk.Rest;

public interface INimbblClient
{
    public IOrders Orders { get; }
    public ITransactions Transactions { get; }
    public IUsers Users { get; }
}