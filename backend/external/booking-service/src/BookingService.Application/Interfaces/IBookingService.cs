using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs.Common;

namespace BookingService.Application.Interfaces
{
    public interface IBookingService
    {
        Task<bool> IsCarAvailable(int carId, DateTime start, DateTime end);
        Task<BookingResponseDto> CreateBooking(int userId, BookingCreateDto dto);
        Task<IEnumerable<BookingResponseDto>> GetUserBookings(int userId);
        Task<PagedResult<BookingResponseDto>> GetUserBookingsPaginated(int userId, BookingQueryParams queryParams);
        Task<BookingResponseDto?> GetBooking(int id, int userId);

        Task<bool> CancelBooking(int id, int userId);
        Task<bool> ConfirmBooking(int id, int userId);
        Task<bool> CompleteBooking(int id, int userId);
    }
}
