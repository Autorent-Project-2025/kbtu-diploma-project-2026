using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Services
{
    public class DynamicPricingService : IDynamicPricingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPartnerCarReadClient _partnerCarReadClient;

        public DynamicPricingService(
            ApplicationDbContext db,
            IPartnerCarReadClient partnerCarReadClient)
        {
            _db = db;
            _partnerCarReadClient = partnerCarReadClient;
        }

        public async Task<PricePreviewDto> GetPricePreviewAsync(
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default)
        {
            if (partnerCarId <= 0)
                throw new ArgumentException("partnerCarId must be greater than zero.");

            if (endTime <= startTime)
                throw new ArgumentException("End time must be greater than start time.");

            var car = await _partnerCarReadClient.GetByIdAsync(partnerCarId, cancellationToken);
            if (car is null)
                throw new Exception("Car not found.");

            if (!car.PriceHour.HasValue || car.PriceHour.Value <= 0)
                throw new Exception("Car price is not configured.");

            var totalHours = Math.Max(1, (int)Math.Ceiling((endTime - startTime).TotalHours));

            var overlappingBookings = await _db.Bookings.CountAsync(
                b => b.PartnerCarId == partnerCarId &&
                     b.StartTime < endTime &&
                     b.EndTime > startTime,
                cancellationToken);

            var demandCoefficient = GetDemandCoefficient(overlappingBookings, out var demandLevel);
            var weekendCoefficient = HasWeekend(startTime, endTime) ? 1.10m : 1.00m;
            var durationCoefficient = GetDurationCoefficient(totalHours);

            var finalPrice = Math.Round(
                car.PriceHour.Value * totalHours * demandCoefficient * weekendCoefficient * durationCoefficient,
                2);

            return new PricePreviewDto
            {
                PartnerCarId = partnerCarId,
                BasePricePerHour = car.PriceHour.Value,
                Hours = totalHours,
                DemandCoefficient = demandCoefficient,
                WeekendCoefficient = weekendCoefficient,
                DurationCoefficient = durationCoefficient,
                FinalPrice = finalPrice,
                DemandLevel = demandLevel,
                Explanation = BuildExplanation(demandLevel, weekendCoefficient, durationCoefficient)
            };
        }

        private decimal GetDemandCoefficient(int overlappingBookings, out string demandLevel)
        {
            if (overlappingBookings < 2)
            {
                demandLevel = "Low";
                return 1.00m;
            }

            if (overlappingBookings < 5)
            {
                demandLevel = "Medium";
                return 1.15m;
            }

            if (overlappingBookings < 8)
            {
                demandLevel = "High";
                return 1.30m;
            }

            demandLevel = "Peak";
            return 1.50m;
        }

        private decimal GetDurationCoefficient(int hours)
        {
            if (hours >= 24 * 7) return 0.90m;
            if (hours >= 24 * 3) return 0.95m;
            return 1.00m;
        }

        private bool HasWeekend(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            for (var date = startTime.Date; date < endTime.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Friday ||
                    date.DayOfWeek == DayOfWeek.Saturday ||
                    date.DayOfWeek == DayOfWeek.Sunday)
                {
                    return true;
                }
            }

            return false;
        }

        private string BuildExplanation(
            string demandLevel,
            decimal weekendCoefficient,
            decimal durationCoefficient)
        {
            var parts = new List<string>
            {
                $"Demand level: {demandLevel}"
            };

            if (weekendCoefficient > 1.0m)
                parts.Add("Weekend pricing applied");

            if (durationCoefficient < 1.0m)
                parts.Add("Long rental discount applied");

            return string.Join(". ", parts);
        }
    }
}