using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Utility helpers for signature and webhook verification.
/// </summary>
public class Util
{
    public static SignatureVerificationResult VerifySignature(JsonElement attributes, string? secretKey = null)
    {
        var secret = string.IsNullOrWhiteSpace(secretKey) ? throw new ArgumentException(ErrorMessages.SecretKeyRequired, nameof(secretKey)) : secretKey!;

        if (!attributes.TryGetProperty("transaction", out var txn) || txn.ValueKind != JsonValueKind.Object)
        {
            var logger = Log.Logger.GetInstance();
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationMissingParams}: transaction");
            return SignatureVerificationResult.FromMissingParams(["transaction"]);
        }

        var order = attributes.TryGetProperty("order", out var orderProp) && orderProp.ValueKind == JsonValueKind.Object
            ? orderProp
            : default;

        var signatureVersion = TryGetString(txn, "signature_version") ?? TryGetString(txn, "signatureVersion") ?? "v3";

        // Only support v3 signature format
        if (signatureVersion != "v3")
        {
            var failMsg = $"Unsupported signature version: {signatureVersion}. Only v3 is supported.";
            var logger = Log.Logger.GetInstance();
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - {failMsg}");
            return SignatureVerificationResult.FromFailed(failMsg);
        }

        // signature fallback chain: nimbbl_signature, transaction.nimbbl_signature, transaction.signature, order.nimbbl_signature
        var signature = TryGetString(attributes, "nimbbl_signature")
            ?? TryGetString(txn, "nimbbl_signature")
            ?? TryGetString(txn, "signature")
            ?? TryGetString(order, "nimbbl_signature");

        var transactionId = TryGetString(attributes, "nimbbl_transaction_id")
            ?? TryGetString(txn, "transaction_id");

        // V3 signature verification
        {
            var logger = Log.Logger.GetInstance();
            
            var invoiceId = TryGetString(order, JsonKeys.InvoiceId) ?? TryGetString(attributes, JsonKeys.InvoiceId);
            var transactionType = TryGetString(txn, "transaction_type") ?? TryGetString(txn, "type");
            var eventType = TryGetString(attributes, "event_type");

            var isRefund = (!string.IsNullOrEmpty(transactionType) && transactionType.Contains("refund", StringComparison.OrdinalIgnoreCase)) ||
                           (!string.IsNullOrEmpty(eventType) && eventType.Contains("refund", StringComparison.OrdinalIgnoreCase));

            double? transactionAmount;
            string? transactionCurrency;
            string? status;

            if (isRefund)
            {
                transactionAmount = TryGetDouble(txn, "refund_amount")
                    ?? TryGetDouble(txn, "payment_transaction_amount")
                    ?? TryGetDouble(txn, "transaction_amount")
                    ?? TryGetDouble(txn, "amount");
                transactionCurrency = TryGetString(txn, "transaction_currency")
                    ?? TryGetString(txn, "currency")
                    ?? TryGetString(order, "currency");
                status = TryGetString(txn, "refund_status") ?? TryGetString(txn, "status");
            }
            else
            {
                transactionAmount = TryGetDouble(txn, "transaction_amount") ?? TryGetDouble(txn, "amount");
                transactionCurrency = TryGetString(txn, "transaction_currency") ?? TryGetString(txn, "currency");
                status = TryGetString(txn, "status");
            }

            var missing = new List<string>();
            if (string.IsNullOrEmpty(invoiceId)) missing.Add(JsonKeys.InvoiceId);
            if (string.IsNullOrEmpty(transactionId)) missing.Add("transaction_id");
            if (transactionAmount == null) missing.Add("transaction_amount");
            if (string.IsNullOrEmpty(transactionCurrency)) missing.Add("transaction_currency");
            if (string.IsNullOrEmpty(status)) missing.Add("status");
            if (string.IsNullOrEmpty(transactionType)) missing.Add("transaction_type");
            if (string.IsNullOrEmpty(signature)) missing.Add("signature");

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
    }

    public static SignatureVerificationResult VerifyAndParseWebhook(string payload, string secret, out JsonElement parsed)
    {
        using var doc = JsonDocument.Parse(payload);
        parsed = doc.RootElement.Clone();
        var result = VerifySignature(parsed, secret);
        if (!result.Success)
        {
            var logger = Log.Logger.GetInstance();
            logger.ErrorWithCaller($"{ErrorMessages.MessageWebhookVerificationFailed}: {result.Message}");
        }
        else
        {
            var logger = Log.Logger.GetInstance();
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

