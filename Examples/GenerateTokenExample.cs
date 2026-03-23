using System.Text.Json;
using Nimbbl.Sdk.Rest;

namespace Examples;

/// <summary>
/// Generate Token Example
/// </summary>
public static class GenerateTokenExample
{
    /// <summary>
    /// Generate Token - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task GenerateTokenExampleAsync(NimbblApi api)
    {
        try
        {
            Console.WriteLine("Generating authentication token...");
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("Request:");
            // Note: access_key and access_secret are automatically masked in SDK logs
            // Format in logs: First 4 chars + **** + last 4 chars (e.g., "Nimb****7890")
            Console.WriteLine();
            
            // Use Auth API client to generate token
            var tokenResponse = await api.Auth().GenerateTokenAsync();
            
            if (tokenResponse.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Token generation failed: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Token generated successfully!\n");
                var token = tokenResponse.TryGetProperty("token", out var tokenProp) 
                    ? tokenProp.GetString() 
                    : "N/A";
                var expiresAt = tokenResponse.TryGetProperty("expires_at", out var expProp) 
                    ? expProp.GetString() 
                    : "N/A";
                
                // Mask token for security (show first 5 and last 7 characters)
                var maskedToken = token != "N/A" && !string.IsNullOrEmpty(token) && token.Length > 12
                    ? $"{token.Substring(0, 5)}***********{token.Substring(token.Length - 7)}"
                    : token != "N/A" && !string.IsNullOrEmpty(token)
                    ? new string('*', token.Length)
                    : "N/A";
                
                Console.WriteLine($"   Token: {maskedToken}");
                Console.WriteLine($"   Expires At: {expiresAt}");
                
                Helpers.PrintInfo("Token can be used for subsequent API calls.\n");
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }
}

