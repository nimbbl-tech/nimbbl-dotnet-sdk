namespace Nimbbl.Sdk.Rest;

public class OrderRequest
{
    public string InvoiceId { get; init; }
    public DateTime OrderDate { get; init; }
    public string Currency { get; init; }
    public long TotalAmount { get; init; }
    public long Tax { get; init; }
    public long AmountBeforeTax { get; init; }
    public string MerchantShopfrontDomain { get; init; }
    public IEnumerable<OrderLineItem> OrderLineItems { get; init; }
    public User User { get; init; }
    public ShippingAddress ShippingAddress { get; init; }
    public string Description { get; init; }
    public string ReferrerPlatform { get; init; }
    public string ReferrerPlatformIdentifier { get; init; }
}
public class ShippingAddress
{
    public string Address1 { get; init; }
    public string Street { get; init; }
    public string Landmark { get; init; }
    public string Area { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string Pincode { get; init; }
    public string AddressType { get; init; }
}