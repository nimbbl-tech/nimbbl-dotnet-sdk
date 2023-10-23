using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class TransactionTest
{
    private readonly ITransactions _transactions;

    public TransactionTest()
    {
        var config = new Config("https://api.nimbbl.tech/api/", "access_key_1MwvMkKkweorz0ry", "access_secret_81x7ByYkRpB4g05N");
        _transactions = new NimbblClient(config).Transactions;
    }

    [Fact]
    public async Task ShouldThrowForNonExistentTransaction()
    {
        var exception = await Assert.ThrowsAsync<NimbblClientException>(() => _transactions.GetAsync("random"));
        Assert.Equal(404, exception.HttpStatusCode);
        Assert.Equal("No transaction found with transaction_id = random", exception.Message);
    }

    [Fact]
    public async Task ShouldGetTransactionById()
    {
        var transaction = await _transactions.GetAsync("o_mZR7lr1eqAW4b39p-220428191537");
        AssertTransactionsMatch(ExpectedTransaction(), transaction);
    }

    [Fact]
    public async Task ShouldGetTransactionBsyOrderId()
    {
        var response = await _transactions.GetByOrderIdAsync("o_mZR7lr1eqAW4b39p");
        Assert.True(response.Success);
        AssertTransactionsMatch(ExpectedTransaction(), response.Transactions.First());
    }
    private static void AssertTransactionsMatch(TransactionResponse expected, TransactionResponse transaction)
    {
        Assert.Equal(expected.TransactionId, transaction.TransactionId);
        Assert.Equal(expected.Status, transaction.Status);
        Assert.Equal(expected.PaymentPartner, transaction.PaymentPartner);
        Assert.Equal(expected.PaymentMode, transaction.PaymentMode);
        Assert.Equal(expected.TransactionType, transaction.TransactionType);
        Assert.Equal(expected.VpaId, transaction.VpaId);
        Assert.Equal(expected.MaskedCard, transaction.MaskedCard);
        Assert.Equal(expected.HolderName, transaction.HolderName);
        Assert.Equal(expected.Issuer, transaction.Issuer);
        Assert.Equal(expected.Network, transaction.Network);
        Assert.Equal(expected.Expiry, transaction.Expiry);
        Assert.Equal(expected.RetryAllowed, transaction.RetryAllowed);
        Assert.Equal(expected.NimbblMerchantMessage, transaction.NimbblMerchantMessage);
        Assert.Equal(expected.NimbblConsumerMessage, transaction.NimbblConsumerMessage);
        Assert.Equal(expected.NimbblErrorCode, transaction.NimbblErrorCode);
        Assert.Equal(expected.RefundDone, transaction.RefundDone);
        Assert.Equal(expected.WalletName, transaction.WalletName);
        Assert.Equal(expected.BankName, transaction.BankName);
        Assert.Equal(expected.SubMerchantId, transaction.SubMerchantId);
        Assert.Equal(expected.MerchantId, transaction.MerchantId);
        Assert.Equal(expected.PspGeneratedTxnId, transaction.PspGeneratedTxnId);
        Assert.Equal(expected.WebhookSent, transaction.WebhookSent);
        Assert.Equal(expected.GrandTotalAmount, transaction.GrandTotalAmount);
        Assert.Equal(expected.AdditionalCharges, transaction.AdditionalCharges);
        Assert.Equal(expected.PspGeneratedRedirectType, transaction.PspGeneratedRedirectType);
        Assert.Equal(expected.PspGeneratedRedirect, transaction.PspGeneratedRedirect);
        Assert.Equal(expected.MerchantCallbackUrl, transaction.MerchantCallbackUrl);
        Assert.Equal(expected.VpaHolder, transaction.VpaHolder);
        Assert.Equal(expected.VpaAppName, transaction.VpaAppName);
        Assert.Equal(expected.PaytmPaymentMode, transaction.PaytmPaymentMode);
        Assert.Equal(expected.FreechargePaymentMode, transaction.FreechargePaymentMode);
    }

    private TransactionResponse ExpectedTransaction()
    {
        return new()
        {
            TransactionId = "o_mZR7lr1eqAW4b39p-220428191537",
            Status = "failed",
            PaymentPartner = "Razorpay",
            PaymentMode = "Credit Card",
            TransactionType = "payment",
            VpaId = null,
            MaskedCard = "XXXXXXXXXXXX1111",
            HolderName = "PEEEE Ceeeeee",
            Issuer = "UNKNOWN",
            Network = "VISA",
            Expiry = "09 / 24",
            RetryAllowed = false,
            NimbblMerchantMessage = "There was a problem with the payment.",
            NimbblConsumerMessage = "Sorry, your payment could not be processed. Please try again or another payment option.",
            NimbblErrorCode = "PAYMENT_ERROR",
            RefundDone = null,
            WalletName = null,
            BankName = null,
            SubMerchantId = 2,
            MerchantId = 4,
            PspGeneratedTxnId = "pay_JOt7o5CA9UhB3b",
            WebhookSent = true,
            GrandTotalAmount = 3,
            AdditionalCharges = 0,
            PspGeneratedRedirectType = "form",
            PspGeneratedRedirect = null,
            MerchantCallbackUrl = null,
            VpaHolder = null,
            VpaAppName = null,
            PaytmPaymentMode = null,
            FreechargePaymentMode = null
        };
    }
}