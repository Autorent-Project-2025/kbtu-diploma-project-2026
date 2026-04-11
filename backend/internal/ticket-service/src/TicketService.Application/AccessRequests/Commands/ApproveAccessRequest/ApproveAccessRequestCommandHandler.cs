using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Commands.ApproveAccessRequest;

public sealed class ApproveAccessRequestCommandHandler
{
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly ITicketUnitOfWork _unitOfWork;

    public ApproveAccessRequestCommandHandler(
        IAccessRequestRepository accessRequestRepository,
        ITicketUnitOfWork unitOfWork)
    {
        _accessRequestRepository = accessRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApproveAccessRequestResult> Handle(
        ApproveAccessRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.RequestId == Guid.Empty)
            throw new ValidationException("Request id is required.");
        if (command.SupermanagerId == Guid.Empty)
            throw new ValidationException("Supermanager id is required.");
        if (command.ExpiresInHours < 1 || command.ExpiresInHours > 168)
            throw new ValidationException("Expiry must be between 1 and 168 hours.");

        var request = await _accessRequestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            throw new NotFoundException($"Access request '{command.RequestId}' was not found.");

        var expiresAt = DateTime.UtcNow.AddHours(command.ExpiresInHours);
        request.Approve(command.SupermanagerId, command.DecisionNote, expiresAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApproveAccessRequestResult(request.ToDto());
    }
}
