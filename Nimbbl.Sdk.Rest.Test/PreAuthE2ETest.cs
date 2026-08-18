using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Xunit;

namespace Nimbbl.Sdk.Rest.Test;

/// <summary>
/// End-to-end pre-authorization (capture / void) webhook tests — mirrors the PHP SDK's PreAuthE2ETest.
///
/// These are OFFLINE and credential-free: they build realistic v4 signed webhook envelopes
/// (with pre-auth authorization_details and capture/void transaction shapes) exactly as the
/// backend would emit them, then assert the SDK verifies and decodes the outcome.
/// </summary>
public class PreAuthE2ETest
{
    private const string Secret = "test_secret_key_12345";

    private static string Json(object o) => JsonSerializer.Serialize(o);

    // Pre-auth authorization_details block carried on an authorized transaction.
    private static Dictionary<string, object?> AuthDetails(double captured = 0.0, double voided = 0.0, double available = 500.00) => new()
    {
        ["mechanism"] = "pre_auth",
        ["capture_mode"] = "manual",
        ["authorized_time"] = "2026-06-22T10:46:14Z",
        ["expiry_time"] = "2026-06-29T10:46:14Z",
        ["captured_amount"] = captured,
        ["voided_amount"] = voided,
        ["available_authorized_amount"] = available,
    };

    private static Dictionary<string, object?> Txn(Dictionary<string, object?> overrides)
    {
        var txn = new Dictionary<string, object?>
        {
            ["transaction_id"] = "o_Rz4Zx2WeyooEpyxa-221117104614",
            ["status"] = "authorized",
            ["transaction_amount"] = 100.00,
            ["transaction_currency"] = "INR",
        };
        foreach (var kv in overrides) txn[kv.Key] = kv.Value;
        return txn;
    }

    private static Dictionary<string, object?> InnerPayload(string eventType, Dictionary<string, object?> txn) => new()
    {
        ["event_type"] = eventType,
        ["order"] = new Dictionary<string, object?> { ["invoice_id"] = "inv_preauth", ["order_id"] = "o_preauth" },
        ["transaction"] = txn,
    };

    // Wrap an inner event in a v4 signed webhook envelope { version, payload(base64), signature }.
    private static string WrapV4(Dictionary<string, object?> inner, string sigField = "signature")
    {
        var innerJson = Json(inner);
        return Json(new Dictionary<string, object?>
        {
            ["version"] = "v4",
            ["payload"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(innerJson)),
            [sigField] = Hmac(innerJson, Secret),
            ["sub_merchant_id"] = "sm_1",
        });
    }

    private static string Hmac(string data, string secret)
    {
        using var h = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLowerInvariant();
    }

    [Fact]
    public void ShouldVerifyPaymentAuthorizedV4()
    {
        var inner = InnerPayload("payment_authorized",
            Txn(new Dictionary<string, object?> { ["status"] = "authorized", ["authorization_details"] = AuthDetails() }));

        var result = SignatureVerifier.VerifyWebhook(WrapV4(inner), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("v4", result.Version);
        Assert.Equal("payment_authorized", result.EventType);

        var ad = result.Payload!.Value.GetProperty("transaction").GetProperty("authorization_details");
        Assert.Equal("pre_auth", ad.GetProperty("mechanism").GetString());
        Assert.Equal("manual", ad.GetProperty("capture_mode").GetString());
        Assert.Equal(500.00, ad.GetProperty("available_authorized_amount").GetDouble());
        Assert.True(ad.TryGetProperty("expiry_time", out _));
    }

    [Fact]
    public void ShouldVerifyCaptureSuccessV4()
    {
        var inner = InnerPayload("capture_success", Txn(new Dictionary<string, object?>
        {
            ["status"] = "succeeded",
            ["transaction_type"] = "capture",
            ["capture_type"] = "full",
            ["original_payment_transaction_id"] = "o_Rz4Zx2WeyooEpyxa-221117104614",
        }));

        var result = SignatureVerifier.VerifyWebhook(WrapV4(inner), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("capture_success", result.EventType);

        var txn = result.Payload!.Value.GetProperty("transaction");
        Assert.Equal("capture", txn.GetProperty("transaction_type").GetString());
        Assert.Equal("full", txn.GetProperty("capture_type").GetString());
        Assert.Equal("o_Rz4Zx2WeyooEpyxa-221117104614", txn.GetProperty("original_payment_transaction_id").GetString());
    }

    [Fact]
    public void ShouldVerifyVoidSuccessV4()
    {
        var inner = InnerPayload("void_success", Txn(new Dictionary<string, object?>
        {
            ["status"] = "succeeded",
            ["transaction_type"] = "void",
            ["reversal_reason"] = "authorization_voided",
        }));

        var result = SignatureVerifier.VerifyWebhook(WrapV4(inner), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("void_success", result.EventType);
        Assert.Equal("authorization_voided", result.Payload!.Value.GetProperty("transaction").GetProperty("reversal_reason").GetString());
    }

    [Fact]
    public void ShouldVerifyVoidExpiredV4NotInitiatedByMerchant()
    {
        // A void_success the merchant did NOT initiate — hold released at expiry.
        var inner = InnerPayload("void_success", Txn(new Dictionary<string, object?>
        {
            ["status"] = "succeeded",
            ["transaction_type"] = "void",
            ["reversal_reason"] = "authorization_expired",
        }));

        var result = SignatureVerifier.VerifyWebhook(WrapV4(inner), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("authorization_expired", result.Payload!.Value.GetProperty("transaction").GetProperty("reversal_reason").GetString());
    }

    [Fact]
    public void ShouldVerifyPreAuthCallbackV4()
    {
        // Client checkout callback for a pre-auth: signed under nimbbl_signature.
        var inner = InnerPayload("payment_authorized",
            Txn(new Dictionary<string, object?> { ["status"] = "authorized", ["authorization_details"] = AuthDetails() }));

        var result = SignatureVerifier.VerifyCallback(WrapV4(inner, "nimbbl_signature"), Secret);

        Assert.True(result.Success, result.Message);
        Assert.Equal("v4", result.Version);
        Assert.Equal("payment_authorized", result.EventType);
    }
}
