namespace Nimbbl.Sdk.Rest;

public class WebhookCallbackResponse {
    public string Status {get; init;}
    public string Message {get; init;}
    public string NimbblOrderId {get; init;}
    public string? NimbblSignature {get; init;}
    public string NimbblTransactionId {get; init;}
    public string OrderId {get; init;}
    public string TransactionId {get; init;}
    public string Signature {get; init;}
    public WCTransaction? Transaction {get; init;}
    public WCOrder Order {get; init;}
}