using System;
using System.Text;
using System.Text.Json;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Generic helper to unwrap, decrypt, and sanitize webhook/callback payloads.
/// </summary>
public static class PayloadHelperUtils
{
    /// <summary>
    /// Parses the raw callback or webhook payload, handling decryption and "payload" unwrapping internally.
    /// Use this before calling VerifySignature.
    /// </summary>
    /// <param name="payload">The raw JSON payload string</param>
    /// <param name="secret">The merchant's access secret key</param>
    /// <returns>A processed JsonElement containing events attributes for verification</returns>
    public static JsonElement Parse(string payload, string secret)
    {
        var logger = Log.Logger.GetInstance();
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            // 1. Detection logic for encrypted_response at various levels
            string? encryptedResponse = null;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty(JsonKeys.EncryptedResponse, out var encProp) && encProp.ValueKind == JsonValueKind.String)
                {
                    encryptedResponse = encProp.GetString();
                }
                else if (root.TryGetProperty(JsonKeys.Payload, out var pProp) && pProp.TryGetProperty(JsonKeys.EncryptedResponse, out var pEncProp) && pEncProp.ValueKind == JsonValueKind.String)
                {
                    encryptedResponse = pEncProp.GetString();
                }
                else if (root.TryGetProperty(JsonKeys.Callback, out var cProp) && cProp.TryGetProperty(JsonKeys.EncryptedResponse, out var cEncProp) && cEncProp.ValueKind == JsonValueKind.String)
                {
                    encryptedResponse = cEncProp.GetString();
                }
            }

            JsonElement processed;
            if (!string.IsNullOrWhiteSpace(encryptedResponse))
            {
                logger.InfoWithCaller($"PayloadHelperUtils: decrypting {JsonKeys.EncryptedResponse}.");
                var enc = new Encryption(secret);
                var decrypted = enc.Decrypt(encryptedResponse!, true);
                using var decryptedDoc = JsonDocument.Parse(decrypted);
                processed = decryptedDoc.RootElement.Clone();
            }
            else if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(JsonKeys.Callback, out var cbProp) && cbProp.ValueKind == JsonValueKind.Object)
            {
                processed = cbProp.Clone();
            }
            else
            {
                processed = root.Clone();
            }

            var eventTypeStr = JsonUtils.TryGetString(processed, JsonKeys.EventType);
            logger.DebugWithCaller($"PayloadHelperUtils: eventTypeStr: {eventTypeStr}");

            // Special handling for popup/redirect callback event type
            if (eventTypeStr == JsonKeys.GlobalHandleCheckoutResponse)
            {
                if (processed.TryGetProperty(JsonKeys.Payload, out var nestedPayload) && nestedPayload.ValueKind == JsonValueKind.Object)
                {
                    logger.DebugWithCaller($"PayloadHelperUtils: detected {JsonKeys.GlobalHandleCheckoutResponse}, unwrapping nested {JsonKeys.Payload}.");
                    processed = nestedPayload.Clone();
                }
            }

            return processed;
        }
        catch (System.Exception ex)
        {
            var failMsg = $"{ErrorMessages.MessageWebhookParseError}: {ex.Message}";
            logger.ErrorWithCaller(failMsg);
            throw;
        }
    }

    /// <summary>
    /// Parses a payment response string, which can be either base64-encoded or a regular JSON string.
    /// Automatically detects the format and handles both cases.
    /// </summary>
    /// <param name="response">The base64 encoded JSON response, or a regular JSON string</param>
    /// <param name="secret">The merchant's access secret key</param>
    /// <returns>A processed JsonElement containing events attributes for verification</returns>
    public static JsonElement ParseResponse(string response, string secret)
    {
         var logger = Log.Logger.GetInstance();
        if (string.IsNullOrWhiteSpace(response)) throw new ArgumentException("Response cannot be empty", nameof(response));
        
        try
        {
            // Try to decode as base64 first
            var jsonResponse = Encoding.UTF8.GetString(Convert.FromBase64String(response));
            return Parse(jsonResponse, secret);
        }
        catch (FormatException)
        {
            // If base64 decoding fails, treat the input as a regular JSON string
            logger.DebugWithCaller("ParseResponse: Input is not base64 encoded, treating as regular JSON string");
            return Parse(response, secret);
        }
    }

}
