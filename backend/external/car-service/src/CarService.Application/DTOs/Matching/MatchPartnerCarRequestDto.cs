namespace CarService.Application.DTOs.Matching
{
    public sealed class MatchPartnerCarRequestDto
    {
        public int ModelId { get; init; }
        public DateTimeOffset StartTime { get; init; }
        public DateTimeOffset EndTime { get; init; }
    }
}
