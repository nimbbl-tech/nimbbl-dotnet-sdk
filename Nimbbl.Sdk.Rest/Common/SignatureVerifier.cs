using System.Globalization;
using System.Linq;
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

        // Legacy (v3) payment-link webhooks nest the signed fields under a "payment_link" object
        // (and name the hash "hash"). Read them from the nested object when absent at the top level.
        var pl = attributes.ValueKind == JsonValueKind.Object
                 && attributes.TryGetProperty(JsonKeys.PaymentLink, out var plProp)
                 && plProp.ValueKind == JsonValueKind.Object
            ? plProp
            : default;

        // signature fallback chain: nimbbl_signature, signature (top level, then nested payment_link)
        var signature = JsonUtils.TryGetString(attributes, JsonKeys.NimbblSignature)
            ?? JsonUtils.TryGetString(attributes, JsonKeys.Signature)
            ?? JsonUtils.TryGetString(pl, JsonKeys.NimbblSignature)
            ?? JsonUtils.TryGetString(pl, JsonKeys.Signature);

        var invoiceId = JsonUtils.TryGetString(attributes, JsonKeys.InvoiceId) ?? JsonUtils.TryGetString(pl, JsonKeys.InvoiceId);
        var status = JsonUtils.TryGetString(attributes, JsonKeys.Status) ?? JsonUtils.TryGetString(pl, JsonKeys.Status);
        var currency = JsonUtils.TryGetString(attributes, JsonKeys.Currency) ?? JsonUtils.TryGetString(pl, JsonKeys.Currency);
        var amountPaid = JsonUtils.TryGetDouble(attributes, JsonKeys.AmountPaid) ?? JsonUtils.TryGetDouble(attributes, JsonKeys.PaymentLinkAmountPaid)
            ?? JsonUtils.TryGetDouble(pl, JsonKeys.AmountPaid) ?? JsonUtils.TryGetDouble(pl, JsonKeys.PaymentLinkAmountPaid) ?? 0.0;
        var paymentLinkHash = JsonUtils.TryGetString(attributes, JsonKeys.PaymentLinkHash)
            ?? JsonUtils.TryGetString(pl, JsonKeys.PaymentLinkHash)
            ?? JsonUtils.TryGetString(pl, JsonKeys.Hash);

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

    /// <summary>
    /// Low-level v4 envelope signature check.
    /// The v4 signature is an HMAC-SHA256 of the ENTIRE compact JSON string
    /// (the Base64-decoded <c>payload</c>) — not a per-field concatenation.
    /// </summary>
    /// <param name="rawCompactJson">The exact Base64-decoded payload string as Nimbbl sent it</param>
    /// <param name="providedSignature">The signature from the envelope</param>
    /// <param name="secret">The merchant's access secret key</param>
    /// <returns>True if the envelope signature is valid, false otherwise</returns>
    public static bool VerifyEnvelopeSignature(string rawCompactJson, string providedSignature, string? secret)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(providedSignature) || rawCompactJson == null)
        {
            return false;
        }

        var expected = GenerateHmacSignature(rawCompactJson, secret);
        return SecureStringEquals(expected, providedSignature);
    }

    /// <summary>
    /// Version-aware webhook verification entry point.
    ///
    /// Uses the top-level <c>version</c> field as the source of truth:
    ///  - version == "v4"  -> new envelope handling (HMAC over whole compact JSON; encrypted => decrypt authenticates)
    ///  - version absent / v1 / v2 / v3 -> legacy handling (parseResponse + VerifySignature)
    /// </summary>
    /// <param name="rawBody">The raw webhook POST body</param>
    /// <param name="secret">The merchant's access secret key</param>
    /// <returns>A <see cref="WebhookVerificationResult"/> with success, version, event_type and payload.</returns>
    public static WebhookVerificationResult VerifyWebhook(string rawBody, string secret)
    {
        var logger = Log.Logger.GetInstance();

        if (string.IsNullOrEmpty(rawBody))
        {
            return Result(false, "Empty webhook body");
        }

        var decoded = TryParseJsonClone(rawBody);
        if (decoded == null || decoded.Value.ValueKind != JsonValueKind.Object)
        {
            return Result(false, "Invalid JSON webhook body");
        }

        // Encrypted payloads must be handled BEFORE version dispatch: when encrypted, `version`
        // lives inside the ciphertext, so the outer body carries only `encrypted_response`.
        // Successful AES-GCM decryption authenticates it — no signature check.
        var encResult = HandleEncryptedPayload(decoded.Value, secret, "Webhook");
        if (encResult != null)
        {
            return encResult;
        }

        var version = JsonUtils.TryGetString(decoded.Value, JsonKeys.Version);

        // v4 -> signed-envelope handling; absent/v1/v2/v3 -> legacy per-field handling.
        if (version == SdkConstants.WebhookCallbackVersionV4)
        {
            logger.InfoWithCaller("VerifyWebhook: v4 payload detected, using envelope handling");
            return VerifyV4Envelope(decoded.Value, secret, JsonKeys.Signature, runPerField: true, apiTag: "Webhook");
        }

        // Legacy path (v1/v2/v3 or no version).
        logger.InfoWithCaller($"VerifyWebhook: legacy payload (version={version ?? "none"}), using legacy handling");
        try
        {
            var payload = PayloadHelperUtils.ParseResponse(rawBody, secret);
            var ok = VerifySignature(payload, secret);
            return Result(ok, ok ? "signature verified" : "signature verification failed", version: version ?? "legacy", payload: payload);
        }
        catch (System.Exception ex)
        {
            return Result(false, "webhook verification failed", error: ex.Message, version: version ?? "legacy");
        }
    }

    /// <summary>
    /// Version-aware callback verification entry point (server payment callback and
    /// client checkout callback forwarded to your server).
    ///
    /// Uses <c>version</c> as the source of truth. For v4 callbacks the envelope HMAC is the
    /// ONLY signature (v4 callbacks carry no per-field transaction.signature) and is signed
    /// under <c>nimbbl_signature</c>. Encrypted v4 callbacks carry no signature at all — a
    /// successful decryption authenticates them.
    /// </summary>
    /// <param name="rawBody">Raw callback body (Base64-encoded response string or raw JSON)</param>
    /// <param name="secret">The merchant's access secret key</param>
    /// <returns>A <see cref="WebhookVerificationResult"/> with success, version, event_type and payload.</returns>
    public static WebhookVerificationResult VerifyCallback(string rawBody, string secret)
    {
        var logger = Log.Logger.GetInstance();

        if (string.IsNullOrEmpty(rawBody))
        {
            return Result(false, "Empty callback body");
        }

        // Redirect callbacks arrive Base64-encoded; POST/popup callbacks arrive as raw JSON.
        var jsonStr = rawBody;
        var maybe = TryBase64ToString(rawBody);
        if (maybe != null && LooksLikeJson(maybe))
        {
            jsonStr = maybe;
        }

        var decoded = TryParseJsonClone(jsonStr);
        if (decoded == null || decoded.Value.ValueKind != JsonValueKind.Object)
        {
            return Result(false, "Invalid JSON callback body");
        }

        // Unwrap the outer checkout envelope (globalCloseCheckoutModal / globalHandleCheckoutResponse):
        // the signed object is the nested `payload`.
        var eventType = JsonUtils.TryGetString(decoded.Value, JsonKeys.EventType);
        var signed = decoded.Value;
        var isCheckoutWrapper = eventType == JsonKeys.GlobalCloseCheckoutModal || eventType == JsonKeys.GlobalHandleCheckoutResponse;
        if (isCheckoutWrapper && decoded.Value.TryGetProperty(JsonKeys.Payload, out var nested) && nested.ValueKind == JsonValueKind.Object)
        {
            signed = nested.Clone();
        }

        // Encrypted callbacks must be handled BEFORE version dispatch.
        var encResult = HandleEncryptedPayload(signed, secret, "Callback");
        if (encResult != null)
        {
            return encResult;
        }

        var version = JsonUtils.TryGetString(signed, JsonKeys.Version);

        if (version == SdkConstants.WebhookCallbackVersionV4)
        {
            logger.InfoWithCaller("VerifyCallback: v4 payload detected, using envelope handling");
            // v4 callbacks: envelope HMAC only, signed under `nimbbl_signature` (no inner per-field signature).
            return VerifyV4Envelope(signed, secret, JsonKeys.NimbblSignature, runPerField: false, apiTag: "Callback");
        }

        // Legacy path (v1/v2/v3 or no version).
        logger.InfoWithCaller($"VerifyCallback: legacy payload (version={version ?? "none"}), using legacy handling");
        try
        {
            var payload = PayloadHelperUtils.ParseResponse(rawBody, secret);
            var ok = VerifyCallbackSignature(payload, secret);
            return Result(ok, ok ? "signature verified" : "signature verification failed", version: version ?? "legacy", payload: payload);
        }
        catch (System.Exception ex)
        {
            return Result(false, "callback verification failed", error: ex.Message, version: version ?? "legacy");
        }
    }

    // --- v4 helper methods ---

    /// <summary>
    /// Verify a plaintext v4 signed envelope. The signature field name differs per source and is
    /// taken STRICTLY from <paramref name="sigKeyPrimary"/> (no fallback):
    ///  - Webhook v4:  { version, payload, signature, sub_merchant_id }        -> signature
    ///  - Callback v4: { version, payload, nimbbl_signature, sub_merchant_id } -> nimbbl_signature
    /// The HMAC is computed over the exact Base64-decoded <c>payload</c> (the inner compact JSON).
    /// </summary>
    private static WebhookVerificationResult VerifyV4Envelope(JsonElement envelope, string secret, string sigKeyPrimary, bool runPerField, string apiTag)
    {
        var logger = Log.Logger.GetInstance();

        var b64 = JsonUtils.TryGetString(envelope, JsonKeys.Payload);
        if (string.IsNullOrEmpty(b64))
        {
            return Result(false, $"Missing v4 {JsonKeys.Payload}");
        }

        // Use ONLY the signature key for this source (no fallback).
        var signature = JsonUtils.TryGetString(envelope, sigKeyPrimary);
        if (string.IsNullOrEmpty(signature))
        {
            var envKeys = envelope.ValueKind == JsonValueKind.Object
                ? string.Join(",", envelope.EnumerateObject().Select(p => p.Name))
                : string.Empty;
            return Result(false, $"Missing v4 signature ({sigKeyPrimary}); envelope keys: [{envKeys}]");
        }

        byte[] innerBytes;
        try
        {
            innerBytes = Convert.FromBase64String(b64);
        }
        catch
        {
            return Result(false, "v4 payload is not valid base64");
        }
        var inner = Encoding.UTF8.GetString(innerBytes);

        if (!VerifyEnvelopeSignature(inner, signature, secret))
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - v4 envelope signature mismatch");
            return Result(false, "v4 envelope signature mismatch");
        }

        var payload = TryParseJsonClone(inner);
        if (payload == null || payload.Value.ValueKind != JsonValueKind.Object)
        {
            return Result(false, "v4 inner payload invalid JSON");
        }

        // Webhook v4 retains the legacy per-field transaction.signature; cross-check when present.
        if (runPerField
            && payload.Value.TryGetProperty(JsonKeys.Transaction, out var txn)
            && txn.ValueKind == JsonValueKind.Object
            && !string.IsNullOrEmpty(JsonUtils.TryGetString(txn, JsonKeys.Signature)))
        {
            var perField = VerifySignature(payload.Value, secret);
            if (!perField)
            {
                return Result(false, "v4 envelope verified but per-field signature failed");
            }
        }

        var envCtx = EventLogContext(payload.Value, apiTag, JsonUtils.TryGetString(envelope, JsonKeys.SubMerchantId));
        logger.InfoWithCaller("v4 envelope signature verified", null, envCtx);
        return Result(true, "v4 envelope signature verified", version: SdkConstants.WebhookCallbackVersionV4, payload: payload);
    }

    /// <summary>
    /// Handle an encrypted webhook/callback payload. A top-level <c>encrypted_response</c> means the
    /// whole (v4 signed) envelope was AES-GCM encrypted. Successful decryption IS the authentication
    /// (the GCM tag guarantees integrity + authenticity), so NO separate HMAC/signature check is
    /// performed on the decrypted content. Returns null when the container is not encrypted.
    /// </summary>
    private static WebhookVerificationResult? HandleEncryptedPayload(JsonElement container, string secret, string apiTag)
    {
        var encrypted = JsonUtils.TryGetString(container, JsonKeys.EncryptedResponse);
        if (string.IsNullOrEmpty(encrypted))
        {
            return null; // not encrypted — caller continues with version-based dispatch
        }

        var logger = Log.Logger.GetInstance();

        string decryptedJson;
        try
        {
            decryptedJson = new Encryption(secret).Decrypt(encrypted, true);
        }
        catch (System.Exception ex)
        {
            logger.ErrorWithCaller($"{ErrorMessages.MessageSignatureVerificationFailed} - encrypted payload decryption failed: {ex.Message}");
            return Result(false, "encrypted payload decryption failed", error: ex.Message);
        }

        var decrypted = TryParseJsonClone(decryptedJson);
        if (decrypted == null || decrypted.Value.ValueKind != JsonValueKind.Object)
        {
            return Result(false, "encrypted payload could not be decoded");
        }

        // Unwrap the signed envelope's Base64 `payload` to the inner event (if present).
        // The envelope signature is intentionally NOT verified — decryption already authenticated it.
        var version = JsonUtils.TryGetString(decrypted.Value, JsonKeys.Version);
        var evt = decrypted.Value;
        var b64 = JsonUtils.TryGetString(decrypted.Value, JsonKeys.Payload);
        if (!string.IsNullOrEmpty(b64))
        {
            try
            {
                var innerRaw = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                var inner = TryParseJsonClone(innerRaw);
                if (inner != null && inner.Value.ValueKind == JsonValueKind.Object)
                {
                    evt = inner.Value;
                    version ??= JsonUtils.TryGetString(inner.Value, JsonKeys.Version);
                }
            }
            catch
            {
                // keep decrypted as the event
            }
        }

        var encCtx = EventLogContext(evt, apiTag, JsonUtils.TryGetString(container, JsonKeys.SubMerchantId));
        logger.InfoWithCaller("Decrypted payload authenticated via decryption", null, encCtx);
        return Result(true, "encrypted payload authenticated via decryption", version: version ?? SdkConstants.WebhookCallbackVersionV4, payload: evt);
    }

    /// <summary>
    /// Builds a <see cref="WebhookVerificationResult"/>, deriving <c>event_type</c> from the payload.
    /// </summary>
    private static WebhookVerificationResult Result(bool success, string message, string? error = null, string? version = null, JsonElement? payload = null)
    {
        string? eventType = payload.HasValue && payload.Value.ValueKind == JsonValueKind.Object
            ? JsonUtils.TryGetString(payload.Value, JsonKeys.EventType)
            : null;

        return new WebhookVerificationResult
        {
            Success = success,
            Message = message,
            Error = error,
            Version = version,
            EventType = eventType,
            Payload = payload
        };
    }

    /// <summary>
    /// Parses a JSON string into an independent (cloned) JsonElement, or null when it is not valid JSON.
    /// </summary>
    private static JsonElement? TryParseJsonClone(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to decode a Base64 string to a UTF-8 string, returning null when it is not valid Base64.
    /// </summary>
    private static string? TryBase64ToString(string value)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Cheap check whether a decoded string looks like a JSON object/array.
    /// </summary>
    private static bool LooksLikeJson(string str)
    {
        if (string.IsNullOrEmpty(str)) return false;
        var t = str.TrimStart();
        return t.Length > 0 && (t[0] == '{' || t[0] == '[');
    }

    /// <summary>
    /// Builds a structured <see cref="Log.LogContext"/> from a verified/decrypted payload so
    /// webhook/callback log lines are traceable (mirrors the PHP SDK's eventLogContext()):
    /// SubMerchantID / OrderID / InvoiceID / TransactionID / EventType.
    /// </summary>
    private static Log.LogContext EventLogContext(JsonElement payload, string apiTag, string? subMerchantId)
    {
        var order = payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty(JsonKeys.Order, out var o) && o.ValueKind == JsonValueKind.Object ? o : default;
        var txn = payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty(JsonKeys.Transaction, out var t) && t.ValueKind == JsonValueKind.Object ? t : default;

        return new Log.LogContext
        {
            ApiTag = apiTag,
            SubMerchantId = subMerchantId ?? JsonUtils.TryGetString(payload, JsonKeys.SubMerchantId),
            // Minimal v4 callbacks carry invoice_id / nimbbl_transaction_id at the TOP level;
            // full webhook payloads carry them under order / transaction.
            OrderId = JsonUtils.TryGetString(payload, JsonKeys.NimbblOrderId) ?? JsonUtils.TryGetString(order, JsonKeys.OrderId),
            InvoiceId = JsonUtils.TryGetString(payload, JsonKeys.InvoiceId) ?? JsonUtils.TryGetString(order, JsonKeys.InvoiceId),
            TransactionId = JsonUtils.TryGetString(payload, JsonKeys.NimbblTransactionId) ?? JsonUtils.TryGetString(txn, JsonKeys.TransactionId),
            EventType = JsonUtils.TryGetString(payload, JsonKeys.EventType),
        };
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

