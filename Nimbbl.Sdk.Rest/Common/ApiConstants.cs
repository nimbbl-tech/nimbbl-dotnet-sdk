namespace Nimbbl.Sdk.Rest.Common;

internal static class ApiConstants
{
    // Base API URL and version
    public const string ApiPath = "/api/";
    public const string BaseUrl = "https://api.nimbbl.tech" + ApiPath;
    public const string ApiVersion = "v3";

    // Shared base paths (to avoid duplication)
    private const string AddressesBase = $"{ApiVersion}/addresses";
    private const string PaymentLinkBase = $"{ApiVersion}/payment-link";

    // Orders
    public const string OrderCreate = $"{ApiVersion}/create-order";
    public const string OrderGet = $"{ApiVersion}/order";

    // Addresses
    public const string AddressList = AddressesBase;
    public const string AddressCreate = AddressesBase;
    public const string AddressGet = AddressesBase;
    public const string AddressUpdate = AddressesBase;
    public const string AddressDelete = AddressesBase;
    public const string AddressImport = $"{AddressesBase}/import";
    public const string AddressCheckEligibility = $"{AddressesBase}/eligibility";
    public const string AddressLinkOrder = $"{AddressesBase}/link";

    // Payments
    public const string PaymentInitiate = $"{ApiVersion}/initiate-payment";
    public const string PaymentComplete = $"{ApiVersion}/payment";
    public const string PaymentResendOtp = $"{ApiVersion}/resend-otp";

    // Payment Links
    public const string PaymentLinkCreate = PaymentLinkBase;
    public const string PaymentLinkUpdate = PaymentLinkBase;
    public const string PaymentLinkEnquiry = $"{PaymentLinkBase}/enquiry";
    public const string PaymentLinkActions = PaymentLinkBase;

    // Checkout Utilities
    public const string CheckoutPaymentModes = $"{ApiVersion}/payment-modes";
    public const string CheckoutListBanks = $"{ApiVersion}/list-of-banks";
    public const string CheckoutListWallets = $"{ApiVersion}/list-of-wallets";
    public const string CheckoutListEmis = $"{ApiVersion}/emis";
    public const string CheckoutOffers = $"{ApiVersion}/offers";
    public const string CheckoutGetBinData = $"{ApiVersion}/get-bin-data";
    public const string CheckoutGetCardDetails = $"{ApiVersion}/cards";
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

    // Token expiration threshold (in minutes)
    // Tokens are considered expired if they will expire within this threshold
    // Calculation: Default token expiration is 20 minutes
    // - 1 minute deducted for client HTTP timeout buffer
    // - 1 minute deducted for server timeout buffer
    // Result: 20 - 1 - 1 = 18 minutes
    public const int TokenExpirationThresholdMinutes = 18;

    // HTTP client timeout (in seconds)
    // Default timeout for all HTTP requests (read/write operations)
    public const int DefaultHttpTimeoutSeconds = 60; // 1 minute

    // Retry configuration
    // Number of retry attempts for failed requests (1 = 1 retry = 2 total attempts)
    // When authentication failure (401/403) is detected, tokens are cleared and request is retried
    public const ushort DefaultRetryCount = 1;
}

