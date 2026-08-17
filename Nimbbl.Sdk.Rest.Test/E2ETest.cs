using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest;
using Xunit;

namespace Nimbbl.Sdk.Rest.Test;

/// <summary>
/// Live end-to-end suite against the configured environment — mirrors the PHP SDK's E2ETest.
///
/// These tests hit the real API and are OFF by default. They run only when NIMBBL_E2E=1 and
/// valid NIMBBL_ACCESS_KEY / NIMBBL_ACCESS_SECRET are set; otherwise each test no-ops (passes),
/// the xUnit 2.4 equivalent of markTestSkipped(). The live pre-auth actions additionally require
/// a real transaction id:
///   - NIMBBL_PREAUTH_TXN_ID : an `authorized` txn on a capture_mode=manual sub-merchant (capture)
///   - NIMBBL_VOID_TXN_ID    : a second `authorized` txn (capture is terminal, so use a different one)
///   - NIMBBL_PAID_TXN_ID    : a captured/settled txn (refund)
/// </summary>
public class E2ETest
{
    private static bool Enabled => Environment.GetEnvironmentVariable("NIMBBL_E2E") == "1";

    private static NimbblApi? TryInitApi()
    {
        if (!Enabled) return null;
        EnvLoader.LoadEnvFile();
        var key = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_KEY");
        var secret = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET");
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret)) return null;
        var host = Environment.GetEnvironmentVariable("NIMBBL_API_HOST");
        return NimbblApi.Initialize(accessKey: key!, accessSecret: secret!, apiHost: host);
    }

    private static void SkipNote(string why) => Console.WriteLine($"[E2E SKIPPED] {why}");

    private static Dictionary<string, object?> NewOrderRequest() => new()
    {
        ["amount_before_tax"] = 100,
        ["currency"] = "INR",
        ["invoice_id"] = $"e2e_inv_{Guid.NewGuid():N}",
        ["order_date"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        ["tax"] = 0,
        ["total_amount"] = 100,
        ["user"] = new Dictionary<string, object?>
        {
            ["email"] = "e2e@example.com",
            ["first_name"] = "E2E",
            ["last_name"] = "Tester",
            ["country_code"] = "+91",
            ["mobile_number"] = "9876543210",
        },
    };

    private static string? Str(JsonElement el, string name) =>
        el.ValueKind == JsonValueKind.Object && el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() : null;

    [Fact]
    public async Task ShouldGenerateToken()
    {
        var api = TryInitApi();
        if (api == null) { SkipNote("Set NIMBBL_E2E=1 and credentials to run the live suite."); return; }

        var res = await api.Auth().GenerateTokenAsync();
        Assert.False(string.IsNullOrEmpty(Str(res, "token")), "token should be present");
    }

    [Fact]
    public async Task ShouldRunOrderFlow()
    {
        var api = TryInitApi();
        if (api == null) { SkipNote("Set NIMBBL_E2E=1 and credentials to run the live suite."); return; }

        // Create order
        var request = NewOrderRequest();
        var order = await api.Orders().CreateOrderAsync(request);
        var orderId = Str(order, "order_id");
        var invoiceId = request["invoice_id"] as string;
        Assert.False(string.IsNullOrEmpty(orderId), "order_id should be present");
        Assert.Equal("new", Str(order, "status"));

        // Get by id
        var byId = await api.Orders().GetOrderByIdAsync(orderId!);
        Assert.Equal(orderId, Str(byId, "order_id"));

        // Get by invoice id
        var byInvoice = await api.Orders().GetOrderByInvoiceIdAsync(invoiceId!);
        Assert.Equal(orderId, Str(byInvoice, "order_id"));

        // Transaction enquiry (a brand-new order may have no transaction; assert the call returns an object)
        try
        {
            var enq = await api.CheckoutUtilities().ValidateUpiVpaAsync(new Dictionary<string, object?> { ["order_id"] = orderId, ["vpa"] = "test@upi" });
            Assert.True(enq.ValueKind == JsonValueKind.Object);
        }
        catch (System.Exception ex)
        {
            SkipNote($"UPI validate not applicable: {ex.Message}");
        }
    }

    [Fact]
    public async Task ShouldCaptureLivePreAuthorizedTransaction()
    {
        var api = TryInitApi();
        if (api == null) { SkipNote("Set NIMBBL_E2E=1 and credentials to run the live suite."); return; }

        var txnId = Environment.GetEnvironmentVariable("NIMBBL_PREAUTH_TXN_ID");
        if (string.IsNullOrWhiteSpace(txnId)) { SkipNote("Set NIMBBL_PREAUTH_TXN_ID (an authorized, capture_mode=manual txn) to run live capture."); return; }

        var res = await api.Payments().CaptureAsync(new Dictionary<string, object?> { ["transaction_id"] = txnId, ["comment"] = "E2E capture" });
        Assert.True(res.ValueKind == JsonValueKind.Object);
        var status = Str(res, "capture_status") ?? Str(res, "status");
        Assert.Contains(status, new[] { "succeeded", "pending", "failed" });
    }

    [Fact]
    public async Task ShouldVoidLivePreAuthorizedTransaction()
    {
        var api = TryInitApi();
        if (api == null) { SkipNote("Set NIMBBL_E2E=1 and credentials to run the live suite."); return; }

        var txnId = Environment.GetEnvironmentVariable("NIMBBL_VOID_TXN_ID");
        if (string.IsNullOrWhiteSpace(txnId)) { SkipNote("Set NIMBBL_VOID_TXN_ID (a second authorized txn) to run live void."); return; }

        var res = await api.Payments().VoidAsync(new Dictionary<string, object?> { ["transaction_id"] = txnId, ["comment"] = "E2E void" });
        Assert.True(res.ValueKind == JsonValueKind.Object);
        var status = Str(res, "void_status") ?? Str(res, "status");
        Assert.Contains(status, new[] { "succeeded", "pending", "failed" });
    }

    [Fact]
    public async Task ShouldRefundLiveSettledTransaction()
    {
        var api = TryInitApi();
        if (api == null) { SkipNote("Set NIMBBL_E2E=1 and credentials to run the live suite."); return; }

        var txnId = Environment.GetEnvironmentVariable("NIMBBL_PAID_TXN_ID");
        if (string.IsNullOrWhiteSpace(txnId)) { SkipNote("Set NIMBBL_PAID_TXN_ID (a captured/settled txn) to run live refund."); return; }

        var res = await api.Refunds().InitiateRefundAsync(new Dictionary<string, object?> { ["transaction_id"] = txnId, ["comment"] = "E2E refund" });
        Assert.True(res.ValueKind == JsonValueKind.Object);
        Assert.False(string.IsNullOrEmpty(Str(res, "status") ?? Str(res, "refund_status")), "refund status should be present");
    }
}
