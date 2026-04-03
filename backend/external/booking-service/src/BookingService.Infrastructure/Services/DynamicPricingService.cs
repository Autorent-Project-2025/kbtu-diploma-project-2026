using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Application.Interfaces.Integrations;

namespace BookingService.Infrastructure.Services
{
    public class DynamicPricingService : IDynamicPricingService
    {
        private readonly IPartnerCarReadClient _partnerCarReadClient;

        public DynamicPricingService(IPartnerCarReadClient partnerCarReadClient)
        {
            _partnerCarReadClient = partnerCarReadClient;
        }

        public async Task<BookingPriceQuoteDto> CalculateQuoteAsync(
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default)
        {
            if (partnerCarId <= 0)
                throw new ArgumentException("partnerCarId must be greater than zero.");

            if (endTime <= startTime)
                throw new ArgumentException("End time must be greater than start time.");

            var context = await _partnerCarReadClient.GetPricingContextAsync(
                partnerCarId,
                startTime,
                endTime,
                cancellationToken);

            if (context is null)
                throw new KeyNotFoundException("Car not found.");

            if (context.PartnerUserId == Guid.Empty)
                throw new InvalidOperationException("Partner car owner must be a valid UUID.");

            if (!context.MarketValueKzt.HasValue || context.MarketValueKzt.Value <= 0m)
                throw new InvalidOperationException("Market value is not available for this car.");

            var rating = context.Rating <= 0m ? 3.0m : context.Rating;
            var billableHours = Math.Max(1, (int)Math.Ceiling((endTime - startTime).TotalHours));
            var daysBeforeBooking = Math.Max(0, (int)Math.Floor((startTime - DateTimeOffset.UtcNow).TotalDays));
            var ratingCoefficient = 1m + (rating - 3m) * 0.05m;
            var advanceBookingCoefficient = 1m - decimal.Min(0.2m, 0.01m * daysBeforeBooking);
            var availabilityCoefficient = decimal.Max(
                0.8m,
                decimal.Min(1.2m, 1m + (20 - context.CurrentAvailableCarsCount) * 0.02m));
            var priceHour = RoundCurrency(
                context.MarketValueKzt.Value *
                0.001m *
                ratingCoefficient *
                advanceBookingCoefficient *
                availabilityCoefficient);
            var totalPrice = RoundCurrency(priceHour * billableHours);

            return new BookingPriceQuoteDto
            {
                PartnerCarId = context.PartnerCarId,
                PartnerUserId = context.PartnerUserId,
                MarketValueKzt = RoundCurrency(context.MarketValueKzt.Value),
                Rating = RoundCurrency(rating),
                CurrentAvailableCarsCount = context.CurrentAvailableCarsCount,
                DaysBeforeBooking = daysBeforeBooking,
                BillableHours = billableHours,
                RatingCoefficient = RoundCurrency(ratingCoefficient),
                AdvanceBookingCoefficient = RoundCurrency(advanceBookingCoefficient),
                AvailabilityCoefficient = RoundCurrency(availabilityCoefficient),
                PriceHour = priceHour,
                TotalPrice = totalPrice,
                Currency = "KZT",
                IsMarketValueStale = context.IsMarketValueStale
            };
        }

        public async Task<PricePreviewDto> GetPricePreviewAsync(
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default)
        {
            var quote = await CalculateQuoteAsync(partnerCarId, startTime, endTime, cancellationToken);
            return new PricePreviewDto
            {
                PartnerCarId = quote.PartnerCarId,
                MarketValueKzt = quote.MarketValueKzt,
                Rating = quote.Rating,
                CurrentAvailableCarsCount = quote.CurrentAvailableCarsCount,
                DaysBeforeBooking = quote.DaysBeforeBooking,
                BillableHours = quote.BillableHours,
                RatingCoefficient = quote.RatingCoefficient,
                AdvanceBookingCoefficient = quote.AdvanceBookingCoefficient,
                AvailabilityCoefficient = quote.AvailabilityCoefficient,
                PriceHour = quote.PriceHour,
                FinalPrice = quote.TotalPrice,
                Currency = quote.Currency,
                IsMarketValueStale = quote.IsMarketValueStale
            };
        }

        private static decimal RoundCurrency(decimal amount)
        {
            return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }
    }
}
