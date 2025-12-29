using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;

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
            Console.WriteLine($"  access_key: {api.GetType().Name}"); // Note: In real usage, key is in config
            Console.WriteLine("  access_secret: " + new string('*', 20));
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
                
                Console.WriteLine($"   Token: {(token != "N/A" ? token : "N/A")}");
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

