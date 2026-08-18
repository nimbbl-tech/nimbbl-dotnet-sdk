using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;

namespace Examples;

/// <summary>
/// Webhook &amp; Callback Examples.
///
/// <para>
/// <see cref="SignatureVerifier.VerifyWebhook"/> and <see cref="SignatureVerifier.VerifyCallback"/>
/// are version-aware: they read the payload's top-level <c>version</c> field and pick the correct
/// handling automatically —
/// </para>
/// <list type="bullet">
///   <item><description>version == "v4" -> signed envelope (HMAC over the whole compact JSON; encrypted =&gt; decryption authenticates)</description></item>
///   <item><description>version absent / v1 / v2 / v3 -> legacy per-field signature handling</description></item>
/// </list>
/// <para>This keeps existing (pre-v4) merchants working unchanged while supporting the new format.</para>
/// </summary>
public static class WebhookExamples
{
    /// <summary>
    /// Display Webhook Info
    /// </summary>
    public static void DisplayWebhookInfo()
    {
        Helpers.PrintHeader("=== Webhook Handling ===");

        Helpers.PrintInfo("Webhook handling is designed for HTTP requests, not CLI.\n");
        Helpers.PrintInfo("To set up webhooks:\n");
        Helpers.PrintInfo("1. Deploy a webhook handler to your web server (HTTPS required)\n");
        Helpers.PrintInfo("2. Ensure the URL accepts POST requests and returns 200 within 15 seconds\n");
        Helpers.PrintInfo("3. Configure webhook URL in Nimbbl Dashboard or contact support@nimbbl.tech\n");
        Helpers.PrintInfo("4. Webhooks will be sent to your configured URL\n");
        Helpers.PrintInfo("\nImportant:\n");
        Helpers.PrintInfo("- URL must be HTTPS and publicly accessible\n");
        Helpers.PrintInfo("- Must return 200 response within 15 seconds\n");
        Helpers.PrintInfo("- Handle idempotency (same webhook may be received multiple times)\n");
        Helpers.PrintInfo("- Webhook order is not guaranteed\n");
        Helpers.PrintInfo("\nSupported Events:\n");
        Helpers.PrintInfo("- payment_success, payment_failed, payment_reversing\n");
        Helpers.PrintInfo("- payment_reversal_failed, payment_reversed\n");
        Helpers.PrintInfo("- refund_success, refund_failed, refund_pending\n");
        Helpers.PrintInfo("- payment_authorized (pre-auth)\n");
        Helpers.PrintInfo("- capture_pending, capture_success, capture_failed (pre-auth)\n");
        Helpers.PrintInfo("- void_pending, void_success, void_failed (pre-auth)\n");
        Helpers.PrintInfo("\nVerification (call from your HTTP handler): WebhookExamples.HandleWebhook(rawBody, accessSecret)\n");
    }

    /// <summary>
    /// Verify and route a raw webhook body. Call this from your HTTP endpoint with the raw request
    /// body and your access_secret. Returns true when verification succeeds (respond 200), false
    /// otherwise (respond 401). Verification transparently handles v4 (signed/encrypted) and legacy payloads.
    /// </summary>
    public static bool HandleWebhook(string rawBody, string accessSecret)
    {
        // One step: verify + parse. The SDK chooses v4-envelope vs legacy handling from `version`.
        var result = SignatureVerifier.VerifyWebhook(rawBody, accessSecret);

        if (!result.Success)
        {
            Helpers.PrintError($"Webhook verification failed: {result.Message}\n");
            return false; // respond 401 from your endpoint
        }

        Helpers.PrintSuccess($"Webhook verified (version={result.Version}, event_type={result.EventType}).\n");

        // result.Payload is the verified (and, when encrypted, decrypted) event object.
        var eventType = result.EventType ?? "unknown";
        switch (eventType)
        {
            case "payment_success":
                Helpers.PrintInfo("Payment succeeded — fulfil the order.\n");
                break;
            case "payment_failed":
                Helpers.PrintInfo("Payment failed — do not fulfil.\n");
                break;
            case "payment_authorized":
                Helpers.PrintInfo("Pre-auth authorized — funds held. Capture to collect, or void to release.\n");
                break;
            case "capture_success":
                Helpers.PrintInfo("Capture succeeded — held funds collected; safe to fulfil.\n");
                break;
            case "void_success":
                Helpers.PrintInfo("Void succeeded — hold released without charging.\n");
                break;
            case "refund_success":
                Helpers.PrintInfo("Refund succeeded.\n");
                break;
            default:
                Helpers.PrintInfo($"Received event '{eventType}'.\n");
                break;
        }

        if (result.Payload.HasValue)
        {
            Console.WriteLine(JsonSerializer.Serialize(result.Payload.Value, new JsonSerializerOptions { WriteIndented = true }));
        }
        return true; // respond 200 from your endpoint
    }

    /// <summary>
    /// Verify and route a raw payment/checkout callback body. Redirect callbacks arrive Base64-encoded
    /// and POST/popup callbacks arrive as raw JSON — <see cref="SignatureVerifier.VerifyCallback"/>
    /// handles both, unwraps the checkout envelope, and verifies the v4 <c>nimbbl_signature</c> (or
    /// legacy per-field signature). Returns true when verification succeeds.
    /// </summary>
    public static bool HandleCallback(string rawBody, string accessSecret)
    {
        var result = SignatureVerifier.VerifyCallback(rawBody, accessSecret);

        if (!result.Success)
        {
            Helpers.PrintError($"Callback verification failed: {result.Message}\n");
            return false;
        }

        Helpers.PrintSuccess($"Callback verified (version={result.Version}, event_type={result.EventType}).\n");
        if (result.Payload.HasValue)
        {
            Console.WriteLine(JsonSerializer.Serialize(result.Payload.Value, new JsonSerializerOptions { WriteIndented = true }));
        }
        return true;
    }
}
