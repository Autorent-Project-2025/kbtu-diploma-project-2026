using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Queries.GetAccessRequestById;

public sealed class GetAccessRequestByIdQueryHandler
{
    private readonly IAccessRequestRepository _accessRequestRepository;

    public GetAccessRequestByIdQueryHandler(IAccessRequestRepository accessRequestRepository)
    {
        _accessRequestRepository = accessRequestRepository;
    }

    public async Task<GetAccessRequestByIdResult> Handle(
        GetAccessRequestByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.RequestId == Guid.Empty)
            throw new ValidationException("Request id is required.");

        var request = await _accessRequestRepository.GetByIdAsync(query.RequestId, cancellationToken);
        if (request is null)
            throw new NotFoundException($"Access request '{query.RequestId}' was not found.");

        return new GetAccessRequestByIdResult(request.ToDto());
    }
}
