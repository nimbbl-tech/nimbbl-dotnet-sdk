using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
using Nimbbl.Sdk.Rest.Exception;
using static Nimbbl.Sdk.Rest.Common.ErrorCodes;
using static Nimbbl.Sdk.Rest.Common.HttpStatusCodes;

namespace Nimbbl.Sdk.Rest.PaymentLinks;

public class PaymentLinks : BaseService
{
    internal PaymentLinks(ApiClient apiClient) : base(apiClient)
    {
    }

    private static void ValidatePaymentLinkIdentifier(Dictionary<string, object?> attributes)
    {
        var hasInvoiceId = attributes.TryGetValue(JsonKeys.InvoiceId, out var invoiceId) && 
                          invoiceId != null && 
                          !string.IsNullOrWhiteSpace(invoiceId.ToString());
        var hasPaymentLinkId = attributes.TryGetValue(JsonKeys.PaymentLinkId, out var paymentLinkId) && 
                              paymentLinkId != null && 
                              !string.IsNullOrWhiteSpace(paymentLinkId.ToString());
        
        if (!hasInvoiceId && !hasPaymentLinkId)
        {
            throw new NimbblException(
                ErrorMessages.IdentifierRequired,
                BadRequest,
                IdentifierRequired
            );
        }
    }

    /// <summary>
    /// Create a new payment link.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Payment link creation request parameters</param>
    /// <returns>JSON response containing payment link details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/create-a-payment-link-v-3/">Create Payment Link API</see> for more details.</remarks>
    public Task<JsonElement> CreatePaymentLinkAsync(Dictionary<string, object?> request)
    {
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentLinkCreate, request);
    }

    /// <summary>
    /// Update an existing payment link.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Payment link update request parameters (must include invoice_id or payment_link_id)</param>
    /// <returns>JSON response containing updated payment link details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/update-a-payment-link-v-3/">Update Payment Link API</see> for more details.</remarks>
    public Task<JsonElement> UpdatePaymentLinkAsync(Dictionary<string, object?> request)
    {
        // Validate that either invoice_id or payment_link_id is provided
        ValidatePaymentLinkIdentifier(request);
        
        return ApiClient.Patch<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentLinkUpdate, request);
    }

    /// <summary>
    /// Get payment link details by invoice ID or payment link ID.
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Enquiry request parameters (must include invoice_id or payment_link_id)</param>
    /// <returns>JSON response containing payment link details</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/payment-link-enquiry-v-3/">Payment Link Enquiry API</see> for more details.</remarks>
    public Task<JsonElement> EnquiryPaymentLinkAsync(Dictionary<string, object?> request)
    {
        // Validate that either invoice_id or payment_link_id is provided
        ValidatePaymentLinkIdentifier(request);
        
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentLinkEnquiry, request);
    }

    /// <summary>
    /// Perform actions on a payment link (send or cancel).
    /// Merchant token is automatically generated and used for authentication.
    /// </summary>
    /// <param name="request">Action request parameters (must include invoice_id or payment_link_id, and action: "send" or "cancel")</param>
    /// <returns>JSON response containing action result</returns>
    /// <remarks>See <see href="https://nimbbl.biz/docs/api-reference/payment-link-actions-v-3/">Payment Link Actions API</see> for more details.</remarks>
    public Task<JsonElement> PerformPaymentLinkActionsAsync(Dictionary<string, object?> request)
    {
        // Validate that either invoice_id or payment_link_id is provided
        ValidatePaymentLinkIdentifier(request);
        
        // Validate that action is provided
        if (!request.TryGetValue(JsonKeys.Action, out var actionObj) || 
            actionObj == null || 
            string.IsNullOrWhiteSpace(actionObj.ToString()))
        {
            throw new NimbblException(
                ErrorMessages.ActionRequired,
                BadRequest,
                ActionRequired
            );
        }
        
        // Validate action value
        var action = actionObj.ToString()!.Trim();
        if (action != ErrorMessages.ActionSend && action != ErrorMessages.ActionCancel)
        {
            throw new NimbblException(
                ErrorMessages.ActionInvalid,
                BadRequest,
                InvalidAction
            );
        }
        
        var endpoint = $"{ApiConstants.PaymentLinkActions}/actions";
        return ApiClient.Post<Dictionary<string, object?>, JsonElement>(endpoint, request);
    }
}

