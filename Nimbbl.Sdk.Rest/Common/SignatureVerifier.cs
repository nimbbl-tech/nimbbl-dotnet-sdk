using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Utility helpers for signature and webhook verification.
/// </summary>
public class SignatureVerifier
{
    /// <summary>
    /// Verify signature for payment status callbacks and webhooks.
    /// Uses format: invoice_id|transaction_id|transaction_amount|transaction_currency|status|transaction_type
    /// </summary>
    /// <param name="attributes">Response attributes containing transaction and order data</param>
    /// <param name="secretKey">Secret key for signature verification</param>
    /// <returns>Signature verification result</returns>
    public static SignatureVerificationResult VerifyPaymentSignature(JsonElement attributes, string? secretKey = null)
    {
        var secret = string.IsNullOrWhiteSpace(secretKey) ? throw new ArgumentException(ErrorMessages.SecretKeyRequired, nameof(secretKey)) : secretKey!;
        var logger = Log.Logger.GetInstance();

        if (!attributes.TryGetProperty(JsonKeys.Transaction, out var txn) || txn.ValueKind != JsonValueKind.Object)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: {JsonKeys.Transaction}");
            return SignatureVerificationResult.FromMissingParams([JsonKeys.Transaction]);
        }

        var order = attributes.TryGetProperty(JsonKeys.Order, out var orderProp) && orderProp.ValueKind == JsonValueKind.Object
            ? orderProp
            : default;

        var signatureVersion = TryGetString(txn, JsonKeys.SignatureVersion) ?? TryGetString(txn, "signatureVersion") ?? SdkConstants.SignatureVersionV3;

        // Only support v3 signature format
        if (signatureVersion != SdkConstants.SignatureVersionV3)
        {
            var failMsg = $"Unsupported signature version: {signatureVersion}. Only {SdkConstants.SignatureVersionV3} is supported.";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return SignatureVerificationResult.FromFailed(failMsg);
        }

        // signature fallback chain: nimbbl_signature, transaction.nimbbl_signature, transaction.signature, order.nimbbl_signature
        var signature = TryGetString(attributes, JsonKeys.NimbblSignature)
            ?? TryGetString(txn, JsonKeys.NimbblSignature)
            ?? TryGetString(txn, JsonKeys.Signature)
            ?? TryGetString(order, JsonKeys.NimbblSignature);

        var transactionId = TryGetString(attributes, JsonKeys.NimbblTransactionId)
            ?? TryGetString(txn, JsonKeys.TransactionId);
        
        var invoiceId = TryGetString(order, JsonKeys.InvoiceId) ?? TryGetString(attributes, JsonKeys.InvoiceId);
        var transactionType = TryGetString(txn, JsonKeys.TransactionType) ?? TryGetString(txn, JsonKeys.Type);
        var transactionAmount = TryGetDouble(txn, JsonKeys.TransactionAmount) ?? TryGetDouble(txn, JsonKeys.Amount);
        var transactionCurrency = TryGetString(txn, JsonKeys.TransactionCurrency) ?? TryGetString(txn, JsonKeys.Currency);
        var status = TryGetString(txn, JsonKeys.Status);

        var missing = new List<string>();
        if (string.IsNullOrEmpty(invoiceId)) missing.Add(JsonKeys.InvoiceId);
        if (string.IsNullOrEmpty(transactionId)) missing.Add(JsonKeys.TransactionId);
        if (transactionAmount == null) missing.Add(JsonKeys.TransactionAmount);
        if (string.IsNullOrEmpty(transactionCurrency)) missing.Add(JsonKeys.TransactionCurrency);
        if (string.IsNullOrEmpty(status)) missing.Add(JsonKeys.Status);
        if (string.IsNullOrEmpty(transactionType)) missing.Add(JsonKeys.TransactionType);
        if (string.IsNullOrEmpty(signature)) missing.Add(JsonKeys.Signature);

        if (missing.Count > 0)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: {string.Join(", ", missing)}");
            return SignatureVerificationResult.FromMissingParams(missing);
        }

        var amountStr = FormatAmount(transactionAmount!.Value);
        var payload = $"{invoiceId}|{transactionId}|{amountStr}|{transactionCurrency}|{status}|{transactionType}";
        var expected = GenerateHmacSignature(payload, secret);

        if (!SecureStringEquals(expected, signature!))
        {
            var failMsg = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Transaction ID: {transactionId}, Amount: {amountStr}, Currency: {transactionCurrency}, Status: {status}, Type: {transactionType}";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return SignatureVerificationResult.FromFailed(failMsg);
        }

        var okMsg = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Transaction ID: {transactionId}, Amount: {amountStr}";
        var successMsg = ErrorMessages.MessageSignatureVerificationSuccess ?? "Signature verification succeeded";
        logger.InfoWithCaller($"{successMsg} - {okMsg}");
        return SignatureVerificationResult.FromSuccess(okMsg);
    }

    /// <summary>
    /// Verify signature for refund callbacks and webhooks.
    /// Uses format: invoice_id|transaction_id|refund_amount|transaction_currency|refund_status|transaction_type
    /// </summary>
    /// <param name="attributes">Response attributes containing transaction and order data</param>
    /// <param name="secretKey">Secret key for signature verification</param>
    /// <returns>Signature verification result</returns>
    public static SignatureVerificationResult VerifyRefundSignature(JsonElement attributes, string? secretKey = null)
    {
        var secret = string.IsNullOrWhiteSpace(secretKey) ? throw new ArgumentException(ErrorMessages.SecretKeyRequired, nameof(secretKey)) : secretKey!;
        var logger = Log.Logger.GetInstance();

        if (!attributes.TryGetProperty(JsonKeys.Transaction, out var txn) || txn.ValueKind != JsonValueKind.Object)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: {JsonKeys.Transaction}");
            return SignatureVerificationResult.FromMissingParams([JsonKeys.Transaction]);
        }

        var order = attributes.TryGetProperty(JsonKeys.Order, out var orderProp) && orderProp.ValueKind == JsonValueKind.Object
            ? orderProp
            : default;

        var signatureVersion = TryGetString(txn, JsonKeys.SignatureVersion) ?? TryGetString(txn, "signatureVersion") ?? SdkConstants.SignatureVersionV3;

        // Only support v3 signature format
        if (signatureVersion != SdkConstants.SignatureVersionV3)
        {
            var failMsg = $"Unsupported signature version: {signatureVersion}. Only {SdkConstants.SignatureVersionV3} is supported.";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return SignatureVerificationResult.FromFailed(failMsg);
        }

        // signature fallback chain: nimbbl_signature, transaction.nimbbl_signature, transaction.signature, order.nimbbl_signature
        var signature = TryGetString(attributes, JsonKeys.NimbblSignature)
            ?? TryGetString(txn, JsonKeys.NimbblSignature)
            ?? TryGetString(txn, JsonKeys.Signature)
            ?? TryGetString(order, JsonKeys.NimbblSignature);

        var transactionId = TryGetString(attributes, JsonKeys.NimbblTransactionId)
            ?? TryGetString(txn, JsonKeys.TransactionId);
        
        var invoiceId = TryGetString(order, JsonKeys.InvoiceId) ?? TryGetString(attributes, JsonKeys.InvoiceId);
        var transactionType = TryGetString(txn, JsonKeys.TransactionType) ?? TryGetString(txn, JsonKeys.Type);
        var refundAmount = TryGetDouble(txn, JsonKeys.RefundAmount)
            ?? TryGetDouble(txn, "payment_transaction_amount")
            ?? TryGetDouble(txn, JsonKeys.TransactionAmount)
            ?? TryGetDouble(txn, JsonKeys.Amount);
        var transactionCurrency = TryGetString(txn, JsonKeys.TransactionCurrency)
            ?? TryGetString(txn, JsonKeys.Currency)
            ?? TryGetString(order, JsonKeys.Currency);
        var status = TryGetString(txn, JsonKeys.RefundStatus) ?? TryGetString(txn, JsonKeys.Status);

        var missing = new List<string>();
        if (string.IsNullOrEmpty(invoiceId)) missing.Add(JsonKeys.InvoiceId);
        if (string.IsNullOrEmpty(transactionId)) missing.Add(JsonKeys.TransactionId);
        if (refundAmount == null) missing.Add(JsonKeys.RefundAmount);
        if (string.IsNullOrEmpty(transactionCurrency)) missing.Add(JsonKeys.TransactionCurrency);
        if (string.IsNullOrEmpty(status)) missing.Add(JsonKeys.RefundStatus);
        if (string.IsNullOrEmpty(transactionType)) missing.Add(JsonKeys.TransactionType);
        if (string.IsNullOrEmpty(signature)) missing.Add(JsonKeys.Signature);

        if (missing.Count > 0)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: {string.Join(", ", missing)}");
            return SignatureVerificationResult.FromMissingParams(missing);
        }

        var amountStr = FormatAmount(refundAmount!.Value);
        var payload = $"{invoiceId}|{transactionId}|{amountStr}|{transactionCurrency}|{status}|{transactionType}";
        var expected = GenerateHmacSignature(payload, secret);

        if (!SecureStringEquals(expected, signature!))
        {
            var failMsg = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Transaction ID: {transactionId}, Refund Amount: {amountStr}, Currency: {transactionCurrency}, Status: {status}, Type: {transactionType}";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return SignatureVerificationResult.FromFailed(failMsg);
        }

        var okMsg = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Transaction ID: {transactionId}, Refund Amount: {amountStr}";
        var successMsg = ErrorMessages.MessageSignatureVerificationSuccess ?? "Signature verification succeeded";
        logger.InfoWithCaller($"{successMsg} - {okMsg}");
        return SignatureVerificationResult.FromSuccess(okMsg);
    }

    /// <summary>
    /// Verify signature for payment link callbacks and webhooks.
    /// Uses format: invoice_id|status|currency|amount_paid|payment_link_hash
    /// </summary>
    /// <param name="attributes">Response attributes containing payment link data</param>
    /// <param name="secretKey">Secret key for signature verification</param>
    /// <returns>Signature verification result</returns>
    public static SignatureVerificationResult VerifyPaymentLinkSignature(JsonElement attributes, string? secretKey = null)
    {
        var secret = string.IsNullOrWhiteSpace(secretKey) ? throw new ArgumentException(ErrorMessages.SecretKeyRequired, nameof(secretKey)) : secretKey!;
        var logger = Log.Logger.GetInstance();

        var signatureVersion = TryGetString(attributes, JsonKeys.SignatureVersion) ?? SdkConstants.SignatureVersionV3;

        // Only support v3 signature format
        if (signatureVersion != SdkConstants.SignatureVersionV3)
        {
            var failMsg = $"Unsupported signature version: {signatureVersion}. Only {SdkConstants.SignatureVersionV3} is supported.";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return SignatureVerificationResult.FromFailed(failMsg);
        }

        // signature fallback chain: nimbbl_signature, signature
        var signature = TryGetString(attributes, JsonKeys.NimbblSignature)
            ?? TryGetString(attributes, JsonKeys.Signature);
        
        var invoiceId = TryGetString(attributes, JsonKeys.InvoiceId);
        var status = TryGetString(attributes, JsonKeys.Status);
        var currency = TryGetString(attributes, JsonKeys.Currency);
        var amountPaid = TryGetDouble(attributes, JsonKeys.AmountPaid) ?? TryGetDouble(attributes, JsonKeys.PaymentLinkAmountPaid) ?? 0.0;
        var paymentLinkHash = TryGetString(attributes, JsonKeys.PaymentLinkHash);

        var missing = new List<string>();
        if (string.IsNullOrEmpty(invoiceId)) missing.Add(JsonKeys.InvoiceId);
        if (string.IsNullOrEmpty(status)) missing.Add(JsonKeys.Status);
        if (string.IsNullOrEmpty(currency)) missing.Add(JsonKeys.Currency);
        if (string.IsNullOrEmpty(paymentLinkHash)) missing.Add(JsonKeys.PaymentLinkHash);
        if (string.IsNullOrEmpty(signature)) missing.Add(JsonKeys.Signature);

        if (missing.Count > 0)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: {string.Join(", ", missing)}");
            return SignatureVerificationResult.FromMissingParams(missing);
        }

        var amountStr = FormatAmount(amountPaid);
        var payload = $"{invoiceId}|{status}|{currency}|{amountStr}|{paymentLinkHash}";
        var expected = GenerateHmacSignature(payload, secret);

        if (!SecureStringEquals(expected, signature!))
        {
            var failMsg = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Status: {status}, Currency: {currency}, Amount Paid: {amountStr}, Payment Link Hash: {paymentLinkHash}";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return SignatureVerificationResult.FromFailed(failMsg);
        }

        var okMsg = $"Signature Version: {signatureVersion}, Invoice ID: {invoiceId}, Amount Paid: {amountStr}";
        var successMsg = ErrorMessages.MessageSignatureVerificationSuccess ?? "Signature verification succeeded";
        logger.InfoWithCaller($"{successMsg} - {okMsg}");
        return SignatureVerificationResult.FromSuccess(okMsg);
    }

    /// <summary>
    /// Verify signature for payment/refund callbacks and webhooks.
    /// Routes to the appropriate verification method based on webhook event type.
    /// </summary>
    /// <param name="attributes">Response attributes containing transaction and order data</param>
    /// <param name="secretKey">Secret key for signature verification</param>
    /// <returns>Signature verification result</returns>
    public static SignatureVerificationResult VerifySignature(JsonElement attributes, string? secretKey = null)
    {
        var logger = Log.Logger.GetInstance();

        // event_type is required - treat missing event_type as invalid payload
        var eventTypeStr = TryGetString(attributes, JsonKeys.EventType);
        if (string.IsNullOrWhiteSpace(eventTypeStr))
        {
            var failMsg = $"Missing required field: {JsonKeys.EventType}. Invalid webhook payload.";
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return SignatureVerificationResult.FromFailed(failMsg);
        }

        var webhookEventType = WebhookEventTypeExtensions.ParseEventType(eventTypeStr);

        // Route to appropriate verification method based on event type
        if (webhookEventType.IsPaymentLinkEvent())
        {
            return VerifyPaymentLinkSignature(attributes, secretKey);
        }

        if (webhookEventType.IsRefundEvent())
        {
            return VerifyRefundSignature(attributes, secretKey);
        }

        // Default to payment signature verification (PaymentSuccess or other payment events)
        return VerifyPaymentSignature(attributes, secretKey);
    }

    public static SignatureVerificationResult VerifyAndParseWebhook(string payload, string secret, out JsonElement parsed)
    {
        var logger = Log.Logger.GetInstance();
        using var doc = JsonDocument.Parse(payload);
        parsed = doc.RootElement.Clone();
        var result = VerifySignature(parsed, secret);
        if (!result.Success)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageWebhookVerificationFailed}: {result.Message}");
        }
        else
        {
            logger.InfoWithCaller($"{ErrorMessages.MessageSignatureVerificationSuccess}");
        }
        return result;
    }

    public static JsonElement? ParseWebhookEvent(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return null;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            return doc.RootElement.Clone();
        }
        catch (System.Exception ex)
        {
            var logger = Log.Logger.GetInstance();
            logger.ErrorWithCaller($"{ErrorMessages.MessageWebhookParseError}: {ex.Message}");
            return null;
        }
    }

    public static string GenerateHmacSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return BitConverter.ToString(signature).Replace("-", "").ToLowerInvariant();
    }

    public static bool SecureStringEquals(string expected, string actual)
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

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static double? TryGetDouble(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;
        if (!element.TryGetProperty(propertyName, out var value)) return null;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)) return d;
        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ds)) return ds;
        return null;
    }
}

public record SignatureVerificationResult(bool Success, string Message, SignatureVerificationError? Error)
{
    public static SignatureVerificationResult FromSuccess(string details) =>
        new(true, $"Signature verification succeeded - {details}", null);

    public static SignatureVerificationResult FromFailed(string details) =>
        new(false, $"Signature verification failed - {details}", new SignatureVerificationError(ErrorCodes.SignatureVerificationFailed, details));

    public static SignatureVerificationResult FromMissingParams(IEnumerable<string> missing) =>
        new(false, $"Signature verification failed - Missing {string.Join(", ", missing)}",
            new SignatureVerificationError(ErrorCodes.SignatureVerificationMissingParams, $"Missing {string.Join(", ", missing)}"));
}

public record SignatureVerificationError(string Code, string MerchantMessage);
