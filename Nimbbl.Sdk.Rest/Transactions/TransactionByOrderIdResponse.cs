namespace Nimbbl.Sdk.Rest;

public class TransactionByOrderIdResponse
{
    public bool Success { get; init; }
    public IEnumerable<TransactionResponse> Transactions { get; init; } = new List<TransactionResponse>();
}