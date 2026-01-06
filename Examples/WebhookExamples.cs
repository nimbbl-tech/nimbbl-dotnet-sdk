namespace Examples;

/// <summary>
/// Webhook Examples
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
        Helpers.PrintInfo("1. Deploy webhook handler to your web server (HTTPS required)\n");
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
        Helpers.PrintInfo("\nFor implementation details, check the webhook examples.\n");
    }
}
