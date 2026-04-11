using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Complaints.Commands.CreateReopenRequest;

public sealed class CreateReopenRequestCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly IReopenRequestRepository _reopenRequestRepository;
    private readonly ITicketUnitOfWork _unitOfWork;

    public CreateReopenRequestCommandHandler(
        IComplaintRepository complaintRepository,
        IReopenRequestRepository reopenRequestRepository,
        ITicketUnitOfWork unitOfWork)
    {
        _complaintRepository = complaintRepository;
        _reopenRequestRepository = reopenRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateReopenRequestResult> Handle(
        CreateReopenRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.RequestedByUserId == Guid.Empty)
            throw new ValidationException("User id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        if (complaint.CreatedByUserId != command.RequestedByUserId)
            throw new ValidationException("Only the complaint reporter can request reopening.");

        if (complaint.Status is not (ComplaintStatus.Resolved or ComplaintStatus.Rejected))
            throw new ValidationException("Only closed complaints can be reopened.");

        var hasPending = await _reopenRequestRepository.ExistsPendingForComplaintAsync(
            command.ComplaintId, cancellationToken);
        if (hasPending)
            throw new ConflictException("A pending reopen request already exists for this complaint.");

        var reopenRequest = ComplaintReopenRequest.Create(
            command.ComplaintId,
            command.RequestedByUserId,
            command.Reason);

        await _reopenRequestRepository.AddAsync(reopenRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateReopenRequestResult(reopenRequest.ToDto());
    }
}
