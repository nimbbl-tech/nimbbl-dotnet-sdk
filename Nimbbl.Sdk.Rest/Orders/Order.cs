//Most of these fields are mapped and with no information about there value.
//Disabling null warning for now

#pragma warning disable CS8618
namespace Nimbbl.Sdk.Rest;

public class Order
{
    public IEnumerable<OrderLineItem> OrderLineItem { get; init; } = new List<OrderLineItem>();
    public SubMerchant SubMerchant { get; init; }
    public long SubMerchantId { get; init; }
    public decimal AdditionalCharges { get; init; }
    public decimal AmountBeforeTax { get; init; }
    public decimal GrandTotalAmount { get; init; }
    public decimal Tax { get; init; }
    public decimal TotalAmount { get; init; }
    public int Attempts { get; init; }
    public int MaxRetries { get; init; }
    public string BrowserName { get; init; }
    public string CallbackMode { get; init; }
    public string CallbackUrl { get; init; }
    public string CancellationReason { get; init; }
    public string Currency { get; init; }
    public string Description { get; init; }
    public string DeviceName { get; init; }
    public string DeviceUserAgent { get; init; }
    public string Fingerprint { get; init; }
    public string InvoiceId { get; init; }
    public string MerchantShopfrontDomain { get; init; }
    public string OrderDate { get; init; }
    public string OrderFromIp { get; init; }
    public string OrderId { get; init; }
    public string OrderTransacType { get; init; }
    public string OsName { get; init; }
    public string PartnerId { get; init; }
    public string ReferrerPlatform { get; init; }
    public string ReferrerPlatformVersion { get; init; }
    public string Status { get; init; }
}