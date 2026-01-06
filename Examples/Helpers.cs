using System;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Exception;

namespace Examples;

/// <summary>
/// Helper utilities for examples
/// </summary>
public static class Helpers
{
    public static string GenerateInvoiceId(string? tag = "INV", string? sdkPrefixOverride = null)
    {
        static string Sanitize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "NA";
            // Keep only alphanumerics, replace others with underscore to be safe for API usage.
            var chars = s.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }
            return new string(chars);
        }

        // Example: Nimbbl_NETSDK-INV-20260106104039999-1a2b3c4d5e6f
        // Note: We intentionally avoid referencing SDK internals (e.g., SdkConstants) from Examples.
        // Keep this stable and aligned with the logger's visible SDK label.
        var sdkPrefix = Sanitize(string.IsNullOrWhiteSpace(sdkPrefixOverride) ? "Nimbbl_NETSDK" : sdkPrefixOverride);
        var safeTag = Sanitize(tag);
        return $"{sdkPrefix}-{safeTag}-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..12]}";
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SUCCESS] {message}");
        Console.ResetColor();
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[INFO] {message}");
        Console.ResetColor();
    }

    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARNING] {message}");
        Console.ResetColor();
    }

    public static void PrintException(Exception ex)
    {
        PrintError($"Exception: {ex.Message}");
        
        if (ex is NimbblException nimbblEx)
        {
            if (!string.IsNullOrEmpty(nimbblEx.ErrorCode))
                Console.WriteLine($"  Error Code: {nimbblEx.ErrorCode}");
            if (nimbblEx.StatusCode > 0)
                Console.WriteLine($"  HTTP Status: {nimbblEx.StatusCode}");
        }
    }

    public static string? GetInput(string prompt, bool required = true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();
        if (required && string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    public static void PrintHeader(string title = "=== Nimbbl .NET SDK Examples ===")
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(title);
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintStep(int stepNumber, string stepName)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Step {stepNumber}: {stepName}");
        Console.WriteLine(new string('-', 60));
        Console.ResetColor();
    }

    public static void PrintSeparator()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(new string('=', 60));
        Console.ResetColor();
    }

    public static void PrintDocLink(string url, string description = "")
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(" Reference: ");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(url);
        Console.ResetColor();
        if (!string.IsNullOrEmpty(description))
        {
            Console.Write($" - {description}");
        }
        Console.WriteLine();
    }
}

