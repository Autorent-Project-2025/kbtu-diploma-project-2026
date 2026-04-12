using BookingService.Application.DTOs.Booking;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
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
            PartnerUserId = booking.PartnerUserId,
            CarBrand = booking.CarBrand ?? string.Empty,
            CarModel = booking.CarModel ?? string.Empty,
            PartnerName = booking.PartnerName,
            CoverImageUrl = booking.CoverImageUrl,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            PriceHour = booking.PriceHour,
            TotalPrice = booking.TotalPrice,
            CreatedAt = booking.CreatedAt,
            TripStartedAt = booking.TripStartedAt,
            TripCompletedAt = booking.TripCompletedAt,
            CompletionReviewTicketId = booking.CompletionReviewTicketId,
            CarCommentId = booking.CarCommentId,
            CarCommentSubmittedAt = booking.CarCommentSubmittedAt,
            CanLeaveComment = booking.Status == BookingStatus.Completed && booking.CarCommentId == null,
            PricingBreakdownJson = booking.PricingBreakdownJson,
            ImageUrlsJson = booking.ImageUrlsJson,
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
                PartnerUserId = booking.PartnerUserId,
                CarBrand = booking.CarBrand ?? string.Empty,
                CarModel = booking.CarModel ?? string.Empty,
                PartnerName = booking.PartnerName,
                CoverImageUrl = booking.CoverImageUrl,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                PriceHour = booking.PriceHour,
                TotalPrice = booking.TotalPrice,
                CreatedAt = booking.CreatedAt,
                TripStartedAt = booking.TripStartedAt,
                TripCompletedAt = booking.TripCompletedAt,
                CompletionReviewTicketId = booking.CompletionReviewTicketId,
                CarCommentId = booking.CarCommentId,
                CarCommentSubmittedAt = booking.CarCommentSubmittedAt,
                CanLeaveComment = booking.Status == BookingStatus.Completed && booking.CarCommentId is null,
                PricingBreakdownJson = booking.PricingBreakdownJson,
                ImageUrlsJson = booking.ImageUrlsJson,
                Status = booking.Status.ToString().ToLowerInvariant()
            };
        }
    }
}
