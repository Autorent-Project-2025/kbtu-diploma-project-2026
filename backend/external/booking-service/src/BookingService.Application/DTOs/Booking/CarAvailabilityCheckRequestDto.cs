using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.DTOs.Booking
{
    public sealed class CarAvailabilityCheckRequestDto
    {
        [MinLength(1)]
        public IReadOnlyCollection<int> CarIds { get; init; } = [];

        public DateTimeOffset StartTime { get; init; }
        public DateTimeOffset EndTime { get; init; }
    }
}
