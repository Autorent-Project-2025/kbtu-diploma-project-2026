namespace BookingService.Application.DTOs.Booking
{
    public sealed class CarAvailabilityResultDto
    {
        public int PartnerCarId { get; init; }
        public bool IsAvailable { get; init; }
        public DateTimeOffset NextAvailableFrom { get; init; }
    }
}
