namespace CarService.Domain.Calculations
{
    public static class PartnerCarDisplayPriceCalculator
    {
        private const decimal DefaultRating = 3.0m;
        private const decimal HourlyBaseFactor = 0.0001m;
        private const decimal DailyDiscountFactor = 0.90m;

        public static (decimal? PriceHour, decimal? PriceDay) Calculate(
            decimal? marketValueKzt,
            decimal? rating)
        {
            if (!marketValueKzt.HasValue || marketValueKzt.Value <= 0m)
            {
                return (null, null);
            }

            var effectiveRating = rating ?? DefaultRating;
            var ratingCoefficient = 1m + (effectiveRating - DefaultRating) * 0.05m;
            var hourlyPrice = RoundCurrency(marketValueKzt.Value * HourlyBaseFactor * ratingCoefficient);
            var dailyPrice = RoundCurrency(hourlyPrice * 24m * DailyDiscountFactor);

            return (hourlyPrice, dailyPrice);
        }

        private static decimal RoundCurrency(decimal amount)
        {
            return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }
    }
}
