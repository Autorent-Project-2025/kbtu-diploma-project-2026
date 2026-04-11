using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Queries.GetBookingReview;

public sealed class GetBookingReviewQueryHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly IBookingReadClient _bookingReadClient;

    public GetBookingReviewQueryHandler(
        IComplaintRepository complaintRepository,
        IAccessRequestRepository accessRequestRepository,
        IBookingReadClient bookingReadClient)
    {
        _complaintRepository = complaintRepository;
        _accessRequestRepository = accessRequestRepository;
        _bookingReadClient = bookingReadClient;
    }

    public async Task<GetBookingReviewResult> Handle(
        GetBookingReviewQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (query.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(query.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{query.ComplaintId}' was not found.");

        if (!query.HasGlobalBookingView)
        {
            var grant = await _accessRequestRepository.GetActiveGrantAsync(
                query.ManagerId, complaint.BookingId, cancellationToken);

            if (grant is null || !grant.IsActiveGrant())
                throw new UnauthorizedException("You do not have access to this booking. Request access or wait for approval.");
        }

        var booking = await _bookingReadClient.GetBookingAsync(complaint.BookingId, cancellationToken);
        if (booking is null)
            throw new NotFoundException($"Booking '{complaint.BookingId}' was not found.");

        return new GetBookingReviewResult(new BookingReviewDto(
            booking.Id,
            booking.Status,
            booking.CarBrand,
            booking.CarModel,
            booking.CoverImageUrl,
            booking.PartnerName,
            booking.StartTime,
            booking.EndTime,
            booking.TotalPrice,
            booking.TripStartedAt,
            complaint.Id,
            complaint.Subject));
    }
}
