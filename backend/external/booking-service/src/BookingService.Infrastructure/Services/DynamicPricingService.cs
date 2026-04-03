using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Observability;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.Services
{
    public class DynamicPricingService : IDynamicPricingService
    {
        private readonly IPartnerCarReadClient _partnerCarReadClient;
        private readonly ILogger<DynamicPricingService> _logger;
        private readonly ObservabilityLogWriter _observabilityLogWriter;

        public DynamicPricingService(
            IPartnerCarReadClient partnerCarReadClient,
            ILogger<DynamicPricingService> logger,
            ObservabilityLogWriter observabilityLogWriter)
        {
            _partnerCarReadClient = partnerCarReadClient;
            _logger = logger;
            _observabilityLogWriter = observabilityLogWriter;
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
            {
                _logger.LogWarning(
                    "Dynamic pricing context was not found for partner car {PartnerCarId} and range {StartTime} - {EndTime}.",
                    partnerCarId,
                    startTime,
                    endTime);
                await WriteQuoteRejectedAsync(
                    "partner_car_not_found",
                    partnerCarId,
                    startTime,
                    endTime,
                    cancellationToken);
                throw new KeyNotFoundException("Car not found.");
            }

            if (context.PartnerUserId == Guid.Empty)
            {
                _logger.LogWarning(
                    "Dynamic pricing context has invalid partner user id for partner car {PartnerCarId}.",
                    partnerCarId);
                await WriteQuoteRejectedAsync(
                    "invalid_partner_user",
                    partnerCarId,
                    startTime,
                    endTime,
                    cancellationToken);
                throw new InvalidOperationException("Partner car owner must be a valid UUID.");
            }

            if (!context.MarketValueKzt.HasValue || context.MarketValueKzt.Value <= 0m)
            {
                _logger.LogWarning(
                    "Dynamic pricing cannot be calculated for partner car {PartnerCarId} because market value is unavailable or invalid. MarketValueKzt={MarketValueKzt}.",
                    partnerCarId,
                    context.MarketValueKzt);
                await WriteQuoteRejectedAsync(
                    "market_value_unavailable",
                    partnerCarId,
                    startTime,
                    endTime,
                    cancellationToken,
                    context.MarketValueKzt);
                throw new InvalidOperationException("Market value is not available for this car.");
            }

            var quotedAtUtc = DateTimeOffset.UtcNow;
            var rating = context.Rating <= 0m ? 3.0m : context.Rating;
            var billableHours = Math.Max(1, (int)Math.Ceiling((endTime - startTime).TotalHours));
            var daysBeforeBooking = Math.Max(0, (int)Math.Floor((startTime - quotedAtUtc).TotalDays));
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

            _logger.LogInformation(
                "Dynamic price calculated for partner car {PartnerCarId}: startTime={StartTime}, endTime={EndTime}, quotedAtUtc={QuotedAtUtc}, marketValueKzt={MarketValueKzt}, rating={Rating}, daysBeforeBooking={DaysBeforeBooking}, currentAvailableCarsCount={CurrentAvailableCarsCount}, ratingCoefficient={RatingCoefficient}, advanceBookingCoefficient={AdvanceBookingCoefficient}, availabilityCoefficient={AvailabilityCoefficient}, billableHours={BillableHours}, priceHour={PriceHour}, totalPrice={TotalPrice}, isMarketValueStale={IsMarketValueStale}.",
                context.PartnerCarId,
                startTime,
                endTime,
                quotedAtUtc,
                context.MarketValueKzt.Value,
                rating,
                daysBeforeBooking,
                context.CurrentAvailableCarsCount,
                ratingCoefficient,
                advanceBookingCoefficient,
                availabilityCoefficient,
                billableHours,
                priceHour,
                totalPrice,
                context.IsMarketValueStale);

            await _observabilityLogWriter.WriteAsync(new
            {
                timestamp = DateTimeOffset.UtcNow,
                service = "booking-service",
                level = "Information",
                @event = "booking_price_quote_calculated",
                partnerCarId = context.PartnerCarId,
                partnerUserId = context.PartnerUserId,
                startTime,
                endTime,
                quotedAtUtc,
                marketValueKzt = context.MarketValueKzt.Value,
                rating,
                daysBeforeBooking,
                currentAvailableCarsCount = context.CurrentAvailableCarsCount,
                ratingCoefficient,
                advanceBookingCoefficient,
                availabilityCoefficient,
                billableHours,
                priceHour,
                totalPrice,
                isMarketValueStale = context.IsMarketValueStale
            }, cancellationToken);

            return new BookingPriceQuoteDto
            {
                PartnerCarId = context.PartnerCarId,
                PartnerUserId = context.PartnerUserId,
                QuotedAtUtc = quotedAtUtc,
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
                QuotedAtUtc = quote.QuotedAtUtc,
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

        private Task WriteQuoteRejectedAsync(
            string reason,
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken,
            decimal? marketValueKzt = null)
        {
            return _observabilityLogWriter.WriteAsync(new
            {
                timestamp = DateTimeOffset.UtcNow,
                service = "booking-service",
                level = "Warning",
                @event = "booking_price_quote_rejected",
                reason,
                partnerCarId,
                startTime,
                endTime,
                marketValueKzt
            }, cancellationToken);
        }
    }
}
