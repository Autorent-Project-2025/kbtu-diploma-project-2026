using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetReopenRequests;

public sealed class GetReopenRequestsQueryHandler
{
    private readonly IReopenRequestRepository _reopenRequestRepository;

    public GetReopenRequestsQueryHandler(IReopenRequestRepository reopenRequestRepository)
    {
        _reopenRequestRepository = reopenRequestRepository;
    }

    public async Task<GetReopenRequestsResult> Handle(
        GetReopenRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var requests = await _reopenRequestRepository.GetByComplaintIdAsync(
            query.ComplaintId, cancellationToken);

        return new GetReopenRequestsResult(
            requests.Select(r => r.ToDto()).ToArray());
    }
}
