using System.ComponentModel.DataAnnotations;

namespace CarService.Application.Attributes
{
    public class EnumNameAttribute : ValidationAttribute
    {
        private readonly Type _enumType;

        public EnumNameAttribute(Type enumType)
        {
            _enumType = enumType;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (Enum.IsDefined(_enumType, value))
            {
                return ValidationResult.Success;
            }

            var allowedValues = string.Join(", ", Enum.GetNames(_enumType));

            return new ValidationResult($"Type '{value}' is invalid. Allowed image types: {allowedValues}");
        }
    }
}
