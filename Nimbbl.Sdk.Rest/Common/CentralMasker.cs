using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Utility to mask sensitive data in headers and JSON bodies for logging.
/// </summary>
internal static class CentralMasker
{
    private static readonly HashSet<string> SensitiveHeaderKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "authorization",
    };

    // Mask sensitive financial/authentication data in INFO logs
    // These are only visible unmasked when DEBUG logging is enabled
    private static readonly HashSet<string> SensitiveBodyKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        // Authentication credentials (masked in INFO, visible in DEBUG)
        "access_key", "access_secret",
        // Authentication tokens
        "token", "refresh_token",
        // Payment card sensitive data (used in card_details and payment requests)
        "card_no", "cvv", "cvc", "expiry", "card_holder_name",
        // UPI/VPA (sensitive payment identifiers)
        "upi_id"
    };

    public static Dictionary<string, string> MaskHeaders(HttpHeaders headers, HttpHeaders? contentHeaders = null)
    {
        var masked = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void AddMaskedHeader(KeyValuePair<string, IEnumerable<string>> header)
        {
            var value = string.Join(", ", header.Value);
            if (SensitiveHeaderKeys.Contains(header.Key))
            {
                masked[header.Key] = MaskString(value);
            }
            else
            {
                masked[header.Key] = value;
            }
        }

        foreach (var header in headers) AddMaskedHeader(header);
        if (contentHeaders != null)
        {
            foreach (var header in contentHeaders) AddMaskedHeader(header);
        }

        return masked;
    }

    /// <summary>
    /// Get headers without masking (for debug logging)
    /// </summary>
    public static Dictionary<string, string> GetUnmaskedHeaders(HttpHeaders headers, HttpHeaders? contentHeaders = null)
    {
        var unmasked = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        void AddHeader(HttpHeaders headerCollection)
        {
            foreach (var header in headerCollection)
            {
                unmasked[header.Key] = string.Join(", ", header.Value);
            }
        }

        AddHeader(headers);
        if (contentHeaders != null)
        {
            AddHeader(contentHeaders);
        }

        return unmasked;
    }

    public static string MaskBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return body;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var masked = MaskElement(doc.RootElement);
            return JsonSerializer.Serialize(masked, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            });
        }
        catch
        {
            // If not JSON, best-effort mask tokens in plain text
            return MaskPlainText(body);
        }
    }

    private static object MaskElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                prop => prop.Name,
                prop =>
                {
                    if (SensitiveBodyKeys.Contains(prop.Name))
                    {
                        if (prop.Value.ValueKind == JsonValueKind.String)
                        {
                            var value = prop.Value.GetString() ?? string.Empty;
                            // Use appropriate masking based on key type
                            if (prop.Name.Contains("secret", StringComparison.OrdinalIgnoreCase))
                                return (object)MaskSecret(value);
                            if (prop.Name.Contains("token", StringComparison.OrdinalIgnoreCase))
                                return (object)MaskToken(value);
                            return (object)MaskString(value);
                        }
                        return (object)"***";
                    }
                    return MaskElement(prop.Value);
                }),
            JsonValueKind.Array => element.EnumerateArray().Select(MaskElement).ToList(),
            JsonValueKind.String => element.GetString() ?? string.Empty, // Don't mask all strings, only sensitive keys
            _ => element.Clone()
        };
    }

    private static string MaskString(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= 8) return "***";
        // Show first 4 and last 4 characters, mask the middle
        return $"{value.Substring(0, 4)}***{value.Substring(value.Length - 4)}";
    }
    
    // Mask token with more characters visible (for longer tokens)
    private static string MaskToken(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= 12) return new string('*', value.Length);
        // Show first 5 and last 7 characters for tokens
        return $"{value.Substring(0, 5)}***********{value.Substring(value.Length - 7)}";
    }
    
    // Mask secret with more characters visible (for longer secrets)
    private static string MaskSecret(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= 21) return new string('*', value.Length);
        // Show first 17 and last 4 characters for secrets
        return $"{value.Substring(0, 17)}***********{value.Substring(value.Length - 4)}";
    }

    // Best-effort masking in non-JSON text (URLs, query strings, card-like patterns)
    private static string MaskPlainText(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var masked = value;
        var sensitiveParams = new[] { "token", "refresh_token" };
        foreach (var param in sensitiveParams)
        {
            masked = Regex.Replace(
                masked,
                $"([?&]{param}=)([^&\\s\"]+)",
                m => $"{m.Groups[1].Value}{MaskString(m.Groups[2].Value)}",
                RegexOptions.IgnoreCase);
        }

        // Mask card-like digit sequences (13-19 digits, allowing space/dash)
        masked = Regex.Replace(
            masked,
            @"(\d[\s-]?){13,19}",
            m =>
            {
                var digits = Regex.Replace(m.Value, @"[\s-]", "");
                return digits.Length is >= 13 and <= 19 ? MaskString(digits) : m.Value;
            });

        return masked;
    }
}

