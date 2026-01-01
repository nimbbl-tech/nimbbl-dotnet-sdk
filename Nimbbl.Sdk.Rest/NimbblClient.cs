using Nimbbl.Sdk.Rest.RestClient;
using NimbblAuth = Nimbbl.Sdk.Rest.Auth.Auth;
using NimbblOrders = Nimbbl.Sdk.Rest.Orders.Orders;
using NimbblPaymentLinks = Nimbbl.Sdk.Rest.PaymentLinks.PaymentLinks;
using NimbblTransactions = Nimbbl.Sdk.Rest.Transactions.Transactions;
using NimbblRefunds = Nimbbl.Sdk.Rest.Refunds.Refunds;
using NimbblAddresses = Nimbbl.Sdk.Rest.Addresses.Addresses;
using NimbblPayments = Nimbbl.Sdk.Rest.Payments.Payments;
using NimbblCheckoutUtilities = Nimbbl.Sdk.Rest.CheckoutUtilities.CheckoutUtilities;
namespace Nimbbl.Sdk.Rest;

public class NimbblClient : IDisposable
{
    public NimbblOrders Orders { get; }
    public NimbblTransactions Transactions { get; }
    public NimbblAddresses Addresses { get; }
    public NimbblPayments Payments { get; }
    public NimbblPaymentLinks PaymentLinks { get; }
    public NimbblCheckoutUtilities CheckoutUtilities { get; }
    public NimbblRefunds Refunds { get; }
    public NimbblAuth Auth { get; }
    private readonly ApiClient _apiClient;

    public NimbblClient(string key, string secret, string baseUrl)
    {
        _apiClient = new ApiClient(key, secret, baseUrl);
        Orders = new NimbblOrders(_apiClient);
        Transactions = new NimbblTransactions(_apiClient);
        Addresses = new NimbblAddresses(_apiClient);
        Payments = new NimbblPayments(_apiClient);
        PaymentLinks = new NimbblPaymentLinks(_apiClient);
        CheckoutUtilities = new NimbblCheckoutUtilities(_apiClient);
        Refunds = new NimbblRefunds(_apiClient);
        Auth = new NimbblAuth(_apiClient);
    }

    public void AddHeader(string key, string value)
    {
        _apiClient.AddHeader(key, value);
    }

    public void SetBearerToken(string token, DateTime? expiresAtUtc = null)
    {
        _apiClient.SetBearerToken(token, expiresAtUtc);
    }

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _apiClient?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }
}