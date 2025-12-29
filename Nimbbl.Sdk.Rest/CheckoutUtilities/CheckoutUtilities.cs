using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;

namespace Nimbbl.Sdk.Rest;

public class CheckoutUtilities
{
    private readonly ApiClient _apiClient;
    
    internal CheckoutUtilities(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<JsonElement> ListPaymentModesAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutPaymentModes, request);
    }

    /// <summary>
    /// List available banks for net banking.
    /// Lists available banks for net banking.
    /// </summary>
    public Task<JsonElement> ListBanksAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutListBanks, request);
    }

    public Task<JsonElement> ListWalletsAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutListWallets, request);
    }

    public Task<JsonElement> ListEmisAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutListEmis, request);
    }

    public Task<JsonElement> GetOffersAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutOffers, request);
    }

    public Task<JsonElement> GetCardBinDataAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutGetBinData, request);
    }

    public Task<JsonElement> GetCardDetailsAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutGetCardDetails, request);
    }

    public Task<JsonElement> ValidateUpiVpaAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutValidateVpa, request);
    }

    public Task<JsonElement> GetUpiAppDetailsAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutGetUpiAppDetails, request);
    }
}

