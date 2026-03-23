namespace Nimbbl.Sdk.Rest.Common;

internal static class ErrorCodes
{
    public const string SignatureVerificationFailed = "SIGNATURE_VERIFICATION_FAILED";
    public const string SignatureVerificationMissingParams = "SIGNATURE_VERIFICATION_MISSING_PARAMS";
    public const string SignatureVerificationError = "SIGNATURE_VERIFICATION_ERROR";
    public const string DeserializationError = "DESERIALIZATION_ERROR";
    public const string AuthError = "AUTH_ERROR";
    public const string ServerError = "SERVER_ERROR";
    public const string InvalidAccessSecret = "INVALID_ACCESS_SECRET";
    public const string InvalidHexString = "INVALID_HEX_STRING";
    public const string DecryptionError = "DECRYPTION_ERROR";
    public const string EncryptionError = "ENCRYPTION_ERROR";
    public const string PincodeRequired = "PINCODE_REQUIRED";
    public const string RefreshTokenRequired = "REFRESH_TOKEN_REQUIRED";
    public const string TokenRequired = "TOKEN_REQUIRED";
    public const string IdentifierRequired = "IDENTIFIER_REQUIRED";
    public const string ActionRequired = "ACTION_REQUIRED";
    public const string InvalidAction = "INVALID_ACTION";
}

