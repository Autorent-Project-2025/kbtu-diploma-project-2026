namespace CarService.Application.DTOs.Bookings
{
    public sealed class CarAvailabilityDto
    {
        public int CarId { get; set; }
        public bool IsAvailable { get; set; }
        public DateTimeOffset NextAvailableFrom { get; set; }
    }
}
