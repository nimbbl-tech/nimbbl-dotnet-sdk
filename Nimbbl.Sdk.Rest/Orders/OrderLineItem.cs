namespace Nimbbl.Sdk.Rest;

public class OrderLineItem
{
    public string SkuId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public long Quantity { get; set; }
    public string? Uom { get; set; }
    public decimal Rate { get; set; }
    public decimal AmountBeforeTax { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public string ImageUrl { get; set; }
}