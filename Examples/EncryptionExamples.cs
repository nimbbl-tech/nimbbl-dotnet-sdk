using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Exception;

namespace Examples;

/// <summary>
/// Encryption Examples
/// Note: Full AES-GCM encryption implementation is pending. This is a placeholder example.
/// </summary>
public static class EncryptionExamples
{
    /// <summary>
    /// Run Encryption Examples - Function to be called from Program.cs or standalone
    /// </summary>
    public static Task RunAllExamples(NimbblApi api)
    {
        Helpers.PrintHeader("=== Nimbbl Encryption/Decryption Examples ===");
                
        Console.WriteLine("For more information:\n");
        Console.WriteLine("  - Standard Checkout Integration: https://nimbbl.biz/docs/standard-checkout/completing-integration/\n");
        Console.WriteLine("  - Encryption/Decryption Guide: https://nimbbl.biz/docs/guides/encrypt-decrypt-payload/\n");
        Console.WriteLine("\nThe Encryption class provides:\n");
        Console.WriteLine("  - AES-GCM encryption/decryption\n");
        Console.WriteLine("  - Hex-encoded output\n");
        Console.WriteLine("  - Support for encrypting arrays/dictionaries\n");
        Console.WriteLine("  - Automatic JSON encoding/decoding\n");
        
        return Task.CompletedTask;
    }
}

