namespace Nimbbl.Sdk.Rest.Log;

/// <summary>
/// Optional structured context attached to a log line. Mirrors the PHP SDK's logger
/// context fields, rendered as a prefix on the message in this order:
/// [APIVersion:..] [APITag:..] [URI:..] [StatusCode:..] [SubMerchantID:..] [OrderID:..]
/// [InvoiceID:..] [TransactionID:..] [EventType:..]
///
/// Only non-empty fields are rendered. Pass an instance to any of the logger's
/// <c>*WithCaller</c> methods to make webhook/callback/API log lines traceable.
/// </summary>
public sealed class LogContext
{
    public string? SubMerchantId { get; init; }
    public string? OrderId { get; init; }
    public string? TransactionId { get; init; }
    public string? ApiVersion { get; init; }
    public string? ApiTag { get; init; }
    public string? Uri { get; init; }
    public string? StatusCode { get; init; }
    public string? EventType { get; init; }
    public string? InvoiceId { get; init; }
}
