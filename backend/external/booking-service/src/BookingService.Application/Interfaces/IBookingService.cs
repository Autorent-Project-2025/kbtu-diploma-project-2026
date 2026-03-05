using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs.Common;

namespace BookingService.Application.Interfaces
{
    public interface IBookingService
    {
        Task<bool> IsPartnerCarAvailable(int partnerCarId, DateTimeOffset startTime, DateTimeOffset endTime);
        Task<BookingResponseDto> CreateBooking(Guid userId, BookingCreateDto dto);
        Task<IEnumerable<BookingResponseDto>> GetUserBookings(Guid userId);
        Task<PagedResult<BookingResponseDto>> GetUserBookingsPaginated(Guid userId, BookingQueryParams queryParams);
        Task<BookingResponseDto?> GetBooking(int id, Guid userId);
        Task<IReadOnlyCollection<BookingResponseDto>> GetBookingsByPartnerCarId(int partnerCarId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<CarBookingCountDto>> GetBookingCountsByPartnerCarIds(IReadOnlyCollection<int> partnerCarIds, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<CarAvailabilityResultDto>> CheckAvailabilityByPartnerCarIds(
            IReadOnlyCollection<int> partnerCarIds,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default);
        Task<bool> CancelBooking(int id, Guid userId);
        Task<bool> ConfirmBooking(int id, Guid userId);
        Task<bool> CompleteBooking(int id, Guid userId);
    }
}
