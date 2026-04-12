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
        Task<PagedResult<BookingResponseDto>> GetAllBookingsPaginated(BookingQueryParams queryParams);
        Task<BookingResponseDto?> GetBooking(int id, Guid userId);
        Task<IReadOnlyCollection<BookingResponseDto>> GetBookingsByPartnerCarId(int partnerCarId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<BookingResponseDto>> GetBookingsByPartnerUserId(Guid partnerUserId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<CarBookingCountDto>> GetBookingCountsByPartnerCarIds(IReadOnlyCollection<int> partnerCarIds, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<CarAvailabilityResultDto>> CheckAvailabilityByPartnerCarIds(
            IReadOnlyCollection<int> partnerCarIds,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default);
        Task<BookingPaymentStatusResponseDto> StartPayment(int id, Guid userId);
        Task<BookingPaymentStatusResponseDto> GetPaymentStatus(int id, Guid userId);
        Task<BookingPaymentStatusResponseDto> SubmitPayment(int id, Guid userId, BookingPaymentSubmitRequestDto dto);
        Task<bool> CancelBooking(int id, Guid userId);
        Task<bool> CancelBookingByPartner(int id, Guid partnerUserId);
        Task<PartnerBookingCancellationRequestResultDto> RequestPartnerCancellation(
            int id,
            Guid partnerUserId,
            string requesterEmail,
            string reason,
            CancellationToken cancellationToken = default);
        Task<bool> ConfirmBooking(int id, Guid userId);
        Task<bool> StartTrip(int id, Guid userId);
        Task<BookingCompletionSubmissionResponseDto> SubmitCompletionReview(int id, Guid userId, BookingCompletionSubmissionDto dto);
        Task<BookingCarCommentSubmissionResponseDto> SubmitCarComment(int id, Guid userId, BookingCarCommentCreateDto dto, CancellationToken cancellationToken = default);
        Task<bool> CompleteBooking(int id, Guid userId);
        Task<IReadOnlyCollection<BookingChargeResponseDto>> GetBookingCharges(int id, Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<BookingChargeResponseDto>> GetAllBookingCharges(int id, CancellationToken cancellationToken = default);
        Task<BookingChargeResponseDto> PayBookingCharge(int id, long chargeId, Guid userId, CancellationToken cancellationToken = default);
        Task<BookingResponseDto?> GetBookingById(int id, CancellationToken cancellationToken = default);
        Task<bool> CancelBookingByAdmin(int id, CancellationToken cancellationToken = default);
        Task ProcessCompletionReviewApproved(
            int bookingId,
            Guid ticketId,
            decimal? latePenaltyAmount,
            string customerEmail,
            string customerFullName,
            CancellationToken cancellationToken = default);
        Task ProcessCompletionReviewFineIssued(
            int bookingId,
            Guid ticketId,
            decimal? latePenaltyAmount,
            decimal damageFineAmount,
            string fineComment,
            string customerEmail,
            string customerFullName,
            CancellationToken cancellationToken = default);
        Task ProcessPartnerCancellationApproved(
            int bookingId,
            Guid ticketId,
            CancellationToken cancellationToken = default);
        Task ProcessPartnerCancellationRejected(
            int bookingId,
            Guid ticketId,
            string decisionReason,
            CancellationToken cancellationToken = default);

        // Profile stats
        Task<BookingStatsDto> GetUserBookingStats(Guid userId, CancellationToken cancellationToken = default);
    }
}
