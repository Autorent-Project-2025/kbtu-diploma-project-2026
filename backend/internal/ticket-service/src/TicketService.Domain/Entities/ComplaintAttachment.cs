using TicketService.Domain.Enums;

namespace TicketService.Domain.Entities;

public sealed class ComplaintAttachment
{
    public Guid Id { get; private set; }
    public Guid ComplaintId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;
    public Guid UploadedByUserId { get; private set; }
    public AttachmentPhase AttachmentPhase { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ComplaintAttachment() { }

    public ComplaintAttachment(
        Guid complaintId,
        string fileName,
        string originalFileName,
        string fileType,
        Guid uploadedByUserId,
        AttachmentPhase attachmentPhase)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name is required.", nameof(originalFileName));
        if (uploadedByUserId == Guid.Empty)
            throw new ArgumentException("Uploader user id is required.", nameof(uploadedByUserId));

        Id = Guid.NewGuid();
        ComplaintId = complaintId;
        FileName = fileName.Trim();
        OriginalFileName = originalFileName.Trim().Length > 255
            ? originalFileName.Trim()[..255]
            : originalFileName.Trim();
        FileType = string.IsNullOrWhiteSpace(fileType) ? "application/octet-stream" : fileType.Trim();
        UploadedByUserId = uploadedByUserId;
        AttachmentPhase = attachmentPhase;
        CreatedAt = DateTime.UtcNow;
    }
}
