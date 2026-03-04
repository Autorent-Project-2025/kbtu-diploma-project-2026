using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.DTOs.Booking
{
    public class BookingCreateDto : IValidatableObject
    {
        public int? PartnerCarId { get; set; }
        public int? CarId { get; set; }
        public Guid PartnerId { get; set; }
        public decimal? PriceHour { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int ResolvePartnerCarId()
        {
            var value = PartnerCarId ?? CarId;
            if (!value.HasValue || value.Value <= 0)
            {
                throw new ArgumentException("PartnerCarId is required and must be greater than zero.");
            }

            return value.Value;
        }

        public DateTimeOffset ResolveStartTime()
        {
            var value = ResolveStartTimeCore();
            if (!value.HasValue)
            {
                throw new ArgumentException("StartTime is required.");
            }

            return value.Value;
        }

        public DateTimeOffset ResolveEndTime()
        {
            var value = ResolveEndTimeCore();
            if (!value.HasValue)
            {
                throw new ArgumentException("EndTime is required.");
            }

            return value.Value;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var resolvedCarId = PartnerCarId ?? CarId;
            if (!resolvedCarId.HasValue || resolvedCarId.Value <= 0)
            {
                yield return new ValidationResult(
                    "PartnerCarId is required and must be greater than zero.",
                    new[] { nameof(PartnerCarId), nameof(CarId) });
            }

            if (PartnerId == Guid.Empty)
            {
                yield return new ValidationResult("PartnerId is required.", new[] { nameof(PartnerId) });
            }

            if (PriceHour.HasValue && PriceHour.Value <= 0)
            {
                yield return new ValidationResult("PriceHour must be greater than zero.", new[] { nameof(PriceHour) });
            }

            var resolvedStart = ResolveStartTimeCore();
            if (!resolvedStart.HasValue)
            {
                yield return new ValidationResult(
                    "StartTime is required.",
                    new[] { nameof(StartTime), nameof(StartDate) });
            }

            var resolvedEnd = ResolveEndTimeCore();
            if (!resolvedEnd.HasValue)
            {
                yield return new ValidationResult(
                    "EndTime is required.",
                    new[] { nameof(EndTime), nameof(EndDate) });
            }

            if (resolvedStart.HasValue && resolvedEnd.HasValue && resolvedEnd.Value <= resolvedStart.Value)
            {
                yield return new ValidationResult(
                    "EndTime must be greater than StartTime.",
                    new[] { nameof(EndTime), nameof(EndDate) });
            }
        }

        private DateTimeOffset? ResolveStartTimeCore()
        {
            if (StartTime.HasValue)
            {
                return StartTime.Value;
            }

            if (StartDate.HasValue)
            {
                return NormalizeLegacyDateTime(StartDate.Value);
            }

            return null;
        }

        private DateTimeOffset? ResolveEndTimeCore()
        {
            if (EndTime.HasValue)
            {
                return EndTime.Value;
            }

            if (EndDate.HasValue)
            {
                return NormalizeLegacyDateTime(EndDate.Value);
            }

            return null;
        }

        private static DateTimeOffset NormalizeLegacyDateTime(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => new DateTimeOffset(value),
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            };
        }
    }
}
