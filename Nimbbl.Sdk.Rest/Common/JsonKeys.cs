namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// JSON keys used in API requests and responses
/// </summary>
public static class JsonKeys
{
    // Error response keys
    public const string OrderSource = "order_source";
    public const string OrderSourceVersion = "order_source_version";
    public const string ErrorCode = "nimbbl_error_code";
    public const string ErrorMerchantMessage = "nimbbl_merchant_message";
    public const string ErrorConsumerMessage = "nimbbl_consumer_message";
    public const string Success = "success";
    public const string Error = "error";
    public const string Message = "message";
    public const string Valid = "valid";
    public const string Received = "received";
    public const string Parsed = "parsed";

    // Token response/request JSON keys
    public const string Token = "token";
    public const string ExpiresAt = "expires_at";
    public const string AccessKey = "access_key";
    public const string AccessSecret = "access_secret";
    public const string RefreshToken = "refresh_token";

    // Encryption JSON keys
    public const string EncryptedPayload = "encrypted_payload";
    public const string EncryptedResponse = "encrypted_response";
    public const string Payload = "payload";

    // v4 webhook / callback envelope keys
    public const string Version = "version";
    public const string SubMerchantId = "sub_merchant_id";

    // Request parameter keys
    public const string InvoiceId = "invoice_id";
    public const string OrderId = "order_id";
    public const string NimbblOrderId = "nimbbl_order_id";
    public const string PaymentLinkId = "payment_link_id";
    public const string Action = "action";
    public const string Pincode = "pincode";

    // Webhook and transaction keys
    public const string EventType = "event_type";
    public const string TransactionId = "transaction_id";
    public const string NimbblTransactionId = "nimbbl_transaction_id";
    public const string TransactionType = "transaction_type";
    public const string TransactionAmount = "transaction_amount";
    public const string TransactionCurrency = "transaction_currency";
    public const string Status = "status";
    public const string Signature = "signature";
    public const string NimbblSignature = "nimbbl_signature";
    public const string SignatureVersion = "signature_version";
    public const string SignatureValid = "signature_valid";
    public const string SignatureMessage = "signature_message";
    public const string SignatureError = "signature_error";
    public const string SignatureVerificationSkipped = "signature_verification_skipped";
    public const string RefundAmount = "refund_amount";
    public const string RefundStatus = "refund_status";
    public const string PaymentTransactionAmount = "payment_transaction_amount";
    public const string PaymentLinkHash = "payment_link_hash";
    public const string AmountPaid = "amount_paid";
    public const string PaymentLinkAmountPaid = "payment_link_amount_paid";
    public const string Currency = "currency";
    public const string Amount = "amount";
    public const string AmountBeforeTax = "amount_before_tax";
    public const string Reason = "reason";
    public const string PaymentMode = "payment_mode";
    public const string Type = "type";
    public const string Transaction = "transaction";
    public const string Order = "order";
    public const string Callback = "callback";
    public const string User = "user";
    public const string Name = "name";

    // Event type values
    public const string GlobalHandleCheckoutResponse = "globalHandleCheckoutResponse";
    public const string GlobalCloseCheckoutModal = "globalCloseCheckoutModal";

    // Transaction type values
    public const string TransactionTypePayment = "payment";
    public const string TransactionTypeFullRefund = "full-refund";
    public const string TransactionTypePartialRefund = "partial-refund";
    public const string RefundKeyword = "refund";

    // PII data keys for masking
    public const string FirstName = "first_name";
    public const string LastName = "last_name";
    public const string UpiHolder = "upi_holder";
    public const string CardHolderName = "card_holder_name";
    // Response-side PII field names (webhook/callback use short forms)
    public const string CardHolder = "card_holder";
    public const string Mobile = "mobile";
    public const string State = "state";
    public const string Street = "street";
    public const string Landmark = "landmark";
    public const string Area = "area";
    public const string City = "city";
    public const string PinCode = "pin_code";
    public const string PostalCode = "postal_code";
    public const string ZipCode = "zip_code";
    public const string Vpa = "vpa";
    public const string CardNo = "card_no";
    public const string CardNumber = "card_number";
    public const string Cvv = "cvv";
    public const string ExpiryDate = "expiry_date";
    public const string AccountNo = "account_no";
    public const string AccountNumber = "account_number";
    public const string IfscCode = "ifsc_code";
    public const string PanCard = "pan_card";
    public const string Email = "email";
    public const string MobileNumber = "mobile_number";
}
