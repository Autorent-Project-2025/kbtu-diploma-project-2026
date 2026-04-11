using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Queries.GetAccessRequests;

public sealed class GetAccessRequestsQueryHandler
{
    private readonly IAccessRequestRepository _accessRequestRepository;

    public GetAccessRequestsQueryHandler(IAccessRequestRepository accessRequestRepository)
    {
        _accessRequestRepository = accessRequestRepository;
    }

    public async Task<GetAccessRequestsResult> Handle(
        GetAccessRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var requests = await _accessRequestRepository.GetAllFilteredAsync(query.Status, cancellationToken);
        return new GetAccessRequestsResult(requests.Select(r => r.ToDto()).ToArray());
    }
}
