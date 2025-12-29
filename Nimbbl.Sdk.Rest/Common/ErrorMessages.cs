namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Error messages used throughout the SDK
/// </summary>
public static class ErrorMessages
{
    // Token-related errors
    public const string TokenRequired = "Token is required. Please provide a token parameter.";
    public const string AccessKeySecretRequired = "access_key and access_secret are required for generating a token.";
    public const string RefreshTokenRequired = "refresh_token is required for refreshing a token.";

    // Validation errors
    public const string PincodeRequired = "pincode is required for address eligibility check.";
    public const string ActionRequired = "action field is required. Possible values: send, cancel";
    public const string ActionInvalid = "Invalid action value. Possible values: send, cancel";
    public const string IdentifierRequired = "Either invoice_id or payment_link_id must be provided in the attributes array.";

    // Unsupported operation errors
    public const string UnsupportedOperation = "Unsupported operation.";
    public const string UnsupportedOperationUpdate = "Unsupported operation. Use update() method instead.";
    public const string UnsupportedOperationOrderModify = "Unsupported operation. Orders cannot be modified after creation.";

    // Error response keys
    public const string ErrorKeyErrorCode = "nimbbl_error_code";
    public const string ErrorKeyMerchantMessage = "nimbbl_merchant_message";
    public const string ErrorKeyConsumerMessage = "nimbbl_consumer_message";
    public const string ResponseKeySuccess = "success";
    public const string ResponseKeyError = "error";
    public const string ResponseKeyMessage = "message";
    public const string ResponseKeyValid = "valid";

    // Error codes
    public const string ErrorCodeUnsupportedOperation = "UNSUPPORTED_OPERATION";

    // Legacy message constants (for backward compatibility)
    public const string MessageApiRequestFailed = "API request failed";
    public const string MessageUnableToParseJson = "Unable to parse JSON";
    public const string MessageAuthenticationFailed = "Authentication failed";
    public const string MessageUnknownError = "Unknown error";
    public const string MessageServerError = "Server error";
    public const string MessageSignatureVerificationFailed = "Signature verification failed";
    public const string MessageSignatureVerificationSuccess = "Signature verification succeeded";
    public const string MessageSignatureVerificationMissingParams = "Signature verification failed - missing parameters";
    public const string MessageWebhookVerificationFailed = "Webhook verification failed";
    public const string MessageWebhookParseError = "Webhook parse failed";
    public const string MessageResponseContentNull = "Response content is null";
    public const string MessageNoValueReturned = "Server returned no value";
    public const string MessageTokenMissing = "Failed to get token from authentication response";

    // Checkout validation errors
    public const string OrderTokenRequired = "OrderToken is required";
    public const string CallbackBaseUrlRequiredForRedirect = "CallbackBaseUrl is required for redirect mode";
    public const string OrderTokenRequiredParam = "orderToken is required";

    // Encryption errors
    public const string AccessSecretRequired = "Access secret is required for encryption";
    public const string InvalidHexString = "Invalid hex string provided for decryption";
    public const string DecryptionError = "Decryption error: {0}";
    public const string EncryptionError = "Encryption error: {0}";

    // Authentication errors
    public const string InvalidTokenResponse = "Server returned invalid token";

    // Validation errors
    public const string SecretKeyRequired = "Secret key is required";

    // Environment variable errors
    public const string AccessKeyRequired = "NIMBBL_ACCESS_KEY environment variable is required";
    public const string AccessSecretRequiredEnv = "NIMBBL_ACCESS_SECRET environment variable is required";
    public const string AccessSecretRequiredEnvWithHint = "NIMBBL_ACCESS_SECRET environment variable is required. Please set it in your .env file or environment.";

    // Application errors
    public const string MerchantTokenUnavailable = "Merchant token unavailable";
    public const string OrderTokenNotReturned = "Order token not returned";
    public const string InvalidResponseFormat = "Invalid response format";

    // API validation errors
    public const string WebhookPayloadEmpty = "Webhook payload is empty";
    public const string InvalidTransactionIdFormat = "Invalid transaction_id format";
    public const string InvalidMerchantTokenFormat = "Invalid merchant_token format";
    public const string TransactionIdRequired = "transaction_id is required";
    public const string MerchantTokenRequired = "merchant_token is required";
}

