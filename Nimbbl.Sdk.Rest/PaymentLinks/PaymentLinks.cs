using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;
using Nimbbl.Sdk.Rest.Exception;

namespace Nimbbl.Sdk.Rest;

public class PaymentLinks
{
    private readonly ApiClient _apiClient;
    
    internal PaymentLinks(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private void ValidatePaymentLinkIdentifier(Dictionary<string, object?> attributes)
    {
        var hasInvoiceId = attributes.TryGetValue("invoice_id", out var invoiceId) && 
                          invoiceId != null && 
                          !string.IsNullOrWhiteSpace(invoiceId.ToString());
        var hasPaymentLinkId = attributes.TryGetValue("payment_link_id", out var paymentLinkId) && 
                              paymentLinkId != null && 
                              !string.IsNullOrWhiteSpace(paymentLinkId.ToString());
        
        if (!hasInvoiceId && !hasPaymentLinkId)
        {
            throw new NimbblException(
                ErrorMessages.IdentifierRequired,
                400,
                "IDENTIFIER_REQUIRED"
            );
        }
    }

    public Task<JsonElement> CreatePaymentLinkAsync(Dictionary<string, object?> request)
    {
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentLinkCreate, request);
    }

    public Task<JsonElement> UpdatePaymentLinkAsync(Dictionary<string, object?> request)
    {
        // Validate that either invoice_id or payment_link_id is provided
        ValidatePaymentLinkIdentifier(request);
        
        return _apiClient.PatchWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentLinkUpdate, request);
    }

    public Task<JsonElement> EnquiryPaymentLinkAsync(Dictionary<string, object?> request)
    {
        // Validate that either invoice_id or payment_link_id is provided
        ValidatePaymentLinkIdentifier(request);
        
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(ApiConstants.PaymentLinkEnquiry, request);
    }

    public Task<JsonElement> PerformPaymentLinkActionsAsync(Dictionary<string, object?> request)
    {
        // Validate that either invoice_id or payment_link_id is provided
        ValidatePaymentLinkIdentifier(request);
        
        // Validate that action is provided
        if (!request.TryGetValue("action", out var actionObj) || 
            actionObj == null || 
            string.IsNullOrWhiteSpace(actionObj.ToString()))
        {
            throw new NimbblException(
                ErrorMessages.ActionRequired,
                400,
                "ACTION_REQUIRED"
            );
        }
        
        // Validate action value
        var action = actionObj.ToString()!.Trim();
        if (action != "send" && action != "cancel")
        {
            throw new NimbblException(
                ErrorMessages.ActionInvalid,
                400,
                "INVALID_ACTION"
            );
        }
        
        var endpoint = $"{ApiConstants.PaymentLinkActions}/actions";
        return _apiClient.PostWithAuth<Dictionary<string, object?>, JsonElement>(endpoint, request);
    }
}

