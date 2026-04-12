using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetAllComplaints;

public sealed class GetAllComplaintsQueryHandler
{
    private readonly IComplaintRepository _complaintRepository;

    public GetAllComplaintsQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<GetAllComplaintsResult> Handle(
        GetAllComplaintsQuery query,
        CancellationToken cancellationToken = default)
    {
        var complaints = await _complaintRepository.GetAllFilteredAsync(
            query.Status,
            query.Category,
            query.Priority,
            query.AssignedToManagerId,
            cancellationToken);

        return new GetAllComplaintsResult(complaints.Select(c => c.ToDto()).ToArray());
    }
}
