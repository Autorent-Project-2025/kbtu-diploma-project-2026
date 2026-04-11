using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.AddManagerNote;

public sealed class AddManagerNoteCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;

    public AddManagerNoteCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddManagerNoteResult> Handle(
        AddManagerNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        complaint.AddManagerNote(command.ManagerId, command.Note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddManagerNoteResult(complaint.ToDto());
    }
}
