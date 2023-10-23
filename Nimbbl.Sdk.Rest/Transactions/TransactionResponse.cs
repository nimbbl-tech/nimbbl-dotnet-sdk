namespace Nimbbl.Sdk.Rest;

public class TransactionResponse
{
    public string TransactionId { get; init; }
    public string Status { get; init; }
    public string PaymentPartner { get; init; }
    public string PaymentMode { get; init; }
    public string TransactionType { get; init; }
    public string? VpaId { get; init; }
    public string MaskedCard { get; init; }
    public string HolderName { get; init; }
    public string Issuer { get; init; }
    public string Network { get; init; }
    public string Expiry { get; init; }
    public bool? RetryAllowed { get; init; }
    public string NimbblMerchantMessage { get; init; }
    public string NimbblConsumerMessage { get; init; }
    public string NimbblErrorCode { get; init; }
    public string? RefundDone { get; init; }
    public string? WalletName { get; init; }
    public string? BankName { get; init; }
    public long SubMerchantId { get; init; }
    public long MerchantId { get; init; }
    public string PspGeneratedTxnId { get; init; }
    public bool WebhookSent { get; init; }
    public decimal GrandTotalAmount { get; init; }
    public decimal AdditionalCharges { get; init; }
    public string PspGeneratedRedirectType { get; init; }
    public string PspGeneratedRedirect { get; init; }
    public string MerchantCallbackUrl { get; init; }
    public string VpaHolder { get; init; }
    public string VpaAppName { get; init; }
    public string PaytmPaymentMode { get; init; }
    public string FreechargePaymentMode { get; init; }
}