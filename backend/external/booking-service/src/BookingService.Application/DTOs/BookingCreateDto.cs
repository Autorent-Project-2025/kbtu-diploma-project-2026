using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.DTOs.Booking
{
    public class BookingCreateDto : IValidatableObject
    {
        [Range(1, int.MaxValue)]
        public int PartnerCarId { get; set; }

        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }

        public int ResolvePartnerCarId()
        {
            if (PartnerCarId <= 0)
            {
                throw new ArgumentException("PartnerCarId is required and must be greater than zero.");
            }

            return PartnerCarId;
        }

        public DateTimeOffset ResolveStartTime()
        {
            if (!StartTime.HasValue)
            {
                throw new ArgumentException("StartTime is required.");
            }

            return StartTime.Value;
        }

        public DateTimeOffset ResolveEndTime()
        {
            if (!EndTime.HasValue)
            {
                throw new ArgumentException("EndTime is required.");
            }

            return EndTime.Value;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PartnerCarId <= 0)
            {
                yield return new ValidationResult(
                    "PartnerCarId is required and must be greater than zero.",
                    new[] { nameof(PartnerCarId) });
            }

            if (!StartTime.HasValue)
            {
                yield return new ValidationResult("StartTime is required.", new[] { nameof(StartTime) });
            }

            if (!EndTime.HasValue)
            {
                yield return new ValidationResult("EndTime is required.", new[] { nameof(EndTime) });
            }

            if (StartTime.HasValue && EndTime.HasValue && EndTime.Value <= StartTime.Value)
            {
                yield return new ValidationResult(
                    "EndTime must be greater than StartTime.",
                    new[] { nameof(EndTime) });
            }
        }
    }
}
