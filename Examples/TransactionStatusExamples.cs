using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.Common;

namespace Examples;

/// <summary>
/// Transaction Status Examples
/// Note: Uses Transactions.TransactionEnquiryAsync (TransactionStatus was removed)
/// </summary>
public static class TransactionStatusExamples
{
    /// <summary>
    /// Transaction Enquiry - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task TransactionEnquiryExample(NimbblApi api)
    {
        Helpers.PrintInfo("Enter one of: transaction_id, order_id, or invoice_id (at least one required)\n");
        
        var transactionId = Helpers.GetInput("Enter Transaction ID (or press Enter to skip): ", false);
        var orderId = Helpers.GetInput("Enter Order ID (or press Enter to skip): ", false);
        var invoiceId = Helpers.GetInput("Enter Invoice ID (or press Enter to skip): ", false);
        
        var transactionData = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            transactionData["transaction_id"] = transactionId;
        }
        if (!string.IsNullOrWhiteSpace(orderId))
        {
            transactionData["order_id"] = orderId;
        }
        if (!string.IsNullOrWhiteSpace(invoiceId))
        {
            transactionData["invoice_id"] = invoiceId;
        }
        
        if (transactionData.Count == 0)
        {
            Helpers.PrintError("At least one of Transaction ID, Order ID, or Invoice ID is required.\n");
            return;
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
            
            var result = await api.Transactions().TransactionEnquiryAsync(transactionData);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Transaction enquiry successful!\n");
                
                // Display transactions
                if (result.TryGetProperty("transaction", out var txnProp) && 
                    txnProp.ValueKind == JsonValueKind.Array)
                {
                    Console.WriteLine("\nTransaction(s):\n");
                    var idx = 1;
                    foreach (var transaction in txnProp.EnumerateArray())
                    {
                        Console.WriteLine($"  Transaction {idx}:");
                        var tid = transaction.TryGetProperty("transaction_id", out var tidProp) 
                            ? tidProp.GetString() 
                            : (transaction.TryGetProperty("nimbbl_transaction_id", out var ntid) ? ntid.GetString() : "N/A");
                        var stat = transaction.TryGetProperty("status", out var statProp) ? statProp.GetString() : "N/A";
                        var amt = transaction.TryGetProperty("amount", out var amtProp) 
                            ? amtProp.GetDecimal() 
                            : 0m;
                        var curr = transaction.TryGetProperty("currency", out var currProp) ? currProp.GetString() : "INR";
                        Console.WriteLine($"    Transaction ID: {tid}");
                        Console.WriteLine($"    Status: {stat}");
                        Console.WriteLine($"    Amount: {amt} {curr}");
                        if (transaction.TryGetProperty(CheckoutConstants.OptionKeyPaymentModeCode, out var pmProp))
                        {
                            Console.WriteLine($"    Payment Mode: {pmProp.GetString()}");
                        }
                        idx++;
                    }
                }
                else
                {
                    Console.WriteLine("\nNo transactions found for this order.\n");
                }
                
                // Display order information
                if (result.TryGetProperty("order", out var orderProp) && 
                    orderProp.ValueKind == JsonValueKind.Object)
                {
                    Console.WriteLine("\nOrder:\n");
                    var order = orderProp;
                    var oid = order.TryGetProperty("nimbbl_order_id", out var oidProp) ? oidProp.GetString() : "N/A";
                    var iid = order.TryGetProperty("invoice_id", out var iidProp) ? iidProp.GetString() : "N/A";
                    var stat = order.TryGetProperty("status", out var statProp) ? statProp.GetString() : "N/A";
                    var amt = order.TryGetProperty("total_amount", out var amtProp) 
                        ? amtProp.GetDecimal() 
                        : 0m;
                    var curr = order.TryGetProperty("currency", out var currProp) ? currProp.GetString() : "INR";
                    Console.WriteLine($"  Order ID: {oid}");
                    Console.WriteLine($"  Invoice ID: {iid}");
                    Console.WriteLine($"  Status: {stat}");
                    Console.WriteLine($"  Amount: {amt} {curr}");
                    if (order.TryGetProperty("offer_discount", out var odProp) && odProp.GetDecimal() > 0)
                    {
                        Console.WriteLine($"  Offer Discount: {odProp.GetDecimal()} {curr}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
        
        Console.WriteLine("\nKey Points:\n");
        Console.WriteLine("  • Transaction Status API gets the latest status of order and transactions\n");
        Console.WriteLine("  • Can query by order_id, invoice_id, or transaction_id\n");
        Console.WriteLine("  • Returns comprehensive transaction and order status information\n");
        Console.WriteLine("  • Use this to check the current state of a transaction\n");
        Console.WriteLine("\nFor more details, see: https://nimbbl.biz/docs/api-reference/transaction-enquiry-v-3/\n");
    }

    /// <summary>
    /// Run All Transaction Status Examples - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task RunAllExamples(NimbblApi api)
    {
        Helpers.PrintHeader("=== Transaction Status Examples ===");
        await TransactionEnquiryExample(api);
    }
}
