using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.DTOs.Booking
{
    public class BookingCreateDto : IValidatableObject
    {
        [Range(1, int.MaxValue)]
        public int CarId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate == default)
            {
                yield return new ValidationResult("StartDate is required.", new[] { nameof(StartDate) });
            }

            if (EndDate == default)
            {
                yield return new ValidationResult("EndDate is required.", new[] { nameof(EndDate) });
            }

            if (StartDate != default && EndDate != default && EndDate <= StartDate)
            {
                yield return new ValidationResult("EndDate must be greater than StartDate.", new[] { nameof(EndDate) });
            }
        }
    }
}
