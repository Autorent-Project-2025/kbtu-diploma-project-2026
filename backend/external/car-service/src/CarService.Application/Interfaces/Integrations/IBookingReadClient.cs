using CarService.Application.DTOs.Bookings;

namespace CarService.Application.Interfaces.Integrations
{
    public interface IBookingReadClient
    {
        Task<IReadOnlyCollection<CarBookingCountDto>> GetBookingCountsByCarIdsAsync(
            IReadOnlyCollection<int> carIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<LinkedBookingDto>> GetBookingsByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CarAvailabilityDto>> CheckAvailabilityByCarIdsAsync(
            IReadOnlyCollection<int> carIds,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default);
    }
}
