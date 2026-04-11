using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.TakeComplaint;

public sealed class TakeComplaintCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;

    public TakeComplaintCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TakeComplaintResult> Handle(
        TakeComplaintCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ManagerId == Guid.Empty)
            throw new ValidationException("Manager id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        complaint.Take(command.ManagerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TakeComplaintResult(complaint.ToDto());
    }
}
