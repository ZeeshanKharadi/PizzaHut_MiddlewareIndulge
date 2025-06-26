using System.ComponentModel.DataAnnotations;

namespace Middleware_Indolge.Models
{
    public class RequiredIfDeliveryAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Access the entire model to get the 'orderChannel' property
            var model = validationContext.ObjectInstance;
            var orderChannelProperty = model.GetType().GetProperty("orderChannel");
            var orderChannelValue = orderChannelProperty?.GetValue(model, null) as string;

            // Check if 'orderChannel' is "delivery" and 'addressNo' is null or empty
            if (orderChannelValue.ToLower() == "delivery" && string.IsNullOrEmpty(value?.ToString()))
            {
                return new ValidationResult("Address is required for delivery orders.", new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
    public sealed class CreateOrderModel
    {
        [Required]
        public string Currency { get; set; }
        public decimal? PosFee { get; set; }

        [Required]
        public decimal GrossAmount { get; set; }

        [Required]
        public decimal NetAmount { get; set; }

        [Required]
        public decimal NetPrice { get; set; }

        [Required]
        public DateTime TransDate { get; set; }

        [Required]
        public int NumberOfItemLines { get; set; }

        [Required]
        public int NumberOfItems { get; set; }

        [Required]
        public decimal TotalTaxCharged { get; set; }

        [Required]
        public int NumberOfPaymentLines { get; set; }

        [Required]
        public string Store { get; set; }

        [Required]
        public int Type { get; set; }
       

        public string? TenderTypeId { get; set; }

        [Required]
        public decimal AmountCur { get; set; }

        [Required]
        public string ThirdPartyOrderId { get; set; }

        public DateTime? BusinessDateCustom { get; set; }

        [Required]
        public decimal DiscAmount { get; set; }

        [Required]
        public decimal DiscAmountWithoutTax { get; set; }
        public string? Comment { get; set; }
        public string? Floor { get; set; }
        public string? Table { get; set; }
        public string? Server { get; set; }
        public int? Person { get; set; }

        
        public string? TaxGroup { get; set; }
        public string? DiscountOfferId { get; set; }

        [Required]
        public List<SalesLine> SalesLines { get; set; }

        [Required]
        public string OrderSource { get; set; }
        public string? Company { get; set; }

        [Required]
        public string orderTime { get; set; }

        [Required]
        public string firstName { get; set; }
        public string? lastName { get; set; }
        
        [Required]
        public string city { get; set; }
        public string? street { get; set; }

      
        public string? secondaryAddress { get; set; }
        public string? postCode { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? entrance { get; set; }
        public string? carrierInstructions { get; set; }
        public string? cookInstructions { get; set; }

        [Required]
        public string orderChannel { get; set; }
        [RequiredIfDelivery]
        public string? addressNo { get; set; }

        [Required]
        public decimal lat { get; set; }
        
        [Required]
        public decimal lng { get; set; }

        public int? clientId { get; set; }
        public bool? isNotPaid { get; set; }
        public bool? sendMessage { get; set; }
    }

    public class SalesLine
    {
        [Required]
        public string ExtItemId { get; set; }
        public string? ItemId { get; set; }

        [Required]
        public decimal Price { get; set; }
        
        [Required]
        public decimal NetAmount { get; set; }

        [Required]
        public decimal NetAmountInclTax { get; set; }
        
        [Required]
        public int Qty { get; set; }

        [Required]
        public int LineNum { get; set; }

        
        public string? TaxGroup { get; set; }

       
        public string? TaxItemGroup { get; set; }
      
        [Required]
        public decimal TaxAmount { get; set; }
        
        [Required]
        public decimal ItemPriceExcTax { get; set; }

        public string? LineComment { get; set; }

        [Required]
        public decimal DiscAmount { get; set; }
 
        [Required]
        public decimal DiscAmountWithoutTax { get; set; }
        public string? Crust { get; set; }
        public string? Size { get; set; }
        
        [Required]
        public int side { get; set; }

        [Required]
        public string position { get; set; }
    }
}
