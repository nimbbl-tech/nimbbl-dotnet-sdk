using System.Linq;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Utility to mask sensitive data in headers and JSON bodies for logging.
/// Behaviour mirrors the PHP SDK's CentralMasker (Nimbbl PII masking guidelines:
/// https://nimbbl.biz/docs/guides/integration/handling-pii-data/).
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
        JsonKeys.AccessKey, JsonKeys.AccessSecret,
        // Authentication tokens
        JsonKeys.Token, JsonKeys.RefreshToken,
        // Names
        JsonKeys.FirstName, JsonKeys.LastName, JsonKeys.CardHolderName, JsonKeys.UpiHolder,
        // Response-side PII field names (webhook/callback use short forms)
        JsonKeys.Name, JsonKeys.CardHolder,
        // Mobile/Phone
        JsonKeys.MobileNumber, JsonKeys.Mobile,
        // Email
        JsonKeys.Email,
        // Address fields
        JsonKeys.Street, JsonKeys.Landmark, JsonKeys.Area, JsonKeys.City, JsonKeys.State,
        // Pincode
        JsonKeys.Pincode, JsonKeys.PinCode, JsonKeys.PostalCode, JsonKeys.ZipCode,
        // UPI/VPA
        JsonKeys.Vpa,
        // Payment card sensitive data
        JsonKeys.CardNo, JsonKeys.CardNumber, JsonKeys.Cvv, JsonKeys.ExpiryDate,
        // Account details
        JsonKeys.AccountNumber, JsonKeys.AccountNo, JsonKeys.IfscCode, JsonKeys.PanCard,
    };

    // JSON output encoder: keep slashes/unicode unescaped (parity with PHP JSON_UNESCAPED_SLASHES).
    private static readonly JsonSerializerOptions MaskedBodyOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static Dictionary<string, string> MaskHeaders(HttpHeaders headers, HttpHeaders? contentHeaders = null)
    {
        var masked = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void AddMaskedHeader(KeyValuePair<string, IEnumerable<string>> header)
        {
            var value = string.Join(", ", header.Value);
            masked[header.Key] = SensitiveHeaderKeys.Contains(header.Key) ? MaskString(value) : value;
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
            return JsonSerializer.Serialize(masked, MaskedBodyOptions);
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
                            var key = prop.Name.ToLowerInvariant();

                            // Use appropriate masking based on key type (following Nimbbl PII masking guidelines)
                            if (key == JsonKeys.AccessSecret)
                                return (object)MaskAccessSecret(value);
                            if (key == JsonKeys.AccessKey)
                                return (object)MaskAccessKey(value);
                            if (key.Contains("token"))
                                return (object)MaskToken(value);
                            if (key.Contains("name") || key == JsonKeys.FirstName || key == JsonKeys.LastName
                                || key == JsonKeys.CardHolderName || key == JsonKeys.CardHolder || key == JsonKeys.UpiHolder)
                                return (object)MaskName(value);
                            if (key.Contains("phone") || key.Contains("mobile") || key.Contains("contact_number"))
                                return (object)MaskPhone(value);
                            if (key.Contains("email"))
                                return (object)MaskEmail(value);
                            if (key.Contains("address") || key == JsonKeys.Street || key == JsonKeys.Landmark || key == JsonKeys.Area)
                                return (object)MaskAddress(value);
                            if (key == JsonKeys.City || key == JsonKeys.State)
                                return (object)MaskCityArea(value);
                            if (key.Contains("pincode") || key == JsonKeys.Pincode || key == JsonKeys.PinCode || key == JsonKeys.PostalCode || key == JsonKeys.ZipCode)
                                return (object)MaskPincode(value);
                            if (key.Contains("upi") || key == JsonKeys.Vpa)
                                return (object)MaskUpiId(value);
                            if (key == JsonKeys.CardNo || key == JsonKeys.CardNumber)
                                return (object)MaskCardNumber(value);
                            if (key == JsonKeys.Cvv)
                                return (object)"***";
                            if (key.Contains("expiry") || key == JsonKeys.ExpiryDate)
                                return (object)"**/****";
                            if (key == JsonKeys.AccountNumber || key == JsonKeys.AccountNo)
                                return (object)MaskAccountNumber(value);
                            if (key.Contains("ifsc") || key == JsonKeys.IfscCode)
                                return (object)MaskIfsc(value);
                            if (key == JsonKeys.PanCard)
                                return (object)MaskPan(value);
                            return (object)MaskString(value);
                        }

                        // Numeric sensitive values are masked as "***"; nested objects/arrays still recurse.
                        if (prop.Value.ValueKind is JsonValueKind.Number)
                            return (object)"***";
                        return MaskElement(prop.Value);
                    }
                    return MaskElement(prop.Value);
                }),
            JsonValueKind.Array => element.EnumerateArray().Select(MaskElement).ToList(),
            JsonValueKind.String => element.GetString() ?? string.Empty, // Don't mask all strings, only sensitive keys
            _ => element.Clone()
        };
    }

    // --- Masking Methods (parity with PHP CentralMasker) ---

    private static string MaskString(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= 8) return "***";
        // Show first 4 and last 4 characters, mask the middle
        return $"{value[..4]}***{value[^4..]}";
    }

    // Nimbbl API format (mask_token): first 5 + 11 asterisks + last 7
    private static string MaskToken(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        value = value.Trim();
        if (value.Length <= 12) return new string('*', value.Length);
        return $"{value[..5]}***********{value[^7..]}";
    }

    // access_key has no dedicated Nimbbl API rule; show first 4 and last 4
    private static string MaskAccessKey(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= 8) return new string('*', value.Length);
        return $"{value[..4]}****{value[^4..]}";
    }

    // Nimbbl API format (mask_access_secret): first 17 + 11 asterisks + last 4
    private static string MaskAccessSecret(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        value = value.Trim();
        if (value.Length <= 21) return new string('*', value.Length);
        return $"{value[..17]}***********{value[^4..]}";
    }

    // Mask name: First letter + asterisks per word (e.g., "Diana Prince" -> "D**** P*****")
    private static string MaskName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var parts = value.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return new string('*', value.Length);

        var maskedParts = parts.Select(part =>
        {
            if (string.IsNullOrEmpty(part)) return part;
            if (part.Length <= 1) return part;
            return part[0] + new string('*', part.Length - 1);
        });

        return string.Join(" ", maskedParts);
    }

    // Mask phone: Country code + last 4 digits (e.g., "+91 9876543210" -> "+91 ******3210")
    private static string MaskPhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var cleaned = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (cleaned.StartsWith("+"))
        {
            var countryCodeEnd = 1;
            while (countryCodeEnd < cleaned.Length && char.IsDigit(cleaned[countryCodeEnd]))
            {
                countryCodeEnd++;
            }

            if (countryCodeEnd < cleaned.Length)
            {
                var countryCode = cleaned[..countryCodeEnd];
                var number = cleaned[countryCodeEnd..];

                if (number.Length >= 4)
                {
                    var last4 = number[^4..];
                    var masked = new string('*', number.Length - 4);
                    return $"{countryCode} {masked}{last4}";
                }
            }
        }

        if (cleaned.Length >= 4 && cleaned.All(char.IsDigit))
        {
            var last4 = cleaned[^4..];
            var masked = new string('*', cleaned.Length - 4);
            return $"{masked}{last4}";
        }

        return new string('*', value.Length);
    }

    // Mask email (mask_email): URL-decode, then tier by local-part length.
    private static string MaskEmail(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var processed = SafeUrlDecode(value);
        var atIndex = processed.IndexOf('@');
        if (atIndex < 0) return value;

        var local = processed[..atIndex];
        var domain = processed[atIndex..]; // includes '@'
        var len = local.Length;

        string maskedLocal;
        if (len <= 2)
        {
            maskedLocal = len > 1 ? local[0] + new string('*', len - 1) : local;
        }
        else if (len <= 4)
        {
            maskedLocal = local[0] + new string('*', len - 2) + local[^1..];
        }
        else
        {
            maskedLocal = local[..2] + new string('*', len - 4) + local[^2..];
        }

        return maskedLocal + domain;
    }

    // Mask city/state (mask_city_area): first 2 letters of each word shown, rest masked.
    private static string MaskCityArea(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var parts = value.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return new string('*', value.Length);

        var maskedParts = parts.Select(part =>
            part.Length <= 2 ? part : part[..2] + new string('*', part.Length - 2));

        return string.Join(" ", maskedParts);
    }

    // Mask address: First char + asterisks (e.g., "123 Main Street" -> "1** M*** S*****")
    private static string MaskAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var parts = value.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return new string('*', value.Length);

        var maskedParts = parts.Select(part =>
        {
            if (string.IsNullOrEmpty(part)) return part;
            if (part.Length == 1) return part;
            if (part.Length == 2) return part[0] + "*";
            return part[0] + new string('*', part.Length - 1);
        });

        return string.Join(" ", maskedParts);
    }

    // Mask pincode: First 2 digits + asterisks (e.g., "100389" -> "10****")
    private static string MaskPincode(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var cleaned = value.Trim().Replace(" ", "").Replace("-", "");
        if (cleaned.Length >= 2 && cleaned.All(char.IsDigit))
        {
            var first2 = cleaned[..2];
            var masked = new string('*', cleaned.Length - 2);
            return $"{first2}{masked}";
        }

        return new string('*', value.Length);
    }

    // Mask UPI ID (mask_vpa_id): URL-decode + validate; then tier by user length.
    private static string MaskUpiId(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var processed = SafeUrlDecode(value);
        var atIndex = processed.IndexOf('@');
        if (atIndex < 0) return value;
        if (!Regex.IsMatch(processed, "^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+$")) return value;

        var user = processed[..atIndex];
        var domain = processed[atIndex..]; // includes '@'
        var len = user.Length;

        string maskedUser;
        if (len <= 4)
        {
            maskedUser = len > 2 ? user[0] + new string('*', len - 2) + user[^1..] : user;
        }
        else
        {
            maskedUser = user[..2] + new string('*', len - 4) + user[^2..];
        }

        return maskedUser + domain;
    }

    // Mask card number (mask_card): fixed "**** **** **** " + last 4.
    private static string MaskCardNumber(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var cleaned = value.Replace(" ", "").Replace("-", "");
        var last4 = cleaned.Length >= 4 ? cleaned[^4..] : cleaned;
        return "**** **** **** " + last4;
    }

    // Mask account number: Last 4 digits visible (e.g., "123456789012" -> "********9012")
    private static string MaskAccountNumber(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var cleaned = value.Trim().Replace(" ", "").Replace("-", "");
        if (cleaned.Length >= 4 && cleaned.All(char.IsDigit))
        {
            var last4 = cleaned[^4..];
            var masked = new string('*', cleaned.Length - 4);
            return $"{masked}{last4}";
        }

        return new string('*', value.Length);
    }

    // Mask IFSC (mask_ifsc_code): <6 returned as-is; >=8 -> first4+*+last2; 6-7 -> first2+*+last2.
    private static string MaskIfsc(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var cleaned = value.Trim().ToUpperInvariant();
        var len = cleaned.Length;
        if (len < 6) return value;
        if (len >= 8) return cleaned[..4] + new string('*', len - 6) + cleaned[^2..];
        return cleaned[..2] + new string('*', len - 4) + cleaned[^2..];
    }

    // Mask PAN: First 3 chars + asterisks + last char (e.g., "BXXPD8601C" -> "BXX******C")
    private static string MaskPan(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var cleaned = value.Trim().ToUpperInvariant();
        if (cleaned.Length >= 4)
        {
            var first3 = cleaned[..3];
            var last1 = cleaned[^1..];
            var masked = new string('*', cleaned.Length - 4);
            return $"{first3}{masked}{last1}";
        }

        return new string('*', value.Length);
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

    private static string SafeUrlDecode(string value)
    {
        try
        {
            return Uri.UnescapeDataString(value);
        }
        catch
        {
            return value;
        }
    }
}
