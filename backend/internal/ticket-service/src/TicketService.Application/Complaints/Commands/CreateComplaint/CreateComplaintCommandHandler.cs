using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Complaints.Commands.CreateComplaint;

public sealed class CreateComplaintCommandHandler
{
    private static readonly HashSet<string> AllowedBookingStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active", "awaitingreview", "completed"
    };

    private readonly IComplaintRepository _complaintRepository;
    private readonly ITicketUnitOfWork _unitOfWork;
    private readonly IBookingReadClient _bookingReadClient;
    private readonly IFileStorageClient _fileStorageClient;
    private readonly IChatServiceClient _chatServiceClient;

    public CreateComplaintCommandHandler(
        IComplaintRepository complaintRepository,
        ITicketUnitOfWork unitOfWork,
        IBookingReadClient bookingReadClient,
        IFileStorageClient fileStorageClient,
        IChatServiceClient chatServiceClient)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _bookingReadClient = bookingReadClient;
        _fileStorageClient = fileStorageClient;
        _chatServiceClient = chatServiceClient;
    }

    public async Task<CreateComplaintResult> Handle(
        CreateComplaintCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.CreatedByUserId == Guid.Empty)
            throw new ValidationException("User id is required.");
        if (command.BookingId <= 0)
            throw new ValidationException("Booking id is required.");

        var booking = await _bookingReadClient.GetBookingAsync(command.BookingId, cancellationToken);
        if (booking is null)
            throw new NotFoundException($"Booking '{command.BookingId}' was not found.");

        ValidateBookingOwnership(command.CreatedByUserId, command.ReporterActorType, booking);
        ValidateBookingStatus(booking);

        var exists = await _complaintRepository.ExistsForBookingAndReporterAsync(
            command.BookingId, command.CreatedByUserId, cancellationToken);
        if (exists)
            throw new ConflictException("A complaint already exists for this booking. Use the reopen request to continue.");

        var snapshot = BuildSnapshot(command.ReporterActorType, booking);

        var complaint = Complaint.Create(
            command.BookingId,
            command.CreatedByUserId,
            command.ReporterActorType,
            command.TargetType,
            command.Category,
            command.Subject,
            command.Description,
            snapshot);

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
                    command.CreatedByUserId,
                    AttachmentPhase.Creation));
            }
        }

        await _complaintRepository.AddAsync(complaint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actorType = command.ReporterActorType == ReporterActorType.Client ? "client" : "partner";
        await _chatServiceClient.CreateConversationAsync(
            "complaint",
            complaint.Id.ToString(),
            "ticket-service",
            [new ChatParticipant(
                command.CreatedByUserId.ToString(),
                actorType,
                "reporter",
                CanRead: true,
                CanWrite: true,
                CanSendInternal: false)],
            "Жалоба создана",
            cancellationToken);

        return new CreateComplaintResult(complaint.ToDto());
    }

    private static void ValidateBookingOwnership(
        Guid userId,
        ReporterActorType reporterActorType,
        BookingForComplaintResult booking)
    {
        var isOwner = reporterActorType switch
        {
            ReporterActorType.Client => booking.UserId == userId,
            ReporterActorType.Partner => booking.PartnerUserId == userId,
            _ => false
        };

        if (!isOwner)
            throw new ValidationException("You can only file a complaint for your own booking.");
    }

    private static void ValidateBookingStatus(BookingForComplaintResult booking)
    {
        var status = booking.Status?.Trim().ToLowerInvariant() ?? string.Empty;

        if (status == "canceled")
        {
            if (booking.TripStartedAt is null)
                throw new ValidationException("Cannot file a complaint for a booking that was cancelled before the trip started.");
            return;
        }

        if (!AllowedBookingStatuses.Contains(status))
            throw new ValidationException($"Cannot file a complaint for a booking with status '{booking.Status}'.");
    }

    private static BookingSnapshotData BuildSnapshot(
        ReporterActorType reporterActorType,
        BookingForComplaintResult booking)
    {
        return new BookingSnapshotData
        {
            BookingId = booking.Id,
            Status = booking.Status ?? string.Empty,
            CarBrand = booking.CarBrand,
            CarModel = booking.CarModel,
            PartnerName = booking.PartnerName,
            CoverImageUrl = booking.CoverImageUrl,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalPrice = booking.TotalPrice,
            ReporterFullName = string.Empty,
            CounterpartyName = reporterActorType == ReporterActorType.Client
                ? booking.PartnerName ?? string.Empty
                : string.Empty,
            CounterpartyUserId = reporterActorType == ReporterActorType.Client
                ? booking.PartnerUserId
                : booking.UserId
        };
    }
}
