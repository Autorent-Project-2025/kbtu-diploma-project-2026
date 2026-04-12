using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetMyComplaints;

public sealed class GetMyComplaintsQueryHandler
{
    private readonly IComplaintRepository _complaintRepository;

    public GetMyComplaintsQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<GetMyComplaintsResult> Handle(
        GetMyComplaintsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ReporterUserId == Guid.Empty)
            throw new ValidationException("Reporter user id is required.");

        var complaints = await _complaintRepository.GetByReporterUserIdAsync(
            query.ReporterUserId, cancellationToken);

        return new GetMyComplaintsResult(complaints.Select(c => c.ToDto()).ToArray());
    }
}
