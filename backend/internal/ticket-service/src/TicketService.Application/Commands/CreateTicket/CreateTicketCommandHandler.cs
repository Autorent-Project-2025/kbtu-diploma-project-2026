using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Commands.CreateTicket;

public sealed class CreateTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketUnitOfWork _ticketUnitOfWork;
    private readonly IFileStorageClient _fileStorageClient;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketUnitOfWork ticketUnitOfWork,
        IFileStorageClient fileStorageClient)
    {
        _ticketRepository = ticketRepository;
        _ticketUnitOfWork = ticketUnitOfWork;
        _fileStorageClient = fileStorageClient;
    }

    public async Task<CreateTicketResult> Handle(
        CreateTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var identityDocumentFileName = await _fileStorageClient.UploadFileAsync(
            command.IdentityDocumentFile,
            cancellationToken);

        string? driverLicenseFileName = null;
        if (command.TicketType == TicketType.Client)
        {
            driverLicenseFileName = await _fileStorageClient.UploadFileAsync(
                command.DriverLicenseFile!,
                cancellationToken);
        }

        var ticket = new Ticket(
            Guid.NewGuid(),
            command.TicketType,
            command.FirstName,
            command.LastName,
            command.Email,
            command.BirthDate,
            command.PhoneNumber,
            identityDocumentFileName,
            driverLicenseFileName,
            command.AvatarUrl,
            DateTime.UtcNow);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _ticketUnitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTicketResult(ticket.ToDto());
    }

    private static void Validate(CreateTicketCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            throw new ValidationException("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            throw new ValidationException("Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            throw new ValidationException("Phone number is required.");
        }

        if (command.TicketType is not TicketType.Client and not TicketType.Partner)
        {
            throw new ValidationException("Ticket type is invalid.");
        }

        if (command.TicketType == TicketType.Client)
        {
            if (command.BirthDate == default)
            {
                throw new ValidationException("Birth date is required.");
            }

            if (command.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ValidationException("Birth date cannot be in the future.");
            }
        }

        ValidatePdf(command.IdentityDocumentFile, nameof(command.IdentityDocumentFile));

        if (command.TicketType == TicketType.Client)
        {
            if (command.DriverLicenseFile is null)
            {
                throw new ValidationException($"{nameof(command.DriverLicenseFile)} is required.");
            }

            ValidatePdf(command.DriverLicenseFile, nameof(command.DriverLicenseFile));
        }
    }

    private static void ValidatePdf(TicketDocumentFilePayload file, string fieldName)
    {
        if (file.Content.Length == 0)
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        var fileName = file.FileName?.Trim() ?? string.Empty;
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException($"{fieldName} must be a PDF file.");
        }

        var contentType = file.ContentType?.Trim() ?? string.Empty;
        if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException($"{fieldName} content type must be application/pdf.");
        }
    }
}
