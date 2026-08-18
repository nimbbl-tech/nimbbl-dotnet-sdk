using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nimbbl.Sdk.Rest.RestClient;
using Xunit;
using PaymentsService = Nimbbl.Sdk.Rest.Payments.Payments;

namespace Nimbbl.Sdk.Rest.Test;

/// <summary>
/// Offline (mocked-transport) test for Payments.InitiatePaymentAsync — no live API/credentials needed.
/// A stub HttpMessageHandler answers the auto token-generation call and the initiate-payment call.
/// </summary>
public class PaymentInitiateMockTest
{
    /// <summary>Minimal HttpMessageHandler that returns canned responses and records the requests it saw.</summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string, HttpResponseMessage> _responder;
        public List<(string Url, string? Body)> Seen { get; } = new();

        public StubHandler(Func<HttpRequestMessage, string, HttpResponseMessage> responder) => _responder = responder;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content != null ? await request.Content.ReadAsStringAsync() : string.Empty;
            var url = request.RequestUri!.ToString();
            Seen.Add((url, body));
            return _responder(request, body);
        }
    }

    private static HttpResponseMessage Json(string body, HttpStatusCode code = HttpStatusCode.OK) =>
        new(code) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task ShouldInitiatePaymentWithMockedApi()
    {
        var handler = new StubHandler((req, _) =>
        {
            var url = req.RequestUri!.ToString();
            if (url.Contains("generate-token"))
                return Json("{\"token\":\"tok_mock_123\",\"expires_at\":\"2099-01-01 00:00:00\",\"valid\":true}");
            if (url.Contains("initiate-payment"))
                return Json("{\"transaction_id\":\"txn_mock_1\",\"status\":\"pending\",\"payment_flow\":\"otp\"}");
            return Json("{\"error\":{\"nimbbl_error_code\":\"NOT_FOUND\"}}", HttpStatusCode.NotFound);
        });

        var apiClient = new ApiClient("access_key_test", "access_secret_test", "https://mock.local/api/", encryptPayload: false, handler: handler);
        var payments = new PaymentsService(apiClient);

        var result = await payments.InitiatePaymentAsync(new Dictionary<string, object?>
        {
            ["order_id"] = "o_mock_1",
            ["payment_mode_code"] = "net_banking",
            ["bank_code"] = "hdfc",
        });

        // Response parsed from the mocked initiate-payment body
        Assert.Equal("txn_mock_1", result.GetProperty("transaction_id").GetString());
        Assert.Equal("pending", result.GetProperty("status").GetString());

        // The SDK auto-generated a merchant token first, then called initiate-payment
        Assert.Contains(handler.Seen, s => s.Url.Contains("generate-token"));
        var initiate = handler.Seen.Last();
        Assert.Contains("initiate-payment", initiate.Url);
        Assert.Contains("\"order_id\":\"o_mock_1\"", initiate.Body);
        Assert.Contains("\"bank_code\":\"hdfc\"", initiate.Body);

        // At least 2 calls were made (token + initiate)
        Assert.True(handler.Seen.Count >= 2);
    }

    [Fact]
    public async Task ShouldSurfaceApiErrorFromMockedInitiatePayment()
    {
        var handler = new StubHandler((req, _) =>
        {
            var url = req.RequestUri!.ToString();
            if (url.Contains("generate-token"))
                return Json("{\"token\":\"tok_mock_123\",\"expires_at\":\"2099-01-01 00:00:00\",\"valid\":true}");
            // initiate-payment fails with a 400 error envelope
            return Json("{\"error\":{\"nimbbl_error_code\":\"INVALID_ORDER\",\"nimbbl_merchant_message\":\"Invalid order id\"}}", HttpStatusCode.BadRequest);
        });

        var apiClient = new ApiClient("access_key_test", "access_secret_test", "https://mock.local/api/", encryptPayload: false, handler: handler);
        var payments = new PaymentsService(apiClient);

        var ex = await Assert.ThrowsAnyAsync<Nimbbl.Sdk.Rest.Exception.NimbblException>(() =>
            payments.InitiatePaymentAsync(new Dictionary<string, object?> { ["order_id"] = "bad" }));

        Assert.Contains("Invalid order id", ex.Message);
    }
}
