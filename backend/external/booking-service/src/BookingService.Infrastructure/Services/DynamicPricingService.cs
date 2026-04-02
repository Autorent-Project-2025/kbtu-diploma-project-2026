using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Services;

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
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        if (partnerCarId <= 0)
            throw new ArgumentException("partnerCarId must be greater than zero.");

        if (endDate <= startDate)
            throw new ArgumentException("End date must be greater than start date.");

        var partnerCar = await _partnerCarReadClient.GetByIdAsync(partnerCarId);

        if (partnerCar == null)
            throw new InvalidOperationException("Partner car not found.");

        if (partnerCar.PriceHour == null || partnerCar.PriceHour <= 0)
            throw new InvalidOperationException("Partner car hourly price is not configured.");

        var totalHours = Math.Max(1, (int)Math.Ceiling((endDate - startDate).TotalHours));
        var days = Math.Max(1, (int)Math.Ceiling(totalHours / 24.0));

        var overlappingBookings = await _db.Bookings.CountAsync(b =>
            b.PartnerCarId == partnerCarId &&
            b.StartTime < endDate &&
            b.EndTime > startDate
        );

        var demandCoefficient = GetDemandCoefficient(overlappingBookings, out var demandLevel);
        var weekendCoefficient = HasWeekend(startDate, endDate) ? 1.10m : 1.00m;
        var durationCoefficient = GetDurationCoefficient(totalHours);

        var finalPrice = Math.Round(
            partnerCar.PriceHour.Value * totalHours * demandCoefficient * weekendCoefficient * durationCoefficient,
            2,
            MidpointRounding.AwayFromZero);

        return new PricePreviewDto
        {
            PartnerCarId = partnerCarId,
            BasePricePerHour = partnerCar.PriceHour.Value,
            TotalHours = totalHours,
            Days = days,
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

    private decimal GetDurationCoefficient(int totalHours)
    {
        if (totalHours >= 24 * 7) return 0.90m;
        if (totalHours >= 24 * 3) return 0.95m;
        if (totalHours >= 24) return 0.98m;
        return 1.00m;
    }

    private bool HasWeekend(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        for (var date = startDate.Date; date < endDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday)
                return true;
        }

        return false;
    }

    private string BuildExplanation(string demandLevel, decimal weekendCoefficient, decimal durationCoefficient)
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