using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Exception;

namespace Examples;

/// <summary>
/// Exception Handling Examples
/// </summary>
public static class ExceptionHandlingExamples
{
    public static async Task RunAllExamples(NimbblApi api)
    {
        Helpers.PrintHeader("=== Exception Handling Examples ===");
        
        // Merchant token is automatically generated and used for authentication
        
        // Example 1: Basic Exception Handling
        Console.WriteLine("Example 1: Basic Exception Handling");
        Console.WriteLine(new string('-', 50));
        await BasicExceptionHandlingExample(api);
        
        Console.WriteLine("\n\n");
        
        // Example 2: Specific Exception Types
        Console.WriteLine("Example 2: Handling Specific Exception Types");
        Console.WriteLine(new string('-', 50));
        await SpecificExceptionTypesExample(api);
        
        Console.WriteLine("\n\n");
        
        // Example 3: Exception Data Access
        Console.WriteLine("Example 3: Accessing Exception Data");
        Console.WriteLine(new string('-', 50));
        await ExceptionDataAccessExample(api);
        
        Console.WriteLine("\n\n");
        
        // Example 4: Best Practice - Comprehensive Error Handling
        Console.WriteLine("Example 4: Best Practice - Comprehensive Error Handling");
        Console.WriteLine(new string('-', 50));
        await ComprehensiveErrorHandlingExample(api);
        
        Console.WriteLine("\n");
        Console.WriteLine("=== Exception Handling Examples Complete ===");
        Console.WriteLine("\nKey Points:\n");
        Console.WriteLine("  • Use specific exception types for better error handling\n");
        Console.WriteLine("  • Always catch NimbblException or more specific types\n");
        Console.WriteLine("  • Access error details via ErrorCode, RequestId, etc.\n");
        Console.WriteLine("  • Log exceptions appropriately for debugging\n");
        Console.WriteLine("  • Provide user-friendly error messages\n");
    }

    private static async Task BasicExceptionHandlingExample(NimbblApi api)
    {
        try
        {
            var orderData = new Dictionary<string, object?>
            {
                ["invoice_id"] = $"test_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                ["amount_before_tax"] = 900m,
                ["tax"] = 100m,
                ["total_amount"] = 1000m,
                ["currency"] = "INR",
                ["user"] = new Dictionary<string, object?>
                {
                    ["email"] = "test@example.com",
                    ["first_name"] = "Test",
                    ["last_name"] = "User",
                    ["mobile_number"] = "9876543210",
                    ["country_code"] = "+91"
                }
            };
            
            var order = await api.Orders().CreateOrderAsync(orderData);
            Helpers.PrintSuccess("Order created successfully\n");
        }
        catch (NimbblException ex)
        {
            Console.WriteLine("[ERROR] Nimbbl Exception caught:\n");
            Console.WriteLine($"  Message: {ex.Message}\n");
            Console.WriteLine($"  Error Code: {ex.ErrorCode ?? "N/A"}\n");
            Console.WriteLine($"  HTTP Status: {(ex.StatusCode > 0 ? ex.StatusCode.ToString() : "N/A")}\n");
        }
    }

    private static async Task SpecificExceptionTypesExample(NimbblApi api)
    {
        try
        {
            // Try to retrieve a non-existent order
            var order = await api.Orders().GetOrderByIdAsync("non_existent_order_id");
        }
        catch (AuthenticationException ex)
        {
            Console.WriteLine("[ERROR] Authentication failed (401):\n");
            Console.WriteLine($"  {ex.Message}\n");
            Console.WriteLine("  → Check your access_key and access_secret\n");
        }
        catch (BadRequestException ex)
        {
            Console.WriteLine("[ERROR] Bad request (400/422):\n");
            Console.WriteLine($"  {ex.Message}\n");
            Console.WriteLine("  → Check your request parameters\n");
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine("[ERROR] Resource not found (404):\n");
            Console.WriteLine($"  {ex.Message}\n");
            Console.WriteLine("  → The requested resource does not exist\n");
        }
        catch (RateLimitException ex)
        {
            Console.WriteLine("[ERROR] Rate limit exceeded (429):\n");
            Console.WriteLine($"  {ex.Message}\n");
            Console.WriteLine("  → Too many requests. Please retry after some time\n");
        }
        catch (ServerException ex)
        {
            Console.WriteLine("[ERROR] Server error (5xx):\n");
            Console.WriteLine($"  {ex.Message}\n");
            Console.WriteLine("  → Nimbbl server is experiencing issues. Please retry later\n");
        }
        catch (ApiException ex)
        {
            Console.WriteLine("[ERROR] API error:\n");
            Console.WriteLine($"  {ex.Message}\n");
            Console.WriteLine($"  Error Code: {ex.ErrorCode ?? "N/A"}\n");
        }
        catch (NimbblException ex)
        {
            Console.WriteLine("[ERROR] General Nimbbl exception:\n");
            Console.WriteLine($"  {ex.Message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Unexpected exception:\n");
            Console.WriteLine($"  {ex.Message}\n");
        }
    }

    private static async Task ExceptionDataAccessExample(NimbblApi api)
    {
        try
        {
            // This will fail with invalid data
            var orderData = new Dictionary<string, object?>
            {
                ["invoice_id"] = $"test_invalid_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                ["amount_before_tax"] = -90m, // Invalid amount (negative)
                ["tax"] = -10m,
                ["total_amount"] = -100m, // Invalid amount (negative)
                ["currency"] = "INR",
                ["user"] = new Dictionary<string, object?>
                {
                    ["email"] = "test@example.com",
                    ["first_name"] = "Test",
                    ["last_name"] = "User",
                    ["mobile_number"] = "9876543210",
                    ["country_code"] = "+91"
                }
            };
            
            var order = await api.Orders().CreateOrderAsync(orderData);
        }
        catch (BadRequestException ex)
        {
            Console.WriteLine("[ERROR] Bad Request Exception:\n");
            Console.WriteLine($"  Message: {ex.Message}\n");
            Console.WriteLine($"  Error Code: {ex.ErrorCode ?? "N/A"}\n");
            Console.WriteLine($"  HTTP Status: {(ex.StatusCode > 0 ? ex.StatusCode.ToString() : "N/A")}\n");
        }
    }

    private static async Task ComprehensiveErrorHandlingExample(NimbblApi api)
    {
        var result = await CreateOrderSafely(api, new Dictionary<string, object?>
        {
            ["invoice_id"] = $"test_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            ["amount_before_tax"] = 900m,
            ["tax"] = 100m,
            ["total_amount"] = 1000m,
            ["currency"] = "INR",
            ["user"] = new Dictionary<string, object?>
            {
                ["email"] = "test@example.com",
                ["first_name"] = "Test",
                ["last_name"] = "User",
                ["mobile_number"] = "9876543210",
                ["country_code"] = "+91"
            }
        });

        if (result.Success)
        {
            Helpers.PrintSuccess("Order created successfully\n");
        }
        else
        {
            Console.WriteLine("[ERROR] Order creation failed:\n");
            Console.WriteLine($"  Type: {result.ErrorType}\n");
            Console.WriteLine($"  Error: {result.ErrorMessage}\n");
            if (result.HttpStatusCode.HasValue)
            {
                Console.WriteLine($"  HTTP Status: {result.HttpStatusCode}\n");
            }
        }
    }

    private static async Task<(bool Success, string? ErrorMessage, string? ErrorType, int? HttpStatusCode)> CreateOrderSafely(NimbblApi api, Dictionary<string, object?> orderData)
    {
        try
        {
            var order = await api.Orders().CreateOrderAsync(orderData);
            return (true, null, null, null);
        }
        catch (AuthenticationException ex)
        {
            return (false, "Authentication failed. Please check your credentials.", "authentication", ex.StatusCode > 0 ? ex.StatusCode : null);
        }
        catch (BadRequestException ex)
        {
            return (false, "Invalid request. Please check your input data.", "validation", ex.StatusCode > 0 ? ex.StatusCode : null);
        }
        catch (RateLimitException ex)
        {
            return (false, "Too many requests. Please try again later.", "rate_limit", ex.StatusCode > 0 ? ex.StatusCode : null);
        }
        catch (ServerException ex)
        {
            return (false, "Server error. Please try again later.", "server_error", ex.StatusCode > 0 ? ex.StatusCode : null);
        }
        catch (NimbblException ex)
        {
            return (false, ex.Message, "nimbbl_error", ex.StatusCode > 0 ? ex.StatusCode : null);
        }
        catch (Exception)
        {
            return (false, "An unexpected error occurred.", "unexpected", null);
        }
    }
}
