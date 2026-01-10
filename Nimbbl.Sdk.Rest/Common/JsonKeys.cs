namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// JSON keys used in API requests and responses
/// </summary>
public static class JsonKeys
{
    // Error response keys
    public const string ErrorCode = "nimbbl_error_code";
    public const string ErrorMerchantMessage = "nimbbl_merchant_message";
    public const string ErrorConsumerMessage = "nimbbl_consumer_message";
    public const string Success = "success";
    public const string Error = "error";
    public const string Message = "message";
    public const string Valid = "valid";

    // Token response/request JSON keys
    public const string Token = "token";
    public const string ExpiresAt = "expires_at";
    public const string AccessKey = "access_key";
    public const string AccessSecret = "access_secret";
    public const string RefreshToken = "refresh_token";

    // Encryption JSON keys
    public const string EncryptedPayload = "encrypted_payload";
    public const string EncryptedResponse = "encrypted_response";

    // Request parameter keys
    public const string InvoiceId = "invoice_id";
    public const string PaymentLinkId = "payment_link_id";
    public const string Action = "action";
    public const string Pincode = "pincode";
}
