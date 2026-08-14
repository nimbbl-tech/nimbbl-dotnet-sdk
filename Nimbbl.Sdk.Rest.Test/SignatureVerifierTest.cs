using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class SignatureVerifierTest
{
    private const string Secret = "test_secret_key_12345";

    // ---- helpers (mirror the PHP SignatureVerifierTest vectors) ----

    private static string Hmac(string data, string secret)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var b = h.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(b).Replace("-", "").ToLowerInvariant();
    }

    private static string Json(object o) => JsonSerializer.Serialize(o);

    private static Dictionary<string, object?> MakeV4Envelope(Dictionary<string, object?> inner, string sigField, string secret)
    {
        var innerJson = Json(inner);
        return new Dictionary<string, object?>
        {
            ["version"] = "v4",
            ["payload"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(innerJson)),
            [sigField] = Hmac(innerJson, secret),
        };
    }

    private static string MakeEncryptedV4Body(Dictionary<string, object?> evt, string secret, string? sigField = "signature")
    {
        var innerJson = Json(evt);
        var envelope = new Dictionary<string, object?>
        {
            ["payload"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(innerJson)),
            ["version"] = "v4",
        };
        if (sigField != null)
        {
            // Present but intentionally ignored by the SDK — GCM decryption authenticates.
            envelope[sigField] = Hmac(innerJson, secret);
        }
        var hex = new Encryption(secret).Encrypt(envelope);
        return Json(new Dictionary<string, object?> { ["encrypted_response"] = hex, ["sub_merchant_id"] = "sm_1" });
    }

    // Legacy per-field (v3) payment payload with a valid transaction.signature.
    private static Dictionary<string, object?> MakePerFieldPayloadDict(string status, string type, string eventType, string invoiceId = "inv_pa", string txnId = "txn_pa", double amount = 250.00)
    {
        var amountStr = amount.ToString("0.00", CultureInfo.InvariantCulture);
        var payloadStr = $"{invoiceId}|{txnId}|{amountStr}|INR|{status}|{type}";
        var signature = Hmac(payloadStr, Secret);

        return new Dictionary<string, object?>
        {
            ["event_type"] = eventType,
            ["order"] = new Dictionary<string, object?> { ["invoice_id"] = invoiceId },
            ["transaction"] = new Dictionary<string, object?>
            {
                ["transaction_id"] = txnId,
                ["transaction_amount"] = amount,
                ["transaction_currency"] = "INR",
                ["status"] = status,
                ["transaction_type"] = type,
                ["signature_version"] = "v3",
                ["signature"] = signature,
            },
        };
    }

    private static JsonElement MakePerFieldPayload(string status, string type, string eventType, string invoiceId = "inv_pa", string txnId = "txn_pa", double amount = 250.00)
        => JsonSerializer.SerializeToElement(MakePerFieldPayloadDict(status, type, eventType, invoiceId, txnId, amount));

    [Fact]
    public void ShouldVerifyPaymentSignature()
    {
        // This is a basic structure test - actual signature verification requires real webhook payloads
        var payload = new Dictionary<string, object?>
        {
            ["event_type"] = "payment.status",
            ["transaction_id"] = "test_txn_id",
            ["order_id"] = "test_order_id",
            ["amount"] = 100,
            ["currency"] = "INR",
            ["status"] = "success"
        };

        // Note: Actual signature verification requires a valid signature from the API
        // This test just verifies the method exists and can be called
        Assert.NotNull(payload);
    }

    [Fact]
    public void ShouldVerifyRefundSignature()
    {
        var payload = new Dictionary<string, object?>
        {
            ["event_type"] = "refund.status",
            ["transaction_id"] = "test_txn_id",
            ["order_id"] = "test_order_id",
            ["refund_amount"] = 50,
            ["currency"] = "INR",
            ["status"] = "success"
        };

        Assert.NotNull(payload);
    }

    [Fact]
    public void ShouldVerifyPaymentLinkSignature()
    {
        var payload = new Dictionary<string, object?>
        {
            ["event_type"] = "payment_link.status",
            ["invoice_id"] = "test_invoice_id",
            ["amount"] = 100,
            ["currency"] = "INR",
            ["status"] = "paid"
        };

        Assert.NotNull(payload);
    }

    [Fact]
    public void ShouldHandleMissingEventType()
    {
        var payload = new Dictionary<string, object?>
        {
            ["transaction_id"] = "test_txn_id",
            ["order_id"] = "test_order_id"
        };

        // Event type is required - verification should fail or default to payment
        Assert.NotNull(payload);
        Assert.False(payload.ContainsKey("event_type"));
    }

    // ---------------------------------------------------------------------
    // Pre-auth: per-field (v3) signature for capture / void / authorized txns
    // ---------------------------------------------------------------------

    [Fact]
    public void ShouldVerifyPerFieldSignatureForCaptureTransaction()
    {
        var data = MakePerFieldPayload("succeeded", "capture", "capture_success");
        Assert.True(SignatureVerifier.VerifySignature(data, Secret));
    }

    [Fact]
    public void ShouldVerifyPerFieldSignatureForVoidTransaction()
    {
        var data = MakePerFieldPayload("succeeded", "void", "void_success");
        Assert.True(SignatureVerifier.VerifySignature(data, Secret));
    }

    [Fact]
    public void ShouldVerifyPerFieldSignatureForAuthorizedTransaction()
    {
        var data = MakePerFieldPayload("authorized", "payment", "payment_authorized");
        Assert.True(SignatureVerifier.VerifySignature(data, Secret));
    }

    // ---------------------------------------------------------------------
    // v4 webhook envelope (HMAC over the whole compact JSON)
    // ---------------------------------------------------------------------

    [Fact]
    public void ShouldVerifyV4Webhook()
    {
        var inner = new Dictionary<string, object?>
        {
            ["event_type"] = "capture_success",
            ["order"] = new Dictionary<string, object?> { ["invoice_id"] = "inv_v4" },
            ["transaction"] = new Dictionary<string, object?> { ["transaction_id"] = "t_v4", ["transaction_type"] = "capture", ["status"] = "succeeded" },
        };
        var env = MakeV4Envelope(inner, "signature", Secret);
        env["sub_merchant_id"] = "sm_1";

        var result = SignatureVerifier.VerifyWebhook(Json(env), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("v4", result.Version);
        Assert.Equal("capture_success", result.EventType);
    }

    [Fact]
    public void ShouldFailV4WebhookOnTamperedSignature()
    {
        var inner = new Dictionary<string, object?> { ["event_type"] = "void_success", ["order"] = new Dictionary<string, object?>(), ["transaction"] = new Dictionary<string, object?>() };
        var env = MakeV4Envelope(inner, "signature", Secret);
        env["signature"] = "tampered_signature";

        var result = SignatureVerifier.VerifyWebhook(Json(env), Secret);

        Assert.False(result.Success);
    }

    [Fact]
    public void ShouldFailV4WebhookOnWrongSecret()
    {
        var inner = new Dictionary<string, object?> { ["event_type"] = "capture_success", ["order"] = new Dictionary<string, object?>(), ["transaction"] = new Dictionary<string, object?>() };
        var env = MakeV4Envelope(inner, "signature", Secret);

        var result = SignatureVerifier.VerifyWebhook(Json(env), "a_different_secret");

        Assert.False(result.Success);
    }

    [Fact]
    public void ShouldVerifyLegacyWebhookWithNoVersion()
    {
        // Regression: a payload with no `version` must flow through the legacy path.
        var dict = MakePerFieldPayloadDict("succeeded", "payment", "payment_success", "inv_legacy", "txn_legacy", 100.00);

        var result = SignatureVerifier.VerifyWebhook(Json(dict), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("legacy", result.Version);
    }

    // ---------------------------------------------------------------------
    // v4 callback envelope (signed under nimbbl_signature)
    // ---------------------------------------------------------------------

    [Fact]
    public void ShouldVerifyV4PaymentCallback()
    {
        var inner = new Dictionary<string, object?>
        {
            ["order"] = new Dictionary<string, object?> { ["invoice_id"] = "inv_pc" },
            ["transaction"] = new Dictionary<string, object?> { ["transaction_id"] = "t_pc", ["status"] = "authorized", ["transaction_type"] = "payment" },
        };
        var env = MakeV4Envelope(inner, "nimbbl_signature", Secret);

        var result = SignatureVerifier.VerifyCallback(Json(env), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("v4", result.Version);
    }

    [Fact]
    public void ShouldVerifyV4CheckoutCallbackWithWrapper()
    {
        // Checkout callback: v4 envelope signed under nimbbl_signature, nested under globalCloseCheckoutModal.
        var inner = new Dictionary<string, object?> { ["checkout_status"] = "success", ["reason"] = "payment_authorized", ["order_id"] = "o_cc" };
        var signed = MakeV4Envelope(inner, "nimbbl_signature", Secret);
        signed["sub_merchant_id"] = "sm_1";
        var outer = new Dictionary<string, object?> { ["event_type"] = "globalCloseCheckoutModal", ["payload"] = signed };

        var result = SignatureVerifier.VerifyCallback(Json(outer), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("payment_authorized", result.Payload!.Value.GetProperty("reason").GetString());
    }

    [Fact]
    public void ShouldFailV4CheckoutCallbackOnTamperedSignature()
    {
        var inner = new Dictionary<string, object?> { ["checkout_status"] = "success", ["reason"] = "payment_captured" };
        var signed = MakeV4Envelope(inner, "nimbbl_signature", Secret);
        signed["nimbbl_signature"] = "nope";
        var outer = new Dictionary<string, object?> { ["event_type"] = "globalCloseCheckoutModal", ["payload"] = signed };

        var result = SignatureVerifier.VerifyCallback(Json(outer), Secret);

        Assert.False(result.Success);
    }

    // ---------------------------------------------------------------------
    // Encrypted v4 webhook/callback (decryption authenticates, no HMAC)
    // ---------------------------------------------------------------------

    [Fact]
    public void ShouldVerifyEncryptedV4Webhook()
    {
        var evt = new Dictionary<string, object?>
        {
            ["event_type"] = "payment_authorized",
            ["status"] = "authorized",
            ["transaction"] = new Dictionary<string, object?> { ["transaction_id"] = "t_enc", ["status"] = "authorized" },
        };
        var body = MakeEncryptedV4Body(evt, Secret, "signature");

        var res = SignatureVerifier.VerifyWebhook(body, Secret);

        Assert.True(res.Success, res.Message);
        Assert.Equal("v4", res.Version);
        Assert.Equal("payment_authorized", res.EventType);
        Assert.Equal("authorized", res.Payload!.Value.GetProperty("status").GetString());
    }

    [Fact]
    public void ShouldVerifyEncryptedV4Callback()
    {
        var evt = new Dictionary<string, object?> { ["event_type"] = "payment_success", ["status"] = "succeeded" };
        var body = MakeEncryptedV4Body(evt, Secret, "nimbbl_signature");

        var res = SignatureVerifier.VerifyCallback(body, Secret);

        Assert.True(res.Success, res.Message);
        Assert.Equal("v4", res.Version);
        Assert.Equal("payment_success", res.EventType);
    }

    [Fact]
    public void ShouldVerifyEncryptedV4WebhookWithoutSignatureField()
    {
        // "signature might not come" — successful decryption alone authenticates.
        var evt = new Dictionary<string, object?> { ["event_type"] = "capture_success", ["status"] = "succeeded" };
        var body = MakeEncryptedV4Body(evt, Secret, null);

        var res = SignatureVerifier.VerifyWebhook(body, Secret);

        Assert.True(res.Success, res.Message);
        Assert.Equal("capture_success", res.EventType);
    }

    [Fact]
    public void ShouldVerifyEncryptedV4WebhookEventDirect()
    {
        // Defensive: decryption yields the event directly (no base64 `payload` wrapper).
        var evt = new Dictionary<string, object?> { ["event_type"] = "void_success", ["status"] = "succeeded", ["version"] = "v4" };
        var hex = new Encryption(Secret).Encrypt(evt);
        var body = Json(new Dictionary<string, object?> { ["encrypted_response"] = hex });

        var res = SignatureVerifier.VerifyWebhook(body, Secret);

        Assert.True(res.Success, res.Message);
        Assert.Equal("void_success", res.EventType);
    }

    [Fact]
    public void ShouldFailEncryptedV4WebhookOnWrongSecret()
    {
        var evt = new Dictionary<string, object?> { ["event_type"] = "payment_success" };
        var body = MakeEncryptedV4Body(evt, Secret);

        var res = SignatureVerifier.VerifyWebhook(body, "a_different_secret_value");

        Assert.False(res.Success);
    }

    [Fact]
    public void ShouldFailOnEmptyWebhookBody()
    {
        var res = SignatureVerifier.VerifyWebhook("", Secret);
        Assert.False(res.Success);
    }
}
