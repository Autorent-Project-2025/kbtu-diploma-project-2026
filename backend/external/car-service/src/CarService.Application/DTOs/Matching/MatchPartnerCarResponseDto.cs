namespace CarService.Application.DTOs.Matching
{
    public sealed class MatchPartnerCarResponseDto
    {
        public bool IsAvailable { get; init; }
        public int? PartnerCarId { get; init; }
        public Guid? PartnerId { get; init; }
        public decimal? PriceHour { get; init; }
        public string? ModelBrand { get; init; }
        public string? ModelName { get; init; }
        public int? ModelYear { get; init; }
        public IReadOnlyCollection<DateTimeOffset> SuggestedStartTimesUtc { get; init; } = [];
    }
}
