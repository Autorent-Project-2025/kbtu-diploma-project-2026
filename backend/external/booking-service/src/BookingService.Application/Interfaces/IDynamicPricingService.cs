using BookingService.Application.DTOs;

namespace BookingService.Application.Interfaces;

public interface IDynamicPricingService
{
    Task<BookingPriceQuoteDto> CalculateQuoteAsync(
        int partnerCarId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    Task<PricePreviewDto> GetPricePreviewAsync(
        int partnerCarId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);
}
