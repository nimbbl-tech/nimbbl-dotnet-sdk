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
    
    // Action values
    public const string ActionSend = "send";
    public const string ActionCancel = "cancel";
    public const string IdentifierRequired = "Either invoice_id or payment_link_id must be provided in the attributes array.";

    // Unsupported operation errors
    public const string UnsupportedOperation = "Unsupported operation.";
    public const string UnsupportedOperationUpdate = "Unsupported operation. Use update() method instead.";
    public const string UnsupportedOperationOrderModify = "Unsupported operation. Orders cannot be modified after creation.";


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

    // Token generation errors
    public const string AccessKeyMissing = "access_key is missing or empty. Please provide a valid access_key in your configuration.";
    public const string AccessSecretMissing = "access_secret is missing or empty. Please provide a valid access_secret in your configuration.";
    public const string TokenNotFoundInResponse = "Failed to generate merchant token: token not found in response";
    public const string TokenEmpty = "Failed to generate merchant token: token is empty";
    public const string NoValidTokenAvailable = "No valid token available and failed to generate merchant token.";
    public const string CheckCredentialsOrNetwork = "Please check your access_key and access_secret credentials, or verify network connectivity.";
    public const string TokenGenerationErrorPrefix = "Token generation error: ";

    // Authentication service error messages (format strings)
    public const string AuthenticationFailedFormat = "Authentication failed ({0}): Invalid access_key or access_secret. {1}";
    public const string ServiceUnavailableFormat = "Service temporarily unavailable ({0}): The token generation service is currently down or unreachable. Please try again later. {1}";
    public const string BadRequestFormat = "Bad request ({0}): Invalid request parameters. {1}";
    public const string TokenGenerationFailedFormat = "Failed to generate token ({0}): {1}";

    // Error message keywords for detection
    public const string AccessKeyKeyword = "access_key";
    public const string AccessSecretKeyword = "access_secret";
    public const string AuthenticationFailedKeyword = "Authentication failed";
    public const string ServiceUnavailableKeyword = "Service temporarily unavailable";
    public const string NetworkKeyword = "network";
    public const string UnreachableKeyword = "unreachable";

    // API validation errors
    public const string WebhookPayloadEmpty = "Webhook payload is empty";
    public const string InvalidTransactionIdFormat = "Invalid transaction_id format";
    public const string InvalidMerchantTokenFormat = "Invalid merchant_token format";
    public const string TransactionIdRequired = "transaction_id is required";
    public const string MerchantTokenRequired = "merchant_token is required";

    // Encryption log messages
    public const string LogPayloadEncryptedSuccessfully = "payload encrypted successfully";
    public const string LogStartingPayloadEncryption = "Starting payload encryption";
    public const string LogEncryptionDisabled = "Encryption disabled, sending plain payload";
    
    // Encryption error messages (format strings)
    // {0} = payload type/name (e.g., "refund", "order", "transaction enquiry", "list banks", "list wallets")
    // {1} = exception error message
    public const string EncryptionErrorFormat = "Failed to encrypt {0} payload: {1}";
}

