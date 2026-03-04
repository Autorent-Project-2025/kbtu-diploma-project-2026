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
    private readonly IImageStorageClient _imageStorageClient;
    private readonly IPartnerContextClient _partnerContextClient;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketUnitOfWork ticketUnitOfWork,
        IFileStorageClient fileStorageClient,
        IImageStorageClient imageStorageClient,
        IPartnerContextClient partnerContextClient)
    {
        _ticketRepository = ticketRepository;
        _ticketUnitOfWork = ticketUnitOfWork;
        _fileStorageClient = fileStorageClient;
        _imageStorageClient = imageStorageClient;
        _partnerContextClient = partnerContextClient;
    }

    public async Task<CreateTicketResult> Handle(
        CreateTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var firstName = command.FirstName;
        var lastName = command.LastName;
        var phoneNumber = command.PhoneNumber;
        var email = command.Email;
        string? identityDocumentFileName = null;
        string? driverLicenseFileName = null;
        string? ownershipDocumentFileName = null;
        Guid? relatedPartnerUserId = null;
        IReadOnlyCollection<PartnerCarTicketImageData>? carImages = null;

        if (command.TicketType == TicketType.PartnerCar)
        {
            var partnerContext = await ResolvePartnerContextAsync(command.AuthorizationHeader, cancellationToken);
            firstName = partnerContext.OwnerFirstName;
            lastName = partnerContext.OwnerLastName;
            phoneNumber = partnerContext.PhoneNumber;
            relatedPartnerUserId = partnerContext.RelatedUserId;

            ownershipDocumentFileName = await _fileStorageClient.UploadFileAsync(
                command.OwnershipDocumentFile!,
                cancellationToken);

            carImages = await UploadPartnerCarImagesAsync(
                command.CarImageFiles!,
                command.AuthorizationHeader!,
                cancellationToken);
        }
        else
        {
            identityDocumentFileName = await _fileStorageClient.UploadFileAsync(
                command.IdentityDocumentFile!,
                cancellationToken);

            if (command.TicketType == TicketType.Client)
            {
                driverLicenseFileName = await _fileStorageClient.UploadFileAsync(
                    command.DriverLicenseFile!,
                    cancellationToken);
            }
        }

        var ticket = new Ticket(
            Guid.NewGuid(),
            command.TicketType,
            firstName,
            lastName,
            email,
            command.BirthDate,
            phoneNumber,
            identityDocumentFileName,
            driverLicenseFileName,
            command.AvatarUrl,
            command.CompanyName,
            command.ContactEmail,
            relatedPartnerUserId,
            command.CarBrand,
            command.CarModel,
            command.LicensePlate,
            ownershipDocumentFileName,
            carImages,
            DateTime.UtcNow);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _ticketUnitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTicketResult(ticket.ToDto());
    }

    private async Task<PartnerContextResult> ResolvePartnerContextAsync(
        string? authorizationHeader,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            throw new UnauthorizedException("Authorization header is required for partner car tickets.");
        }

        var context = await _partnerContextClient.GetCurrentPartnerAsync(authorizationHeader, cancellationToken);
        if (context is null)
        {
            throw new UnauthorizedException("Current user is not a partner.");
        }

        return context;
    }

    private async Task<IReadOnlyCollection<PartnerCarTicketImageData>> UploadPartnerCarImagesAsync(
        IReadOnlyCollection<TicketDocumentFilePayload> imageFiles,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var uploadedImages = new List<PartnerCarTicketImageData>(imageFiles.Count);
        foreach (var imageFile in imageFiles)
        {
            var uploaded = await _imageStorageClient.UploadAsync(imageFile, authorizationHeader, cancellationToken);
            uploadedImages.Add(new PartnerCarTicketImageData
            {
                ImageId = uploaded.ImageId,
                ImageUrl = uploaded.ImageUrl
            });
        }

        return uploadedImages;
    }

    private static void Validate(CreateTicketCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (command.TicketType is not TicketType.Client and not TicketType.Partner and not TicketType.PartnerCar)
        {
            throw new ValidationException("Ticket type is invalid.");
        }

        if (command.TicketType == TicketType.PartnerCar)
        {
            if (string.IsNullOrWhiteSpace(command.CarBrand))
            {
                throw new ValidationException("Car brand is required for partner car tickets.");
            }

            if (string.IsNullOrWhiteSpace(command.CarModel))
            {
                throw new ValidationException("Car model is required for partner car tickets.");
            }

            if (string.IsNullOrWhiteSpace(command.LicensePlate))
            {
                throw new ValidationException("License plate is required for partner car tickets.");
            }

            if (command.OwnershipDocumentFile is null)
            {
                throw new ValidationException($"{nameof(command.OwnershipDocumentFile)} is required.");
            }

            if (command.CarImageFiles is null || command.CarImageFiles.Count == 0)
            {
                throw new ValidationException("At least one partner car image is required.");
            }

            ValidatePdf(command.OwnershipDocumentFile, nameof(command.OwnershipDocumentFile));

            foreach (var file in command.CarImageFiles)
            {
                ValidateImage(file, nameof(command.CarImageFiles));
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            throw new ValidationException("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            throw new ValidationException("Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            throw new ValidationException("Phone number is required.");
        }

        if (!string.IsNullOrWhiteSpace(command.CompanyName) && command.CompanyName.Trim().Length > 300)
        {
            throw new ValidationException("Company name length must not exceed 300.");
        }

        if (!string.IsNullOrWhiteSpace(command.ContactEmail) && command.ContactEmail.Trim().Length > 255)
        {
            throw new ValidationException("Contact email length must not exceed 255.");
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

        if (command.IdentityDocumentFile is null)
        {
            throw new ValidationException($"{nameof(command.IdentityDocumentFile)} is required.");
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

    private static void ValidateImage(TicketDocumentFilePayload file, string fieldName)
    {
        if (file.Content.Length == 0)
        {
            throw new ValidationException($"{fieldName} contains an empty file.");
        }

        var fileName = file.FileName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ValidationException($"{fieldName} contains a file with empty name.");
        }

        var contentType = file.ContentType?.Trim() ?? string.Empty;
        var hasImageContentType = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        var extension = Path.GetExtension(fileName);
        var hasKnownImageExtension =
            extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase);

        if (!hasImageContentType &&
            !(string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase) && hasKnownImageExtension))
        {
            throw new ValidationException($"{fieldName} files must be images.");
        }
    }
}
