using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.RestClient;

namespace Nimbbl.Sdk.Rest.CheckoutUtilities;

public class CheckoutUtilities : BaseService
{
    internal CheckoutUtilities(ApiClient apiClient) : base(apiClient)
    {
    }

    /// <summary>
    /// List available payment modes for an order.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters including order_id (required), and optionally os and upi_app_package_names</param>
    /// <returns>JSON response containing available payment modes</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/list-of-payment-modes-v-3/">List Payment Modes API</see> for more details.</remarks>
    public Task<JsonElement> ListPaymentModesAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutPaymentModes, request);
    }

    /// <summary>
    /// List available banks for net banking.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <returns>JSON response containing list of available banks</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/list-of-banks-v-3/">List Banks API</see> for more details.</remarks>
    public Task<JsonElement> ListBanksAsync(Dictionary<string, object?> request)
    {
        var isEncryptEnabled = ApiClient.IsEncryptPayloadEnabled();
        Logger.DebugWithCaller($"ListBanksAsync - Encryption enabled: {isEncryptEnabled}");

        if (isEncryptEnabled)
        {
            try
            {
                Logger.DebugWithCaller($"ListBanksAsync - {ErrorMessages.LogStartingPayloadEncryption}");
                var encryption = new Encryption(ApiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(request);

                request = new Dictionary<string, object?>
                {
                    [JsonKeys.EncryptedPayload] = encryptedPayload
                };

                Logger.InfoWithCaller($"List banks {ErrorMessages.LogPayloadEncryptedSuccessfully}");
            }
            catch (System.Exception ex)
            {
                Logger.ExceptionWithCaller(string.Format(ErrorMessages.EncryptionErrorFormat, "list banks", ex.Message), ex);
                throw new NimbblException(string.Format(ErrorMessages.EncryptionErrorFormat, "list banks", ex.Message), 500, ErrorCodes.EncryptionError);
            }
        }
        else
        {
            Logger.DebugWithCaller($"ListBanksAsync - {ErrorMessages.LogEncryptionDisabled}");
        }

        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutListBanks, request);
    }

    /// <summary>
    /// List available wallets for payment.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <returns>JSON response containing list of available wallets</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/list-of-wallets-v-3/">List Wallets API</see> for more details.</remarks>
    public Task<JsonElement> ListWalletsAsync(Dictionary<string, object?> request)
    {
        var isEncryptEnabled = ApiClient.IsEncryptPayloadEnabled();
        Logger.DebugWithCaller($"ListWalletsAsync - Encryption enabled: {isEncryptEnabled}");

        if (isEncryptEnabled)
        {
            try
            {
                Logger.DebugWithCaller($"ListWalletsAsync - {ErrorMessages.LogStartingPayloadEncryption}");
                var encryption = new Encryption(ApiClient.GetConfigSecret());
                var encryptedPayload = encryption.Encrypt(request);

                request = new Dictionary<string, object?>
                {
                    [JsonKeys.EncryptedPayload] = encryptedPayload
                };

                Logger.InfoWithCaller($"List wallets {ErrorMessages.LogPayloadEncryptedSuccessfully}");
            }
            catch (System.Exception ex)
            {
                Logger.ExceptionWithCaller(string.Format(ErrorMessages.EncryptionErrorFormat, "list wallets", ex.Message), ex);
                throw new NimbblException(string.Format(ErrorMessages.EncryptionErrorFormat, "list wallets", ex.Message), 500, ErrorCodes.EncryptionError);
            }
        }
        else
        {
            Logger.DebugWithCaller($"ListWalletsAsync - {ErrorMessages.LogEncryptionDisabled}");
        }

        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutListWallets, request);
    }

    /// <summary>
    /// List available EMI options.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <returns>JSON response containing list of available EMI options</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/list-of-em-is-v-3/">List EMIs API</see> for more details.</remarks>
    public Task<JsonElement> ListEmisAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutListEmis, request);
    }

    /// <summary>
    /// Get available offers for an order.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <returns>JSON response containing available offers</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/offers-v-3/">Get Offers API</see> for more details.</remarks>
    public Task<JsonElement> GetOffersAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutOffers, request);
    }

    /// <summary>
    /// Get card BIN data for a card number.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters including card_bin</param>
    /// <returns>JSON response containing card BIN data</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-card-bin-data-v-3/">Get Card BIN Data API</see> for more details.</remarks>
    public Task<JsonElement> GetCardBinDataAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutGetBinData, request);
    }

    /// <summary>
    /// Get card details using encrypted card data.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters including encrypted card_details</param>
    /// <returns>JSON response containing card details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-card-details-v-3/">Get Card Details API</see> for more details.</remarks>
    public Task<JsonElement> GetCardDetailsAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutGetCardDetails, request);
    }

    /// <summary>
    /// Validate a UPI VPA (Virtual Payment Address).
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters including upi_id</param>
    /// <returns>JSON response containing VPA validation result</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/validate-upi-vpa-v-3/">Validate UPI VPA API</see> for more details.</remarks>
    public Task<JsonElement> ValidateUpiVpaAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutValidateVpa, request);
    }

    /// <summary>
    /// Get UPI app details.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <returns>JSON response containing UPI app details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/get-upi-app-details-v-3/">Get UPI App Details API</see> for more details.</remarks>
    public Task<JsonElement> GetUpiAppDetailsAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.CheckoutGetUpiAppDetails, request);
    }
}

