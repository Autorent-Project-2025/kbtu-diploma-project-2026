using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.AddManagerNote;

public sealed class AddManagerNoteCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IChatServiceClient _chatServiceClient;

    public AddManagerNoteCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _chatServiceClient = chatServiceClient;
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

        // Send manager note as internal-only message to chat conversation
        _ = Task.Run(async () =>
        {
            try
            {
                var conversationId = await _chatServiceClient.GetConversationIdByContextAsync(
                    "complaint", complaint.Id.ToString(), cancellationToken);
                if (conversationId is not null)
                {
                    await _chatServiceClient.SendSystemMessageAsync(
                        conversationId,
                        $"Заметка менеджера: {command.Note}",
                        internalOnly: true,
                        cancellationToken);
                }
            }
            catch { /* non-blocking */ }
        });

        return new AddManagerNoteResult(complaint.ToDto());
    }
}
