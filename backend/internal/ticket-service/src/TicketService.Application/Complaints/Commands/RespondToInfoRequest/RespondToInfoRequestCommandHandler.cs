using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Complaints.Commands.RespondToInfoRequest;

public sealed class RespondToInfoRequestCommandHandler
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IFileStorageClient _fileStorageClient;

    public RespondToInfoRequestCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IFileStorageClient fileStorageClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _fileStorageClient = fileStorageClient;
    }

    public async Task<RespondToInfoRequestResult> Handle(
        RespondToInfoRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ComplaintId == Guid.Empty)
            throw new ValidationException("Complaint id is required.");
        if (command.ReporterUserId == Guid.Empty)
            throw new ValidationException("Reporter user id is required.");

        var complaint = await _complaintRepository.GetByIdAsync(command.ComplaintId, cancellationToken);
        if (complaint is null)
            throw new NotFoundException($"Complaint '{command.ComplaintId}' was not found.");

        if (complaint.CreatedByUserId != command.ReporterUserId)
            throw new ValidationException("You can only respond to your own complaint.");

        complaint.RespondToInfoRequest(command.ReporterUserId, command.Message);

        if (command.Attachments is { Count: > 0 })
        {
            foreach (var file in command.Attachments.Take(5))
            {
                var fileName = await _fileStorageClient.UploadFileAsync(file, cancellationToken);
                complaint.AddAttachment(new ComplaintAttachment(
                    complaint.Id,
                    fileName,
                    file.FileName,
                    file.ContentType,
                    command.ReporterUserId,
                    AttachmentPhase.InfoResponse));
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RespondToInfoRequestResult(complaint.ToDto());
    }
}
