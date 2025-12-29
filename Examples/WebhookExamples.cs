using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;

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
        
        // Demonstrate with example payload
        var examplePayload = GetExampleWebhookPayload();
        
        // Read secret key from environment variables (loaded from .env by SDK or set manually)
        var secretKey = Environment.GetEnvironmentVariable("NIMBBL_ACCESS_SECRET") 
            ?? throw new InvalidOperationException(ErrorMessages.AccessSecretRequiredEnvWithHint);
        
        Console.WriteLine("\nDemonstrating webhook verification with example payload:\n");
        VerifyWebhookSignatureExample(examplePayload, secretKey);
    }

    /// <summary>
    /// Verify Webhook Signature Example
    /// </summary>
    public static void VerifyWebhookSignatureExample(string webhookPayload, string secretKey)
    {
        Helpers.PrintHeader("=== Webhook Signature Verification Example ===");
        
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
        
        if (string.IsNullOrWhiteSpace(webhookPayload))
        {
            Helpers.PrintError("Webhook payload is empty\n");
            return;
        }
        
        try
        {
            // Parse webhook payload
            using var jsonDoc = JsonDocument.Parse(webhookPayload);
            var webhookData = jsonDoc.RootElement;
            
            // Verify and parse webhook using Util class (v3 format - signature in payload)
            var result = Util.VerifyAndParseWebhook(webhookPayload, secretKey, out var parsed);
            
            if (!result.Success)
            {
                Helpers.PrintError($"Webhook verification failed: {result.Message}\n");
                return;
            }
            
            // Get event type from payload
            var eventType = parsed.TryGetProperty("event_type", out var etProp) ? etProp.GetString() : null;
            
            // Extract IDs from payload
            var nimbblOrderId = GetOrderId(parsed);
            var nimbblTransactionId = GetTransactionId(parsed);
            
            Helpers.PrintInfo($"Webhook event_type: {eventType ?? "N/A"}\n");
            Helpers.PrintInfo($"Nimbbl Order ID: {nimbblOrderId ?? "N/A"}\n");
            Helpers.PrintInfo($"Nimbbl Transaction ID: {nimbblTransactionId ?? "N/A"}\n");
            
            // Process webhook event
            ProcessWebhookEvent(parsed, eventType);
            
            Helpers.PrintSuccess("Webhook processed successfully!\n");
        }
        catch (JsonException ex)
        {
            Helpers.PrintError($"Error parsing webhook payload: {ex.Message}\n");
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }
    
    private static void ProcessWebhookEvent(JsonElement eventData, string? eventType)
    {
        // Handle webhook events based on documentation
        switch (eventType)
        {
            case "payment_success":
                HandlePaymentSuccess(eventData);
                break;
            case "payment_failed":
                HandlePaymentFailed(eventData);
                break;
            case "payment_reversing":
                HandlePaymentReversing(eventData);
                break;
            case "payment_reversal_failed":
                HandlePaymentReversalFailed(eventData);
                break;
            case "payment_reversed":
                HandlePaymentReversed(eventData);
                break;
            case "refund_success":
                HandleRefundSuccess(eventData);
                break;
            case "refund_failed":
                HandleRefundFailed(eventData);
                break;
            case "refund_pending":
                HandleRefundPending(eventData);
                break;
            default:
                Helpers.PrintWarning($"Unknown event type: {eventType ?? "N/A"}\n");
                break;
        }
    }
    
    private static void HandlePaymentSuccess(JsonElement eventData)
    {
        Helpers.PrintInfo($"Payment successful: {GetTransactionId(eventData) ?? "N/A"}\n");
        
        // Get transaction and order data
        var transactionData = GetTransactionData(eventData);
        var orderData = GetOrderData(eventData);
        
        // Your business logic here
        // Example: Update order status to 'paid', send confirmation email, fulfill order, etc.
        
        var transactionId = GetTransactionId(eventData);
        var orderId = GetOrderId(eventData);
        
        if (!string.IsNullOrWhiteSpace(transactionId) && !string.IsNullOrWhiteSpace(orderId))
        {
            Helpers.PrintInfo($"Processing successful payment for transaction: {transactionId}, order: {orderId}\n");
        }
    }
    
    private static void HandlePaymentFailed(JsonElement eventData)
    {
        Helpers.PrintInfo($"Payment failed: {GetTransactionId(eventData) ?? "N/A"}\n");
        
        // Your business logic here
        // Example: Update order status to 'payment_failed', send notification to customer, etc.
    }
    
    private static void HandlePaymentReversing(JsonElement eventData)
    {
        Helpers.PrintInfo($"Payment reversing: {GetTransactionId(eventData) ?? "N/A"}\n");
        
        // Your business logic here
        // Example: Update transaction status to 'reversing'
    }
    
    private static void HandlePaymentReversalFailed(JsonElement eventData)
    {
        Helpers.PrintInfo($"Payment reversal failed: {GetTransactionId(eventData) ?? "N/A"}\n");
        
        // Your business logic here
        // Example: Log reversal failure, notify admin
    }
    
    private static void HandlePaymentReversed(JsonElement eventData)
    {
        Helpers.PrintInfo($"Payment reversed: {GetTransactionId(eventData) ?? "N/A"}\n");
        
        // Your business logic here
        // Example: Update order status, reverse inventory changes, update accounting records
    }
    
    private static void HandleRefundSuccess(JsonElement eventData)
    {
        Helpers.PrintInfo($"Refund successful: {GetRefundId(eventData) ?? "N/A"}\n");
        
        // Your business logic here
        // Example: Update order status, send refund confirmation, update accounting records
    }
    
    private static void HandleRefundFailed(JsonElement eventData)
    {
        Helpers.PrintInfo($"Refund failed: {GetRefundId(eventData) ?? "N/A"}\n");
        
        // Your business logic here
        // Example: Log failure, notify admin, update refund status
    }
    
    private static void HandleRefundPending(JsonElement eventData)
    {
        Helpers.PrintInfo($"Refund pending: {GetRefundId(eventData) ?? "N/A"}\n");
        
        // Your business logic here
        // Example: Update refund status to 'pending', notify customer
    }
    
    // Helper methods
    private static string? GetOrderId(JsonElement eventData)
    {
        if (eventData.TryGetProperty("nimbbl_order_id", out var prop))
            return prop.GetString();
        if (eventData.TryGetProperty("order_id", out var oidProp))
            return oidProp.GetString();
        if (eventData.TryGetProperty("order", out var orderProp) && 
            orderProp.TryGetProperty("order_id", out var oid))
            return oid.GetString();
        return null;
    }
    
    private static string? GetTransactionId(JsonElement eventData)
    {
        if (eventData.TryGetProperty("nimbbl_transaction_id", out var prop))
            return prop.GetString();
        if (eventData.TryGetProperty("transaction_id", out var tidProp))
            return tidProp.GetString();
        if (eventData.TryGetProperty("transaction", out var txnProp) && 
            txnProp.TryGetProperty("transaction_id", out var tid))
            return tid.GetString();
        return null;
    }
    
    private static string? GetRefundId(JsonElement eventData)
    {
        if (eventData.TryGetProperty("refund_id", out var prop))
            return prop.GetString();
        if (eventData.TryGetProperty("refund", out var refundProp) && 
            refundProp.TryGetProperty("refund_id", out var rid))
            return rid.GetString();
        return null;
    }
    
    private static JsonElement? GetOrderData(JsonElement eventData)
    {
        if (eventData.TryGetProperty("order", out var orderProp))
            return orderProp;
        return null;
    }
    
    private static JsonElement? GetTransactionData(JsonElement eventData)
    {
        if (eventData.TryGetProperty("transaction", out var txnProp))
            return txnProp;
        return null;
    }
    
    private static JsonElement? GetRefundData(JsonElement eventData)
    {
        if (eventData.TryGetProperty("refund", out var refundProp))
            return refundProp;
        return null;
    }
    
    /// <summary>
    /// Example webhook payload for testing
    /// </summary>
    public static string GetExampleWebhookPayload()
    {
        return """
        {
            "event_type": "payment_success",
            "nimbbl_order_id": "o_XXXXXXXXXX",
            "nimbbl_transaction_id": "t_XXXXXXXXXX",
            "order": {
                "order_id": "o_XXXXXXXXXX",
                "invoice_id": "INV-123456",
                "total_amount": 1000.00,
                "currency": "INR",
                "status": "paid"
            },
            "transaction": {
                "transaction_id": "t_XXXXXXXXXX",
                "transaction_amount": 1000.00,
                "transaction_currency": "INR",
                "status": "succeeded",
                "transaction_type": "payment",
                "signature_version": "v3",
                "nimbbl_signature": "calculated_signature_here"
            }
        }
        """;
    }
}
