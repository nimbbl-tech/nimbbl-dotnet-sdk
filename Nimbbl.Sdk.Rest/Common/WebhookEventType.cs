namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Webhook event types for Nimbbl webhooks and callbacks.
/// </summary>
public enum WebhookEventType
{
    /// <summary>
    /// Payment success event (default)
    /// </summary>
    PaymentSuccess,

    /// <summary>
    /// Payment failure event
    /// </summary>
    PaymentFailure,

    /// <summary>
    /// Payment reversing event
    /// </summary>
    PaymentReversing,

    /// <summary>
    /// Payment reversal failed event
    /// </summary>
    PaymentReversalFailed,

    /// <summary>
    /// Payment reversed event
    /// </summary>
    PaymentReversed,

    /// <summary>
    /// Payment authorized event (pre-auth): funds held, awaiting capture or void
    /// </summary>
    PaymentAuthorized,

    /// <summary>
    /// Capture pending event (pre-auth)
    /// </summary>
    CapturePending,

    /// <summary>
    /// Capture success event (pre-auth): held funds collected
    /// </summary>
    CaptureSuccess,

    /// <summary>
    /// Capture failed event (pre-auth)
    /// </summary>
    CaptureFailed,

    /// <summary>
    /// Void pending event (pre-auth)
    /// </summary>
    VoidPending,

    /// <summary>
    /// Void success event (pre-auth): hold released without charging
    /// </summary>
    VoidSuccess,

    /// <summary>
    /// Void failed event (pre-auth)
    /// </summary>
    VoidFailed,

    /// <summary>
    /// Refund pending event
    /// </summary>
    RefundPending,

    /// <summary>
    /// Refund success event
    /// </summary>
    RefundSuccess,

    /// <summary>
    /// Refund failure event
    /// </summary>
    RefundFailure,

    /// <summary>
    /// Payment link paid event
    /// </summary>
    PaymentLinkPaid,

    /// <summary>
    /// Payment link expired event
    /// </summary>
    PaymentLinkExpired,

    /// <summary>
    /// Payment link cancelled event
    /// </summary>
    PaymentLinkCancelled,

    /// <summary>
    /// Payment link sent event
    /// </summary>
    PaymentLinkSent,

    /// <summary>
    /// Payment link opened event
    /// </summary>
    PaymentLinkOpened,

    /// <summary>
    /// Payment link created event
    /// </summary>
    PaymentLinkCreated
}

/// <summary>
/// Extension methods for WebhookEventType enum.
/// </summary>
public static class WebhookEventTypeExtensions
{
    /// <summary>
    /// Converts a string event type to WebhookEventType enum.
    /// </summary>
    /// <param name="eventType">Event type string from webhook payload</param>
    /// <returns>WebhookEventType enum value, defaults to PaymentSuccess if not recognized</returns>
    public static WebhookEventType ParseEventType(string? eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return WebhookEventType.PaymentSuccess;

        return eventType.ToLowerInvariant() switch
        {
            "payment_success" => WebhookEventType.PaymentSuccess,
            "payment_failed" => WebhookEventType.PaymentFailure,
            "payment_reversing" => WebhookEventType.PaymentReversing,
            "payment_reversal_failed" => WebhookEventType.PaymentReversalFailed,
            "payment_reversed" => WebhookEventType.PaymentReversed,
            "payment_authorized" => WebhookEventType.PaymentAuthorized,
            "capture_pending" => WebhookEventType.CapturePending,
            "capture_success" => WebhookEventType.CaptureSuccess,
            "capture_failed" => WebhookEventType.CaptureFailed,
            "void_pending" => WebhookEventType.VoidPending,
            "void_success" => WebhookEventType.VoidSuccess,
            "void_failed" => WebhookEventType.VoidFailed,
            "refund_pending" => WebhookEventType.RefundPending,
            "refund_success" => WebhookEventType.RefundSuccess,
            "refund_failed" => WebhookEventType.RefundFailure,
            "payment_link_paid" => WebhookEventType.PaymentLinkPaid,
            "payment_link_expired" => WebhookEventType.PaymentLinkExpired,
            "payment_link_cancelled" => WebhookEventType.PaymentLinkCancelled,
            "payment_link_sent" => WebhookEventType.PaymentLinkSent,
            "payment_link_opened" => WebhookEventType.PaymentLinkOpened,
            "payment_link_created" => WebhookEventType.PaymentLinkCreated,
            _ => WebhookEventType.PaymentSuccess // Default to payment
        };
    }

    /// <summary>
    /// Determines if the event type is a payment-related event.
    /// </summary>
    public static bool IsPaymentEvent(this WebhookEventType eventType)
    {
        return eventType switch
        {
            WebhookEventType.PaymentSuccess => true,
            WebhookEventType.PaymentFailure => true,
            WebhookEventType.PaymentReversing => true,
            WebhookEventType.PaymentReversalFailed => true,
            WebhookEventType.PaymentReversed => true,
            WebhookEventType.PaymentAuthorized => true,
            WebhookEventType.CapturePending => true,
            WebhookEventType.CaptureSuccess => true,
            WebhookEventType.CaptureFailed => true,
            WebhookEventType.VoidPending => true,
            WebhookEventType.VoidSuccess => true,
            WebhookEventType.VoidFailed => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if the event type is a refund-related event.
    /// </summary>
    public static bool IsRefundEvent(this WebhookEventType eventType)
    {
        return eventType switch
        {
            WebhookEventType.RefundPending => true,
            WebhookEventType.RefundSuccess => true,
            WebhookEventType.RefundFailure => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if the event type is a payment link-related event.
    /// </summary>
    public static bool IsPaymentLinkEvent(this WebhookEventType eventType)
    {
        return eventType switch
        {
            WebhookEventType.PaymentLinkPaid => true,
            WebhookEventType.PaymentLinkExpired => true,
            WebhookEventType.PaymentLinkCancelled => true,
            WebhookEventType.PaymentLinkSent => true,
            WebhookEventType.PaymentLinkOpened => true,
            WebhookEventType.PaymentLinkCreated => true,
            _ => false
        };
    }
}
