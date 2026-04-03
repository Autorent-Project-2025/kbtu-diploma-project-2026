using CarService.Application.DTOs.Recommendation;
using CarService.Application.Interfaces;
using CarService.Domain.Entities;
using CarService.Domain.Enums;
using CarService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services;

public class CarRecommendationService : ICarRecommendationService
{
    private readonly ApplicationDbContext _db;

    public CarRecommendationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<RecommendedPartnerCarDto>> GetRecommendationsAsync(
        RecommendationQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var partnerCars = await _db.PartnerCars
            .AsNoTracking()
            .Include(pc => pc.CarModel)
                .ThenInclude(c => c.Brand)
            .Include(pc => pc.CarModel)
                .ThenInclude(c => c.ModelLookup)
            .Include(pc => pc.Images)
            .Where(pc =>
                pc.Status == PartnerCarStatus.Available &&
                pc.PriceHour != null &&
                pc.PriceHour > 0)
            .ToListAsync(cancellationToken);

        var result = partnerCars
            .Select(pc => new
            {
                PartnerCar = pc,
                Score = CalculateScore(pc, query),
                ReasonTag = BuildReasonTag(pc, query)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.PartnerCar.PriceHour ?? decimal.MaxValue)
            .Take(6)
            .Select(x => new RecommendedPartnerCarDto
            {
                PartnerCarId = x.PartnerCar.Id,
                CarModelId = x.PartnerCar.CarModelId,
                Brand = x.PartnerCar.CarModel.Brand.Name,
                Model = x.PartnerCar.CarModel.ModelLookup.Name,
                Year = x.PartnerCar.CarModel.Year,
                PriceHour = x.PartnerCar.PriceHour,
                PriceDay = x.PartnerCar.PriceDay,
                Seats = x.PartnerCar.CarModel.Seats,
                Transmission = x.PartnerCar.CarModel.Transmission,
                Rating = x.PartnerCar.Rating ?? x.PartnerCar.CarModel.Rating,
                Score = x.Score,
                ReasonTag = x.ReasonTag,
                ImageUrl = x.PartnerCar.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToList();

        return result;
    }

    private decimal CalculateScore(PartnerCar pc, RecommendationQueryDto query)
    {
        decimal score = 0;

        if (query.MaxBudgetPerHour.HasValue && pc.PriceHour.HasValue)
        {
            if (pc.PriceHour.Value <= query.MaxBudgetPerHour.Value)
                score += 30;
            else if (pc.PriceHour.Value <= query.MaxBudgetPerHour.Value * 1.2m)
                score += 10;
        }

        if (query.Passengers.HasValue && pc.CarModel.Seats.HasValue)
        {
            if (pc.CarModel.Seats.Value >= query.Passengers.Value)
                score += 20;
        }

        if (!string.IsNullOrWhiteSpace(query.Transmission) &&
            !string.IsNullOrWhiteSpace(pc.CarModel.Transmission) &&
            pc.CarModel.Transmission.Equals(query.Transmission, StringComparison.OrdinalIgnoreCase))
        {
            score += 15;
        }

        if (!string.IsNullOrWhiteSpace(query.TripPurpose))
        {
            score += GetTripPurposeScore(pc, query.TripPurpose);
        }

        var rating = pc.Rating ?? pc.CarModel.Rating;
        if (rating.HasValue)
        {
            score += Math.Min(rating.Value * 3, 15);
        }

        return score;
    }

    private decimal GetTripPurposeScore(PartnerCar pc, string tripPurpose)
    {
        var purpose = tripPurpose.Trim().ToLowerInvariant();

        return purpose switch
        {
            "city" when pc.PriceHour is not null && pc.PriceHour.Value <= 4000 => 20,
            "family" when pc.CarModel.Seats is not null && pc.CarModel.Seats.Value >= 5 => 20,
            "business" when pc.CarModel.Year >= 2020 => 20,
            "luxury" when pc.PriceHour is not null && pc.PriceHour.Value >= 8000 => 20,
            "travel" when pc.CarModel.Seats is not null && pc.CarModel.Seats.Value >= 4 => 20,
            _ => 5
        };
    }

    private string BuildReasonTag(PartnerCar pc, RecommendationQueryDto query)
    {
        if (query.MaxBudgetPerHour.HasValue &&
            pc.PriceHour.HasValue &&
            pc.PriceHour.Value <= query.MaxBudgetPerHour.Value)
        {
            return "Best budget fit";
        }

        if (query.Passengers.HasValue &&
            pc.CarModel.Seats.HasValue &&
            pc.CarModel.Seats.Value >= query.Passengers.Value)
        {
            return "Enough seats";
        }

        if (!string.IsNullOrWhiteSpace(query.Transmission) &&
            !string.IsNullOrWhiteSpace(pc.CarModel.Transmission) &&
            pc.CarModel.Transmission.Equals(query.Transmission, StringComparison.OrdinalIgnoreCase))
        {
            return "Preferred transmission";
        }

        var rating = pc.Rating ?? pc.CarModel.Rating;
        if (rating.HasValue && rating.Value >= 4.5m)
        {
            return "Top rated";
        }

        return "Recommended match";
    }
}
