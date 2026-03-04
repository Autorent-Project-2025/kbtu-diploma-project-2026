namespace BookingService.Application.DTOs.Booking
{
    public class CarBookingCountDto
    {
        public int PartnerCarId { get; set; }
        public int Count { get; set; }

        // Backward-compatible alias for integrations still expecting `carId`.
        public int CarId => PartnerCarId;
    }
}
