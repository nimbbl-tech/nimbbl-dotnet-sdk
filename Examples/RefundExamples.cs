using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Exception;

namespace Examples;

/// <summary>
/// Refund Examples
/// </summary>
public static class RefundExamples
{
    /// <summary>
    /// Initiate Refund - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task InitiateRefundExample(NimbblApi api)
    {
        Helpers.PrintInfo("Enter either transaction_id OR invoice_id (at least one required)\n");
        
        var transactionId = Helpers.GetInput("Enter Transaction ID (or press Enter to skip): ", false);
        var invoiceId = Helpers.GetInput("Enter Invoice ID (or press Enter to skip): ", false);
        
        if (string.IsNullOrWhiteSpace(transactionId) && string.IsNullOrWhiteSpace(invoiceId))
        {
            Helpers.PrintError("Either Transaction ID or Invoice ID is required.\n");
            return;
        }
        
        var refundAmountStr = Helpers.GetInput("Enter Refund Amount (or press Enter for full refund): ", false);
        var comment = Helpers.GetInput("Enter Refund Comment (optional): ", false);
        var refundRequestId = Helpers.GetInput("Enter Refund Request ID (optional, for idempotency): ", false);
        
        var data = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            data["transaction_id"] = transactionId;
        }
        if (!string.IsNullOrWhiteSpace(invoiceId))
        {
            data["invoice_id"] = invoiceId;
        }
        if (!string.IsNullOrWhiteSpace(refundAmountStr) && decimal.TryParse(refundAmountStr, out var refundAmount))
        {
            data["refund_amount"] = refundAmount;
        }
        if (!string.IsNullOrWhiteSpace(comment))
        {
            data["comment"] = comment;
        }
        if (!string.IsNullOrWhiteSpace(refundRequestId))
        {
            data["refund_request_id"] = refundRequestId;
        }
        
        // Order line items support (optional)
        var includeOrderLineItems = Helpers.GetInput("Include order line items? (y/N): ", false);
        if (includeOrderLineItems?.ToLower() == "y")
        {
            var orderLineItems = new List<Dictionary<string, object?>>();
            while (true)
            {
                var skuId = Helpers.GetInput("Enter SKU ID (or press Enter to finish): ", false);
                if (string.IsNullOrWhiteSpace(skuId))
                    break;
                
                var item = new Dictionary<string, object?> { ["sku_id"] = skuId };
                
                var serialNumbers = Helpers.GetInput("Enter Serial Numbers (comma-separated, optional): ", false);
                if (!string.IsNullOrWhiteSpace(serialNumbers))
                {
                    item["serial_numbers"] = serialNumbers.Split(',').Select(s => s.Trim()).ToArray();
                }
                
                orderLineItems.Add(item);
            }
            
            if (orderLineItems.Count > 0)
            {
                data["order_line_items"] = orderLineItems;
            }
        }
        
        try
        {
            // Generate merchant token
            var tokenResponse = await api.Auth().GenerateTokenAsync();
            var merchantToken = tokenResponse.TryGetProperty("token", out var tokenProp) 
                ? tokenProp.GetString() 
                : null;
            
            if (string.IsNullOrWhiteSpace(merchantToken))
            {
                Helpers.PrintError("Failed to generate merchant token.\n");
                return;
            }
            
            api.SetBearerToken(merchantToken);
            
            var result = await api.Refunds().InitiateRefundAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Refund initiated successfully!\n");
                var refundId = result.TryGetProperty("refund_id", out var rid) 
                    ? rid.GetString() 
                    : (result.TryGetProperty("nimbbl_refund_id", out var nrid) ? nrid.GetString() : "N/A");
                var tid = result.TryGetProperty("transaction_id", out var txid) 
                    ? txid.GetString() 
                    : (result.TryGetProperty("nimbbl_transaction_id", out var ntxid) ? ntxid.GetString() : "N/A");
                var ramt = result.TryGetProperty("refund_amount", out var ramtProp) 
                    ? ramtProp.GetDecimal() 
                    : 0m;
                var curr = result.TryGetProperty("currency", out var currProp) ? currProp.GetString() : "INR";
                var stat = result.TryGetProperty("status", out var statProp) ? statProp.GetString() : "N/A";
                
                Console.WriteLine($"  Refund ID: {refundId}");
                Console.WriteLine($"  Transaction ID: {tid}");
                Console.WriteLine($"  Refund Amount: {ramt} {curr}");
                Console.WriteLine($"  Status: {stat}");
                if (result.TryGetProperty("comment", out var commProp))
                {
                    Console.WriteLine($"  Comment: {commProp.GetString()}");
                }
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
        
        Console.WriteLine("\nImportant Notes:\n");
        Console.WriteLine("  • Refund transactions once requested cannot be rolled back\n");
        Console.WriteLine("  • Be very sure of the amount before initiating a refund\n");
        Console.WriteLine("  • Use refund_request_id to avoid duplicate refund requests\n");
        Console.WriteLine("  • Use Transaction Status API to check refund status\n");
        Console.WriteLine("\nFor more details, see: https://nimbbl.biz/docs/api-reference/refund-a-payment-v-3/\n");
    }

    /// <summary>
    /// Run All Refund Examples - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task RunAllExamples(NimbblApi api)
    {
        Helpers.PrintHeader("=== Refund Examples ===");
        await InitiateRefundExample(api);
    }
}
