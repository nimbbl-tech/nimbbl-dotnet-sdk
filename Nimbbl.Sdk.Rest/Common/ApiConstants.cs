namespace Nimbbl.Sdk.Rest.Common;

internal static class ApiConstants
{
    // Base API URL and version
    public const string BaseUrl = "https://api.nimbbl.tech/api/";
    public const string ApiVersion = "v3";

    // Orders
    public const string OrderCreate = $"{ApiVersion}/create-order";
    public const string OrderGet = $"{ApiVersion}/order";

    // Addresses
    public const string AddressList = $"{ApiVersion}/addresses";
    public const string AddressCreate = $"{ApiVersion}/addresses";
    public const string AddressGet = $"{ApiVersion}/addresses";
    public const string AddressUpdate = $"{ApiVersion}/addresses";
    public const string AddressDelete = $"{ApiVersion}/addresses";
    public const string AddressImport = $"{ApiVersion}/addresses/import";
    public const string AddressCheckEligibility = $"{ApiVersion}/addresses/eligibility";
    public const string AddressLinkOrder = $"{ApiVersion}/addresses/link";

    // Payments
    public const string PaymentInitiate = $"{ApiVersion}/initiate-payment";
    public const string PaymentComplete = $"{ApiVersion}/payment";
    public const string PaymentResendOtp = $"{ApiVersion}/resend-otp";

    // Payment Links
    public const string PaymentLinkCreate = $"{ApiVersion}/payment-link";
    public const string PaymentLinkUpdate = $"{ApiVersion}/payment-link";
    public const string PaymentLinkEnquiry = $"{ApiVersion}/payment-link/enquiry";
    public const string PaymentLinkActions = $"{ApiVersion}/payment-link";

    // Checkout Utilities
    public const string CheckoutPaymentModes = $"{ApiVersion}/payment-modes";
    public const string CheckoutListBanks = $"{ApiVersion}/list-of-banks";
    public const string CheckoutListWallets = $"{ApiVersion}/list-of-wallets";
    public const string CheckoutListEmis = $"{ApiVersion}/emis";
    public const string CheckoutOffers = $"{ApiVersion}/offers";
    public const string CheckoutGetBinData = $"{ApiVersion}/get-bin-data";
    public const string CheckoutGetCardDetails = $"{ApiVersion}/get-card-details";
    public const string CheckoutValidateVpa = $"{ApiVersion}/validate-vpa";
    public const string CheckoutGetUpiAppDetails = $"{ApiVersion}/get-upi-app-details";

    // Transactions
    public const string TransactionEnquiry = $"{ApiVersion}/transaction-enquiry";

    // Refunds
    public const string RefundInitiate = $"{ApiVersion}/refund";

    // Auth
    public const string AuthGenerateToken = $"{ApiVersion}/generate-token";
    public const string AuthRefreshToken = $"{ApiVersion}/refresh-token";

    // HTTP Methods
    public const string HttpGet = "GET";
    public const string HttpPost = "POST";
    public const string HttpPatch = "PATCH";
    public const string HttpPut = "PUT";
    public const string HttpDelete = "DELETE";
}

