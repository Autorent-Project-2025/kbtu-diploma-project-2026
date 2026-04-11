using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetComplaintByBooking;

public sealed class GetComplaintByBookingQueryHandler
{
    private readonly IComplaintRepository _complaintRepository;

    public GetComplaintByBookingQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<GetComplaintByBookingResult> Handle(
        GetComplaintByBookingQuery query,
        CancellationToken cancellationToken = default)
    {
        var complaint = await _complaintRepository.GetByBookingAndReporterAsync(
            query.BookingId, query.ReporterUserId, cancellationToken);

        return new GetComplaintByBookingResult(complaint?.ToDto());
    }
}
