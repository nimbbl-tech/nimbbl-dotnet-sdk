namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Constants for checkout payment modes and option keys
/// </summary>
public static class CheckoutConstants
{
    // Payment Mode Codes
    public const string PaymentModeNetBanking = "net_banking";
    public const string PaymentModeWallet = "wallet";
    public const string PaymentModeUpi = "upi";
    public const string PaymentModeEmi = "emi";
    public const string PaymentModeAll = "allpayment";

    // Checkout Modes
    public const string CheckoutModePopup = "popup";
    public const string CheckoutModeRedirect = "redirect";

    // Checkout Option Keys
    public const string OptionKeyBankCode = "bank_code";
    public const string OptionKeyWalletCode = "wallet_code";
    public const string OptionKeyPaymentFlow = "payment_flow";
    public const string OptionKeyEmiCode = "emi_code";
    public const string OptionKeyPaymentModeCode = "payment_mode_code";
    public const string OptionKeyCallbackUrl = "callback_url";
    public const string OptionKeyCallbackHandler = "callback_handler";
    public const string OptionKeyCallbackHandlerJs = "callback_handler_js";
    public const string OptionKeyHandlerPostUrl = "handler_post_url";
    public const string OptionKeyUpiId = "upi_id";
    public const string OptionKeyUpiAppCode = "upi_app_code";

    // Checkout Config Keys
    public const string ConfigKeyToken = "token";
    public const string ConfigKeyApiHost = "apiHost";
    public const string ConfigKeyCheckoutHost = "checkoutHost";

    // Default Values
    public const string DefaultMode = CheckoutModePopup;
    public const string DefaultHandlerPostUrl = "handle-callback";
    public const string DefaultCallbackRoute = "payment-callback";
}

