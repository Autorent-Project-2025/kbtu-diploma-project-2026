using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Commands.RejectAccessRequest;

public sealed class RejectAccessRequestCommandHandler
{
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly ITicketUnitOfWork _unitOfWork;

    public RejectAccessRequestCommandHandler(
        IAccessRequestRepository accessRequestRepository,
        ITicketUnitOfWork unitOfWork)
    {
        _accessRequestRepository = accessRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RejectAccessRequestResult> Handle(
        RejectAccessRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.RequestId == Guid.Empty)
            throw new ValidationException("Request id is required.");
        if (command.SupermanagerId == Guid.Empty)
            throw new ValidationException("Supermanager id is required.");

        var request = await _accessRequestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            throw new NotFoundException($"Access request '{command.RequestId}' was not found.");

        request.Reject(command.SupermanagerId, command.DecisionNote);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RejectAccessRequestResult(request.ToDto());
    }
}
