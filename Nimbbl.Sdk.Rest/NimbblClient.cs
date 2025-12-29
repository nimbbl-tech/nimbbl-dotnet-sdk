using Nimbbl.Sdk.Rest.RestClient;
using NimbblAuth = Nimbbl.Sdk.Rest.Auth.Auth;
namespace Nimbbl.Sdk.Rest;

public class NimbblClient : IDisposable
{
    public Orders Orders { get; }
    public Transactions Transactions { get; }
    public Addresses Addresses { get; }
    public Payments Payments { get; }
    public PaymentLinks PaymentLinks { get; }
    public CheckoutUtilities CheckoutUtilities { get; }
    public Refunds Refunds { get; }
    public NimbblAuth Auth { get; }
    // Users API removed - not an official public API
    private readonly ApiClient _apiClient;

    public NimbblClient(string key, string secret, string baseUrl, Action<string, string>? logAction = null)
    {
        _apiClient = new ApiClient(key, secret, baseUrl, logAction);
        Orders = new Orders(_apiClient);
        Transactions = new Transactions(_apiClient);
        Addresses = new Addresses(_apiClient);
        Payments = new Payments(_apiClient);
        PaymentLinks = new PaymentLinks(_apiClient);
        CheckoutUtilities = new CheckoutUtilities(_apiClient);
        Refunds = new Refunds(_apiClient);
        Auth = new NimbblAuth(_apiClient);
        // Users API removed - not an official public API
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
        GC.SuppressFinalize(this);
    }
}