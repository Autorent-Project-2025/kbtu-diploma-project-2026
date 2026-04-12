using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Queries.GetAccessRequestForComplaint;

public sealed class GetAccessRequestForComplaintQueryHandler
{
    private readonly IAccessRequestRepository _accessRequestRepository;

    public GetAccessRequestForComplaintQueryHandler(IAccessRequestRepository accessRequestRepository)
    {
        _accessRequestRepository = accessRequestRepository;
    }

    public async Task<GetAccessRequestForComplaintResult> Handle(
        GetAccessRequestForComplaintQuery query,
        CancellationToken cancellationToken = default)
    {
        var request = await _accessRequestRepository.GetForComplaintAndManagerAsync(
            query.ComplaintId, query.ManagerId, cancellationToken);

        return new GetAccessRequestForComplaintResult(request?.ToDto());
    }
}
