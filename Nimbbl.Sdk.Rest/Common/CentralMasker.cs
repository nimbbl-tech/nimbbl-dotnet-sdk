using System.Linq;
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
    // Based on Nimbbl PII masking guidelines: https://nimbbl.biz/docs/guides/handling-pii-data/
    private static readonly HashSet<string> SensitiveBodyKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        // Authentication credentials (masked in INFO, visible in DEBUG)
       JsonKeys.AccessKey, JsonKeys.AccessSecret,
        // Authentication tokens
        JsonKeys.Token,JsonKeys.RefreshToken,
        // Names (First letter + asterisks)
        JsonKeys.FirstName, JsonKeys.LastName, JsonKeys.CardHolderName, JsonKeys.UpiHolder,
        // Mobile/Phone (Country code + last 4 digits)
         JsonKeys.MobileNumber,
        // Email (First 2 chars + asterisks + domain)
        JsonKeys.Email,
        // Address fields (First char/letters + asterisks)
          JsonKeys.Street, JsonKeys.Landmark, JsonKeys.Area, JsonKeys.City,
        // Pincode (First 2 digits + asterisks)
        JsonKeys.Pincode, JsonKeys.PinCode, JsonKeys.PostalCode, JsonKeys.ZipCode,
        // UPI/VPA (First 2 digits + asterisks + last 2 digits + domain)
        JsonKeys.Vpa,
        // Payment card sensitive data
        JsonKeys.CardNo, JsonKeys.CardNumber, JsonKeys.Cvv,JsonKeys.ExpiryDate,
        // Account details
        JsonKeys.AccountNumber, JsonKeys.AccountNo, JsonKeys.IfscCode, JsonKeys.PanCard,
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
                            var key = prop.Name.ToLowerInvariant();
                            
                            // Use appropriate masking based on key type (following Nimbbl PII masking guidelines)
                            if (key == JsonKeys.AccessKey || key == JsonKeys.AccessSecret)
                                return (object)MaskAccessKey(value);
                            if (key.Contains("token", StringComparison.OrdinalIgnoreCase))
                                return (object)MaskToken(value);
                            if (key.Contains("name") || key == JsonKeys.FirstName || key == JsonKeys.LastName || key == JsonKeys.UpiHolder || key == JsonKeys.CardHolderName)
                                return (object)MaskName(value);
                            if (key.Contains("phone") || key.Contains("mobile") || key.Contains("contact_number"))
                                return (object)MaskPhone(value);
                            if (key.Contains("email"))
                                return (object)MaskEmail(value);
                            if (key.Contains("address") || key == JsonKeys.Street || key == JsonKeys.Landmark || key == JsonKeys.Area || key == JsonKeys.City)
                                return (object)MaskAddress(value);
                            if (key.Contains("pincode") || key == JsonKeys.Pincode || key == JsonKeys.PostalCode || key == JsonKeys.ZipCode)
                                return (object)MaskPincode(value);
                            if (key.Contains("upi") || key == JsonKeys.Vpa)
                                return (object)MaskUpiId(value);
                            if (key == JsonKeys.CardNo || key == JsonKeys.CardNumber)
                                return (object)MaskCardNumber(value);
                            if (key == JsonKeys.Cvv)
                                return (object)"XXX";
                            if (key.Contains("expiry") || key == JsonKeys.ExpiryDate)
                                return (object)"XX/XXXX";
                            if (key == JsonKeys.AccountNumber || key == JsonKeys.AccountNo)
                                return (object)MaskAccountNumber(value);
                            if (key.Contains("ifsc") || key == JsonKeys.IfscCode)
                                return (object)MaskIfsc(value);
                            if (key == JsonKeys.PanCard)
                                return (object)MaskPan(value);
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
        return $"{value[..4]}***{value[^4..]}";
    }
    
    // Mask token with more characters visible (for longer tokens)
    private static string MaskToken(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= 12) return new string('*', value.Length);
        // Show first 5 and last 7 characters for tokens
        return $"{value[..5]}***********{value[^7..]}";
    }
    
    // Mask access_key and access_secret: Show first 4 and last 4 characters
    // For fixed-length 20-character keys: "pKx7rWVgVpbXQvq2" -> "pKx7****XQvq2"
    private static string MaskAccessKey(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= 8) return new string('*', value.Length);
        // Show first 4 and last 4 characters for access keys/secrets (consistent for fixed-length 20-char keys)
        return $"{value[..4]}****{value[^4..]}";
    }


    // Mask name: First letter + asterisks (e.g., "Diana Prince" -> "D**** P*****")
    private static string MaskName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        // Split by spaces to handle full names
        var parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return new string('*', value.Length);
        
        var maskedParts = parts.Select(part =>
        {
            if (string.IsNullOrWhiteSpace(part)) return part;
            if (part.Length == 1) return part + "*";
            // First letter + asterisks
            return part[0] + new string('*', part.Length - 1);
        });
        
        return string.Join(" ", maskedParts);
    }
    
    // Mask phone: Country code + last 4 digits (e.g., "+91 9876543210" -> "+91 ******3210")
    private static string MaskPhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        // Remove spaces and common separators
        var cleaned = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        
        // Try to detect country code (starts with +)
        if (cleaned.StartsWith("+"))
        {
            // Find where country code ends (usually 1-3 digits after +)
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
        
        // If no country code, show last 4 digits
        if (cleaned.Length >= 4 && cleaned.All(char.IsDigit))
        {
            var last4 = cleaned[^4..];
            var masked = new string('*', cleaned.Length - 4);
            return $"{masked}{last4}";
        }
        
        return new string('*', value.Length);
    }
    
    // Mask email: First 2 chars + asterisks + domain (e.g., "[email protected]" -> "wo********@example.com")
    private static string MaskEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        var atIndex = value.IndexOf('@');
        if (atIndex > 0 && atIndex < value.Length - 1)
        {
            var localPart = value[..atIndex];
            var domain = value[atIndex..];
            
            if (localPart.Length <= 2)
            {
                return new string('*', localPart.Length) + domain;
            }
            
            var first2 = localPart[..2];
            var masked = new string('*', localPart.Length - 2);
            return $"{first2}{masked}{domain}";
        }
        
        return new string('*', value.Length);
    }
    
    // Mask address: First char + asterisks (e.g., "123 Main Street" -> "1** M*** S*****")
    private static string MaskAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        // Split by spaces to handle multi-word addresses
        var parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return new string('*', value.Length);
        
        var maskedParts = parts.Select(part =>
        {
            if (string.IsNullOrWhiteSpace(part)) return part;
            if (part.Length == 1) return part + "*";
            // First char + asterisks
            return part[0] + new string('*', part.Length - 1);
        });
        
        return string.Join(" ", maskedParts);
    }
    
    // Mask pincode: First 2 digits + asterisks (e.g., "100389" -> "10****")
    private static string MaskPincode(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        var cleaned = value.Trim().Replace(" ", "").Replace("-", "");
        if (cleaned.Length >= 2 && cleaned.All(char.IsDigit))
        {
            var first2 = cleaned[..2];
            var masked = new string('*', cleaned.Length - 2);
            return $"{first2}{masked}";
        }
        
        return new string('*', value.Length);
    }
    
    // Mask UPI ID: First 2 digits + asterisks + last 2 digits + domain (e.g., "91111111111@superyes" -> "91*******11@superyes")
    private static string MaskUpiId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        var atIndex = value.IndexOf('@');
        if (atIndex > 0 && atIndex < value.Length - 1)
        {
            var localPart = value[..atIndex];
            var domain = value[atIndex..];
            
            if (localPart.Length <= 4)
            {
                return new string('*', localPart.Length) + domain;
            }
            
            var first2 = localPart[..2];
            var last2 = localPart[^2..];
            var masked = new string('*', localPart.Length - 4);
            return $"{first2}{masked}{last2}{domain}";
        }
        
        // If no @, treat as number and mask
        if (value.Length >= 4)
        {
            var first2 = value[..2];
            var last2 = value[^2..];
            var masked = new string('*', value.Length - 4);
            return $"{first2}{masked}{last2}";
        }
        
        return new string('*', value.Length);
    }
    
    // Mask card number: Last 4 digits visible (e.g., "4111 1111 1111 1111" -> "XXXX XXXX XXXX 1111")
    private static string MaskCardNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        // Remove spaces and dashes
        var cleaned = value.Replace(" ", "").Replace("-", "");
        
        if (cleaned.Length >= 4 && cleaned.All(char.IsDigit))
        {
            var last4 = cleaned[^4..];
            // Format as XXXX XXXX XXXX 1111 (group by 4)
            var masked = new string('X', cleaned.Length - 4);
            var formatted = string.Join(" ", Enumerable.Range(0, (masked.Length + 3) / 4)
                .Select(i => masked[(i * 4)..Math.Min(i * 4 + 4, masked.Length)]));
            return formatted + " " + last4;
        }
        
        return "XXXX XXXX XXXX XXXX";
    }
    
    // Mask account number: Last 4 digits visible (e.g., "123456789012" -> "********9012")
    private static string MaskAccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        var cleaned = value.Trim().Replace(" ", "").Replace("-", "");
        if (cleaned.Length >= 4 && cleaned.All(char.IsDigit))
        {
            var last4 = cleaned[^4..];
            var masked = new string('*', cleaned.Length - 4);
            return $"{masked}{last4}";
        }
        
        return new string('*', value.Length);
    }
    
    // Mask IFSC: First 4 chars + asterisks + last 2 chars (e.g., "UTIB00047" -> "UTIB***47")
    private static string MaskIfsc(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        var cleaned = value.Trim().ToUpperInvariant();
        if (cleaned.Length >= 6)
        {
            var first4 = cleaned[..4];
            var last2 = cleaned[^2..];
            var masked = new string('*', cleaned.Length - 6);
            return $"{first4}{masked}{last2}";
        }
        
        return new string('*', value.Length);
    }
    
    // Mask PAN: First 3 chars + asterisks + last char (e.g., "BXXPD8601C" -> "BXX******C")
    private static string MaskPan(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
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
}

