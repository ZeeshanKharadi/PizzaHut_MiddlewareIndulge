using System.ComponentModel.DataAnnotations;

namespace Middleware_Indolge.Models
{
    public class UpdateOrderModel
    {
        [TenderTypeRequiredIfStatus("02", "03")]

        public string? TenderTypeId { get; set; }
        public string OrderStatus { get; set; }
    }

    public class TenderTypeRequiredIfStatusAttribute : ValidationAttribute
    {
        private readonly string[] _requiredStatuses;

        public TenderTypeRequiredIfStatusAttribute(params string[] requiredStatuses)
        {
            _requiredStatuses = requiredStatuses;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var model = (UpdateOrderModel)validationContext.ObjectInstance;
            var status = model.OrderStatus;

            if (_requiredStatuses.Contains(status) && string.IsNullOrWhiteSpace(model.TenderTypeId))
            {
                return new ValidationResult("TenderTypeId is required when OrderStatus is 02 or 03.");
            }

            return ValidationResult.Success!;
        }
    }
}
