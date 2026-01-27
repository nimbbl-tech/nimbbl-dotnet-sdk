using System.Text.Json;
using Nimbbl.Sdk.Rest;
using Nimbbl.Sdk.Rest;
using Nimbbl.Sdk.Rest.Exception;

namespace Examples;

/// <summary>
/// Order Examples
/// </summary>
public static class OrderExamples
{
    /// <summary>
    /// Create Order - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task CreateOrderExample(NimbblApi api)
    {
        try
        {
            Helpers.PrintInfo("Enter order details:\n");
            
            var invoiceId = Helpers.GetInput("Enter Invoice ID (optional, auto-generated if blank): ", false);
            if (string.IsNullOrWhiteSpace(invoiceId))
            {
                // Auto-generate a unique invoice ID if user leaves it blank
                invoiceId = Helpers.GenerateInvoiceId("INV");
                Helpers.PrintInfo($"Generated Invoice ID: {invoiceId}\n");
            }
            
            var totalAmountStr = Helpers.GetInput("Enter Total Amount: ");
            if (string.IsNullOrWhiteSpace(totalAmountStr))
            {
                Helpers.PrintError("Total Amount is required.\n");
                return;
            }
            
            var amountBeforeTaxStr = Helpers.GetInput("Enter Amount Before Tax: ");
            if (string.IsNullOrWhiteSpace(amountBeforeTaxStr))
            {
                Helpers.PrintError("Amount Before Tax is required.\n");
                return;
            }
            
            var taxStr = Helpers.GetInput("Enter Tax (default: 0): ", false) ?? "0";
            var currency = Helpers.GetInput("Enter Currency (default: INR): ", false) ?? "INR";
            
            // User details
            Helpers.PrintInfo("\nUser Details:\n");
            var email = Helpers.GetInput("Enter User Email (default: user@example.com): ", false) ?? "user@example.com";
            var firstName = Helpers.GetInput("Enter User First Name (default: Test): ", false) ?? "Test";
            var lastName = Helpers.GetInput("Enter User Last Name (optional): ", false);
            var countryCode = Helpers.GetInput("Enter Country Code (default: +91): ", false) ?? "+91";
            var mobileNumber = Helpers.GetInput("Enter Mobile Number (default: 9999999999): ", false) ?? "9999999999";
            
            // Build order data
            var orderData = new Dictionary<string, object?>
            {
                ["invoice_id"] = invoiceId,
                ["total_amount"] = decimal.Parse(totalAmountStr),
                ["amount_before_tax"] = decimal.Parse(amountBeforeTaxStr),
                ["tax"] = decimal.Parse(taxStr),
                ["currency"] = currency,
                ["user"] = new Dictionary<string, object?>
                {
                    ["email"] = email,
                    ["first_name"] = firstName,
                    ["country_code"] = countryCode,
                    ["mobile_number"] = mobileNumber
                }
            };
            
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                ((Dictionary<string, object?>)orderData["user"]!)["last_name"] = lastName;
            }
            
            // Merchant token is automatically generated and used for authentication
            var order = await api.Orders().CreateOrderAsync(orderData);
            
            if (order.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Order created successfully!\n");
                var orderId = order.TryGetProperty("order_id", out var oid) 
                    ? oid.GetString() 
                    : (order.TryGetProperty("nimbbl_order_id", out var noid) ? noid.GetString() : "N/A");
                var invoiceIdResult = order.TryGetProperty("invoice_id", out var iid) ? iid.GetString() : "N/A";
                var orderToken = order.TryGetProperty("token", out var ot) ? ot.GetString() : "N/A";
                Console.WriteLine($"   Order ID: {orderId}");
                Console.WriteLine($"   Invoice ID: {invoiceIdResult}");
                Console.WriteLine($"   Order Token: {orderToken}");
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    /// <summary>
    /// Get Order by ID - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task GetOrderByIdExample(NimbblApi api)
    {
        try
        {
            var orderId = Helpers.GetInput("Enter Order ID: ");
            if (string.IsNullOrWhiteSpace(orderId))
            {
                Helpers.PrintError("Order ID is required.\n");
                return;
            }
            
            // Merchant token is automatically generated and used for authentication
            var order = await api.Orders().GetOrderByIdAsync(orderId);
            
            if (order.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Order retrieved successfully!\n");
                var oid = order.TryGetProperty("order_id", out var oidProp) 
                    ? oidProp.GetString() 
                    : (order.TryGetProperty("nimbbl_order_id", out var noid) ? noid.GetString() : "N/A");
                var iid = order.TryGetProperty("invoice_id", out var iidProp) ? iidProp.GetString() : "N/A";
                var stat = order.TryGetProperty("status", out var statProp) ? statProp.GetString() : "N/A";
                var amt = order.TryGetProperty("total_amount", out var amtProp) 
                    ? amtProp.GetDecimal() 
                    : 0m;
                var curr = order.TryGetProperty("currency", out var currProp) ? currProp.GetString() : "INR";
                Console.WriteLine($"   Order ID: {oid}");
                Console.WriteLine($"   Invoice ID: {iid}");
                Console.WriteLine($"   Status: {stat}");
                Console.WriteLine($"   Amount: {amt} {curr}");
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    /// <summary>
    /// Get Order by Invoice ID - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task GetOrderByInvoiceIdExample(NimbblApi api)
    {
        try
        {
            var invoiceId = Helpers.GetInput("Enter Invoice ID: ");
            if (string.IsNullOrWhiteSpace(invoiceId))
            {
                Helpers.PrintError("Invoice ID is required.\n");
                return;
            }
            
            // Merchant token is automatically generated and used for authentication
            var order = await api.Orders().GetOrderByInvoiceIdAsync(invoiceId);
            
            if (order.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Order retrieved successfully!\n");
                var oid = order.TryGetProperty("order_id", out var oidProp) 
                    ? oidProp.GetString() 
                    : (order.TryGetProperty("nimbbl_order_id", out var noid) ? noid.GetString() : "N/A");
                var iid = order.TryGetProperty("invoice_id", out var iidProp) ? iidProp.GetString() : "N/A";
                var stat = order.TryGetProperty("status", out var statProp) ? statProp.GetString() : "N/A";
                var amt = order.TryGetProperty("total_amount", out var amtProp) 
                    ? amtProp.GetDecimal() 
                    : 0m;
                var curr = order.TryGetProperty("currency", out var currProp) ? currProp.GetString() : "INR";
                Console.WriteLine($"   Order ID: {oid}");
                Console.WriteLine($"   Invoice ID: {iid}");
                Console.WriteLine($"   Status: {stat}");
                Console.WriteLine($"   Amount: {amt} {curr}");
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }
}
