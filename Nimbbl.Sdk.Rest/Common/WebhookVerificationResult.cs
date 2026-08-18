using System.Text.Json;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Result of a version-aware webhook or callback verification
/// (see <see cref="SignatureVerifier.VerifyWebhook"/> and <see cref="SignatureVerifier.VerifyCallback"/>).
///
/// Mirrors the PHP SDK's verification result shape:
/// { success, message, error, version, event_type, payload }.
/// </summary>
public sealed class WebhookVerificationResult
{
    /// <summary>Whether verification succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Human-readable outcome message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional error detail when <see cref="Success"/> is false.</summary>
    public string? Error { get; init; }

    /// <summary>
    /// Payload version as reported by the top-level <c>version</c> field
    /// ("v4", "legacy", or the raw version string).
    /// </summary>
    public string? Version { get; init; }

    /// <summary>The verified payload's <c>event_type</c>, when available.</summary>
    public string? EventType { get; init; }

    /// <summary>
    /// The verified (and, when encrypted, decrypted) event payload, when available.
    /// This is the inner event object — the checkout/envelope wrapper is unwrapped.
    /// </summary>
    public JsonElement? Payload { get; init; }
}
