using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Utility helpers for signature and webhook verification.
/// </summary>
public static class SignatureVerifier
{
    /// <summary>
    /// Verify signature for payment status callbacks and webhooks.
    /// Uses format: invoice_id|transaction_id|transaction_amount|transaction_currency|status|transaction_type
    /// </summary>
    /// <param name="attributes">Response attributes containing transaction and order data</param>
    /// <param name="secretKey">Secret key for signature verification</param>
    /// <returns>True if signature is valid, false otherwise</returns>
    public static bool VerifyPaymentSignature(JsonElement attributes, string? secretKey = null)
    {
        var (secret, logger) = InitializeVerifier(secretKey);
        
        // Log incoming JsonElement for debugging
        logger.InfoWithCaller($"VerifyPaymentSignature - Incoming JSON: {attributes.GetRawText()}");

        // Transaction object is required for payment signature verification
        if (!attributes.TryGetProperty(JsonKeys.Transaction, out var txn) || txn.ValueKind != JsonValueKind.Object)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: {JsonKeys.Transaction}");
            return false;
        }

        var order = attributes.TryGetProperty(JsonKeys.Order, out var orderProp) && orderProp.ValueKind == JsonValueKind.Object
            ? orderProp
            : default;

        // Read signatureVersion only from inside transaction object
        var signatureVersion = JsonUtils.TryGetString(txn, JsonKeys.SignatureVersion);

        // Read signature only from inside transaction object (no fallback to attributes or order)
        var signature = JsonUtils.TryGetString(txn, JsonKeys.Signature);

        // Read transaction_id only from inside the transaction object (no fallback)
        var transactionId = JsonUtils.TryGetString(txn, JsonKeys.TransactionId);

        var invoiceId = JsonUtils.TryGetString(order, JsonKeys.InvoiceId);
        var transactionType = JsonUtils.TryGetString(txn, JsonKeys.TransactionType);
        var transactionAmount = JsonUtils.TryGetDouble(txn, JsonKeys.TransactionAmount);
        var transactionCurrency = JsonUtils.TryGetString(txn, JsonKeys.TransactionCurrency);
        var status = JsonUtils.TryGetString(txn, JsonKeys.Status);

        var missing = new List<string>();
        if (string.IsNullOrEmpty(invoiceId)) missing.Add(JsonKeys.InvoiceId);
        if (string.IsNullOrEmpty(transactionId)) missing.Add(JsonKeys.TransactionId);
        if (transactionAmount == null) missing.Add(JsonKeys.TransactionAmount);
        if (string.IsNullOrEmpty(transactionCurrency)) missing.Add(JsonKeys.TransactionCurrency);
        if (string.IsNullOrEmpty(status)) missing.Add(JsonKeys.Status);
        if (string.IsNullOrEmpty(transactionType)) missing.Add(JsonKeys.TransactionType);
        if (string.IsNullOrEmpty(signature)) missing.Add(JsonKeys.Signature);

        var amountStr = transactionAmount != null ? FormatAmount(transactionAmount.Value) : string.Empty;
        var payload = $"{invoiceId}|{transactionId}|{amountStr}|{transactionCurrency}|{status}|{transactionType}";
        var failDetails = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Transaction ID: {transactionId}, Amount: {amountStr}, Currency: {transactionCurrency}, Status: {status}, Type: {transactionType}";
        var okDetails = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Transaction ID: {transactionId}, Amount: {amountStr}";

        return VerifyPayloadAndSignature(signatureVersion, signature, payload, missing, failDetails, okDetails, secret, logger);
    }

    /// <summary>
    /// Verify signature for refund callbacks and webhooks.
    /// Uses format: invoice_id|transaction_id|refund_amount|transaction_currency|refund_status|transaction_type
    /// </summary>
    /// <param name="attributes">Response attributes containing transaction and order data</param>
    /// <param name="secretKey">Secret key for signature verification</param>
    /// <returns>True if signature is valid, false otherwise</returns>
    public static bool VerifyRefundSignature(JsonElement attributes, string? secretKey = null)
    {
        var (secret, logger) = InitializeVerifier(secretKey);

        if (!attributes.TryGetProperty(JsonKeys.Transaction, out var txn) || txn.ValueKind != JsonValueKind.Object)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: {JsonKeys.Transaction}");
            return false;
        }

        var order = attributes.TryGetProperty(JsonKeys.Order, out var orderProp) && orderProp.ValueKind == JsonValueKind.Object
            ? orderProp
            : default;

        var signatureVersion = JsonUtils.TryGetString(txn, JsonKeys.SignatureVersion);

        // signature fallback chain: nimbbl_signature, transaction.nimbbl_signature, transaction.signature, order.nimbbl_signature
        var signature = JsonUtils.TryGetString(attributes, JsonKeys.NimbblSignature)
            ?? JsonUtils.TryGetString(txn, JsonKeys.NimbblSignature)
            ?? JsonUtils.TryGetString(txn, JsonKeys.Signature)
            ?? JsonUtils.TryGetString(order, JsonKeys.NimbblSignature);

        // Prioritize transaction_id from inside the transaction object (more authoritative)
        var transactionId = JsonUtils.TryGetString(txn, JsonKeys.TransactionId)
            ?? JsonUtils.TryGetString(attributes, JsonKeys.NimbblTransactionId);

        var invoiceId = JsonUtils.TryGetString(order, JsonKeys.InvoiceId) ?? JsonUtils.TryGetString(attributes, JsonKeys.InvoiceId);
        var transactionType = JsonUtils.TryGetString(txn, JsonKeys.TransactionType) ?? JsonUtils.TryGetString(txn, JsonKeys.Type);
        var refundAmount = JsonUtils.TryGetDouble(txn, JsonKeys.RefundAmount)
            ?? JsonUtils.TryGetDouble(txn, JsonKeys.PaymentTransactionAmount)
            ?? JsonUtils.TryGetDouble(txn, JsonKeys.TransactionAmount)
            ?? JsonUtils.TryGetDouble(txn, JsonKeys.Amount);
        var transactionCurrency = JsonUtils.TryGetString(txn, JsonKeys.TransactionCurrency)
            ?? JsonUtils.TryGetString(txn, JsonKeys.Currency)
            ?? JsonUtils.TryGetString(order, JsonKeys.Currency);
        var status = JsonUtils.TryGetString(txn, JsonKeys.RefundStatus) ?? JsonUtils.TryGetString(txn, JsonKeys.Status);

        var missing = new List<string>();
        if (string.IsNullOrEmpty(invoiceId)) missing.Add(JsonKeys.InvoiceId);
        if (string.IsNullOrEmpty(transactionId)) missing.Add(JsonKeys.TransactionId);
        if (refundAmount == null) missing.Add(JsonKeys.RefundAmount);
        if (string.IsNullOrEmpty(transactionCurrency)) missing.Add(JsonKeys.TransactionCurrency);
        if (string.IsNullOrEmpty(status)) missing.Add(JsonKeys.RefundStatus);
        if (string.IsNullOrEmpty(transactionType)) missing.Add(JsonKeys.TransactionType);
        if (string.IsNullOrEmpty(signature)) missing.Add(JsonKeys.Signature);

        var amountStr = refundAmount != null ? FormatAmount(refundAmount.Value) : string.Empty;
        var payload = $"{invoiceId}|{transactionId}|{amountStr}|{transactionCurrency}|{status}|{transactionType}";
        var failDetails = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Transaction ID: {transactionId}, Refund Amount: {amountStr}, Currency: {transactionCurrency}, Status: {status}, Type: {transactionType}";
        var okDetails = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Transaction ID: {transactionId}, Refund Amount: {amountStr}";

        return VerifyPayloadAndSignature(signatureVersion, signature, payload, missing, failDetails, okDetails, secret, logger);
    }

    /// <summary>
    /// Verify signature for payment link callbacks and webhooks.
    /// Uses format: invoice_id|status|currency|amount_paid|payment_link_hash
    /// </summary>
    /// <param name="attributes">Response attributes containing payment link data</param>
    /// <param name="secretKey">Secret key for signature verification</param>
    /// <returns>True if signature is valid, false otherwise</returns>
    public static bool VerifyPaymentLinkSignature(JsonElement attributes, string? secretKey = null)
    {
        var (secret, logger) = InitializeVerifier(secretKey);

        var signatureVersion = JsonUtils.TryGetString(attributes, JsonKeys.SignatureVersion) ?? SdkConstants.SignatureVersionV3;

        // signature fallback chain: nimbbl_signature, signature
        var signature = JsonUtils.TryGetString(attributes, JsonKeys.NimbblSignature)
            ?? JsonUtils.TryGetString(attributes, JsonKeys.Signature);

        var invoiceId = JsonUtils.TryGetString(attributes, JsonKeys.InvoiceId);
        var status = JsonUtils.TryGetString(attributes, JsonKeys.Status);
        var currency = JsonUtils.TryGetString(attributes, JsonKeys.Currency);
        var amountPaid = JsonUtils.TryGetDouble(attributes, JsonKeys.AmountPaid) ?? JsonUtils.TryGetDouble(attributes, JsonKeys.PaymentLinkAmountPaid) ?? 0.0;
        var paymentLinkHash = JsonUtils.TryGetString(attributes, JsonKeys.PaymentLinkHash);

        var missing = new List<string>();
        if (string.IsNullOrEmpty(invoiceId)) missing.Add(JsonKeys.InvoiceId);
        if (string.IsNullOrEmpty(status)) missing.Add(JsonKeys.Status);
        if (string.IsNullOrEmpty(currency)) missing.Add(JsonKeys.Currency);
        if (string.IsNullOrEmpty(paymentLinkHash)) missing.Add(JsonKeys.PaymentLinkHash);
        if (string.IsNullOrEmpty(signature)) missing.Add(JsonKeys.Signature);

        var amountStr = FormatAmount(amountPaid);
        var payload = $"{invoiceId}|{status}|{currency}|{amountStr}|{paymentLinkHash}";
        var failDetails = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Status: {status}, Currency: {currency}, Amount Paid: {amountStr}, Payment Link Hash: {paymentLinkHash}";
        var okDetails = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Amount Paid: {amountStr}";

        return VerifyPayloadAndSignature(signatureVersion, signature, payload, missing, failDetails, okDetails, secret, logger);
    }



    /// <summary>
    /// Verify signature for payment callbacks from the popup/redirect checkout.
    /// Handles nested "payload" structures and automatically detects/decrypts encrypted responses.
    /// </summary>
    /// <param name="payload">The raw JSON payload string to verify</param>
    /// <param name="secret">The merchant's access secret key</param>
    /// <returns>True if signature is valid, false otherwise</returns>
    public static bool VerifyCallbackSignature(JsonElement payload, string secret)
    {
        return VerifyPaymentSignature(payload, secret);
    }

    public static bool VerifySignature(string payload, string secret)
    {
        try
        {
            var parsed = PayloadHelperUtils.Parse(payload, secret);
            return VerifySignature(parsed, secret);
        }
        catch
        {
             return false;
        }
    }

    /// <summary>
    /// Verify signature for a parsed set of attributes.
    /// Routes to the appropriate verification method based on event type.
    /// </summary>
    /// <param name="attributes">The parsed JsonElement containing event attributes</param>
    /// <param name="secret">The merchant's access secret key</param>
    /// <returns>True if signature is valid, false otherwise</returns>
    public static bool VerifySignature(JsonElement attributes, string secret)
    {
        var logger = Log.Logger.GetInstance();
        var eventTypeStr = JsonUtils.TryGetString(attributes, JsonKeys.EventType);

        if (string.IsNullOrWhiteSpace(eventTypeStr))
        {
            var failMsg = $"Missing required field: {JsonKeys.EventType}. Invalid payload.";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return false;
        }

        var webhookEventType = WebhookEventTypeExtensions.ParseEventType(eventTypeStr);
        bool success;

        // Route to appropriate verification method based on event type
        if (webhookEventType.IsPaymentLinkEvent())
        {
            success = VerifyPaymentLinkSignature(attributes, secret);
        }
        else if (webhookEventType.IsRefundEvent())
        {
            success = VerifyRefundSignature(attributes, secret);
        }
        else
        {
             // Default to payment signature verification
            success = VerifyPaymentSignature(attributes, secret);
        }

        if (!success)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageWebhookVerificationFailed}");
        }
        else
        {
            logger.InfoWithCaller($"{ErrorMessages.MessageSignatureVerificationSuccess}");
        }
        return success;
    }

    private static bool VerifyPayloadAndSignature(string signatureVersion, string? signature, string payload, List<string> missing, string failDetails, string okDetails, string secret, Log.Logger logger)
    {
        // Only support v3 signature format
        if (signatureVersion != SdkConstants.SignatureVersionV3)
        {
            var failMsg = $"Unsupported signature version: {signatureVersion}. Only {SdkConstants.SignatureVersionV3} is supported.";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return false;
        }

        if (missing != null && missing.Count > 0)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: {string.Join(", ", missing)}");
            return false;
        }

        var expected = GenerateHmacSignature(payload, secret);
        if (!SecureStringEquals(expected, signature ?? string.Empty))
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failDetails}");
            return false;
        }

        var successMsg = ErrorMessages.MessageSignatureVerificationSuccess;
        logger.InfoWithCaller($"{successMsg} - {okDetails}");
        return true;
    }

    private static (string secret, Log.Logger logger) InitializeVerifier(string? secretKey)
    {
        var secret = string.IsNullOrWhiteSpace(secretKey) ? throw new ArgumentException(ErrorMessages.SecretKeyRequired, nameof(secretKey)) : secretKey!;
        var logger = Log.Logger.GetInstance();
        return (secret, logger);
    }

    private static string GenerateHmacSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return BitConverter.ToString(signature).Replace("-", "").ToLowerInvariant();
    }

    private static bool SecureStringEquals(string expected, string actual)
    {
        if (expected == null || actual == null) return false;
        if (expected.Length != actual.Length) return false;
        var diff = 0;
        for (int i = 0; i < expected.Length; i++)
        {
            diff |= expected[i] ^ actual[i];
        }
        return diff == 0;
    }

    private static string FormatAmount(double amount)
    {
        return amount.ToString("0.00", CultureInfo.InvariantCulture);
    }
}

