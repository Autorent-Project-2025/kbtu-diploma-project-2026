using BookingService.Application.DTOs.Booking;
using BookingService.Domain.Entities;
using System.Linq.Expressions;

namespace BookingService.Application.Mappers
{
    public static class BookingMappers
    {
        private static readonly Expression<Func<Booking, BookingResponseDto>> BookingResponseProjection = booking => new BookingResponseDto
        {
            Id = booking.Id,
            UserId = booking.UserId,
            CarId = booking.CarId,
            CarBrand = string.Empty,
            CarModel = string.Empty,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            Price = booking.Price,
            Status = booking.Status.ToString()
        };

        public static IQueryable<BookingResponseDto> SelectToBookingResponseDto(this IQueryable<Booking> query)
        {
            return query.Select(BookingResponseProjection);
        }

        public static BookingResponseDto ToBookingResponseDto(this Booking booking)
        {
            return new BookingResponseDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                CarId = booking.CarId,
                CarBrand = string.Empty,
                CarModel = string.Empty,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Price = booking.Price,
                Status = booking.Status.ToString()
            };
        }
    }
}
