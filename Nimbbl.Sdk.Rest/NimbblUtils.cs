using System.Security.Cryptography;
using System.Text;

namespace Nimbbl.Sdk.Rest;

public class NimbblUtils
{

    public static bool VerifySignature(WebhookCallbackResponse data, string secretKey, double? orderAmount)
    {
        if (data.Transaction != null)
        {
            WCTransaction txn = data.Transaction;
            WCOrder order = data.Order;
            string signatureString;
            var amount="0";
            if (txn.SignatureVersion != null && txn.SignatureVersion.Equals("v3") == true)
            {
                amount = FormatAmount(txn.Amount);
                signatureString = order.InvoiceId + "|" + txn.TransactionId + "|" + amount + "|" + txn.TransactionCurrency + "|" + txn.Status + "|" + txn.TransactionType;
                return CalculateSignature(signatureString, secretKey).Equals(txn.Signature);
            }
            amount = orderAmount?.ToString(".00");
            signatureString = order.InvoiceId + "|" + txn.TransactionId + "|" + amount + "|" + txn.TransactionCurrency;
            return CalculateSignature(signatureString, secretKey).Equals(txn.Signature);
        }
        return false;
    }

    private static string CalculateSignature(String input, String secretKey)
    {
        var hasher = new HMACSHA256(Encoding.ASCII.GetBytes(secretKey));
        var calculatedHash = hasher.ComputeHash(Encoding.ASCII.GetBytes(input));
        return BitConverter.ToString(calculatedHash).Replace("-", "").ToLower();
    }

    private static String FormatAmount(double amount)
    {
        string input = amount.ToString();
        var array = input.Split('.');
        string formattedAmount = "";
        formattedAmount += array[0];
        if (array.Length == 1)
        {
            formattedAmount += ".00";
        }
        else
        {
            string secondHalf = array[1];
            if (secondHalf.Length == 1)
            {
                formattedAmount += array[1];
                formattedAmount += ".0";
            }
            else
            {
                var counter = 0;
                formattedAmount += ".";
                foreach (var character in secondHalf)
                {
                    if (counter == 2) break;
                    formattedAmount += character;
                    counter++;
                }
            }
        }
        return formattedAmount;
    }
}