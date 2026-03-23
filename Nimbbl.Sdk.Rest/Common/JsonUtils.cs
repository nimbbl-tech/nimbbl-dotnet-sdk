using System.Globalization;
using System.Text.Json;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Utility helpers for JSON parsing and property extraction.
/// </summary>
public static class JsonUtils
{
    /// <summary>
    /// Safely attempts to get a string value from a JsonElement.
    /// </summary>
    public static string? TryGetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.String
            ? value.GetString()
            : null;
    }

    /// <summary>
    /// Safely attempts to get a double value from a JsonElement, handling both Number and String kinds.
    /// </summary>
    public static double? TryGetDouble(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;
        if (!element.TryGetProperty(propertyName, out var value)) return null;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)) return d;
        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ds)) return ds;
        return null;
    }
}
