using BookingService.Application.DTOs;

namespace BookingService.Application.Interfaces;

public interface IDynamicPricingService
{
    Task<PricePreviewDto> GetPricePreviewAsync(
        int partnerCarId,
        DateTimeOffset startDate,
        DateTimeOffset endDate);
}