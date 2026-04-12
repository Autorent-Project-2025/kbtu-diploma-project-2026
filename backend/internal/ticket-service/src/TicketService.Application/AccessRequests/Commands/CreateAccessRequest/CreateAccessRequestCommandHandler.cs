using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.AccessRequests.Commands.CreateAccessRequest;

public sealed class CreateAccessRequestCommandHandler
{
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;

    public CreateAccessRequestCommandHandler(
        IAccessRequestRepository accessRequestRepository,
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork)
    {
        _accessRequestRepository = accessRequestRepository;
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateAccessRequestResult> Handle(
        CreateAccessRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");
        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new ValidationException("Reason is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        if (complaint.AssignedToManagerId != command.ManagerId)
            throw new ValidationException("Only the assigned manager can request booking access.");

        if (complaint.Status is ComplaintStatus.Resolved or ComplaintStatus.Rejected)
            throw new ValidationException("Cannot request booking access for a closed complaint.");

        var hasPending = await _accessRequestRepository.ExistsPendingAsync(
            command.ComplaintId, complaint.BookingId, command.ManagerId, cancellationToken);
        if (hasPending)
            throw new ConflictException("A pending access request already exists for this complaint and booking.");

        var activeGrant = await _accessRequestRepository.GetActiveGrantAsync(
            command.ManagerId, complaint.BookingId, cancellationToken);
        if (activeGrant is not null)
            throw new ConflictException("An active access grant already exists for this booking.");

        var request = ComplaintBookingAccessRequest.Create(
            command.ComplaintId,
            complaint.BookingId,
            command.ManagerId,
            command.Reason);

        await _accessRequestRepository.AddAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAccessRequestResult(request.ToDto());
    }
}
