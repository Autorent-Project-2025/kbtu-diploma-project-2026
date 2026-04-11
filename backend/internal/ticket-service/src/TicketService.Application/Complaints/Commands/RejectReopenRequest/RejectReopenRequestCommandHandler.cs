using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.RejectReopenRequest;

public sealed class RejectReopenRequestCommandHandler
{
    private readonly IReopenRequestRepository _reopenRequestRepository;
    private readonly ITicketUnitOfWork _unitOfWork;

    public RejectReopenRequestCommandHandler(
        IReopenRequestRepository reopenRequestRepository,
        ITicketUnitOfWork unitOfWork)
    {
        _reopenRequestRepository = reopenRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RejectReopenRequestResult> Handle(
        RejectReopenRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ReopenRequestId == Guid.Empty)
            throw new ValidationException("Reopen request id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");

        var reopenRequest = await _reopenRequestRepository.GetByIdAsync(
            command.ReopenRequestId, cancellationToken);
        if (reopenRequest is null)
            throw new NotFoundException($"Reopen request '{command.ReopenRequestId}' was not found.");

        reopenRequest.Reject(command.ManagerId, command.Note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RejectReopenRequestResult(reopenRequest.ToDto());
    }
}
