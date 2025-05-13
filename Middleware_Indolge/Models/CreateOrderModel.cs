namespace Middleware_Indolge.Models
{
    public sealed class CreateOrderModel
    {

        public string Currency { get; set; }
        public decimal? PosFee { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal NetPrice { get; set; }
        public DateTime TransDate { get; set; }
        public int NumberOfItemLines { get; set; }
        public int NumberOfItems { get; set; }
        public decimal TotalTaxCharged { get; set; }
        public int NumberOfPaymentLines { get; set; }
        public string Store { get; set; }
        public int Type { get; set; }
        public string? TenderTypeId { get; set; }
        public decimal AmountCur { get; set; }
        public string ThirdPartyOrderId { get; set; }
        public DateTime BusinessDateCustom { get; set; }
        public decimal DiscAmount { get; set; }
        public decimal DiscAmountWithoutTax { get; set; }
        public string? Comment { get; set; }
        public string? Floor { get; set; }
        public string? Table { get; set; }
        public string? Server { get; set; }
        public int? Person { get; set; }
        public string TaxGroup { get; set; }
        public string? DiscountOfferId { get; set; }
        public List<SalesLine> SalesLines { get; set; }
        public string OrderSource { get; set; }
        public string? Company { get; set; }
        public string orderTime { get; set; }
        public string firstName { get; set; }
        public string? lastName { get; set; }
        public string city { get; set; }
        public string? street { get; set; }
        public string addressNo { get; set; }
        public string? secondaryAddress { get; set; }
        public string? postCode { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? entrance { get; set; }
        public string? carrierInstructions { get; set; }
        public string? cookInstructions { get; set; }
        public decimal lat { get; set; }
        public decimal lng { get; set; }
        public int? clientId { get; set; }
        public bool? isNotPaid { get; set; }
        public bool? sendMessage { get; set; }
    }

    public class SalesLine
    {
        public string ExtItemId { get; set; }
        public string? ItemId { get; set; }
        public decimal Price { get; set; }
        public decimal NetAmount { get; set; }
        public decimal NetAmountInclTax { get; set; }
        public int Qty { get; set; }
        public int LineNum { get; set; }
        public string TaxGroup { get; set; }
        public string TaxItemGroup { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ItemPriceExcTax { get; set; }
        public string? LineComment { get; set; }
        public decimal DiscAmount { get; set; }
        public decimal DiscAmountWithoutTax { get; set; }
        public string? Crust { get; set; }
        public string? Size { get; set; }
        public int side { get; set; }
        public string position { get; set; }
    }
}
