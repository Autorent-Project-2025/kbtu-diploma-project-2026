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
            PartnerCarId = booking.PartnerCarId,
            PartnerId = booking.PartnerId,
            CarBrand = string.Empty,
            CarModel = string.Empty,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            PriceHour = booking.PriceHour,
            TotalPrice = booking.TotalPrice,
            CreatedAt = booking.CreatedAt,
            Status = booking.Status.ToString().ToLowerInvariant()
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
                PartnerCarId = booking.PartnerCarId,
                PartnerId = booking.PartnerId,
                CarBrand = string.Empty,
                CarModel = string.Empty,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                PriceHour = booking.PriceHour,
                TotalPrice = booking.TotalPrice,
                CreatedAt = booking.CreatedAt,
                Status = booking.Status.ToString().ToLowerInvariant()
            };
        }
    }
}
