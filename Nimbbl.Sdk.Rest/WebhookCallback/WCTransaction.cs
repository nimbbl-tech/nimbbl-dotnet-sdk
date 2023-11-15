namespace Nimbbl.Sdk.Rest;

public class WCTransaction {
    public string TransactionId {get; set;}
    public string Status {get; set;}
    public string PaymentPartner {get; set;}
    public string PSPTransactionId {get; set;}
    public double Amount {get; set;}
    public string TransactionType {get; set;}
    public string TransactionCurrency {get; set;}
    public double AdditionalCharges {get; set;}
    public double OfferDiscount {get; set;}
    public string? OfferId {get; set;}
    public string Signature {get; set;}
    public string SignatureVersion {get; set;}
}