using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs.Common;

namespace BookingService.Application.Interfaces
{
    public interface IBookingService
    {
        Task<bool> IsCarAvailable(int carId, DateTime start, DateTime end);
        Task<BookingResponseDto> CreateBooking(string userId, BookingCreateDto dto);
        Task<IEnumerable<BookingResponseDto>> GetUserBookings(string userId);
        Task<PagedResult<BookingResponseDto>> GetUserBookingsPaginated(string userId, BookingQueryParams queryParams);
        Task<BookingResponseDto?> GetBooking(int id, string userId);
        Task<IReadOnlyCollection<BookingResponseDto>> GetBookingsByCarId(int carId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<CarBookingCountDto>> GetBookingCountsByCarIds(IReadOnlyCollection<int> carIds, CancellationToken cancellationToken = default);

        Task<bool> CancelBooking(int id, string userId);
        Task<bool> ConfirmBooking(int id, string userId);
        Task<bool> CompleteBooking(int id, string userId);
    }
}
