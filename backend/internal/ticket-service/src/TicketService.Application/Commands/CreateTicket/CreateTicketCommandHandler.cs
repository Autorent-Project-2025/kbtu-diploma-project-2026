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
        IReadOnlyCollection<BookingCompletionTicketPhotoData>? completionPhotos = null;

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
                command.CarImageTypes,
                command.AuthorizationHeader!,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException("Partner email is required for partner car tickets.");
            }
        }
        else if (command.TicketType == TicketType.BookingCompletion)
        {
            completionPhotos = await UploadBookingCompletionPhotosAsync(command, cancellationToken);
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

        var ticket = command.TicketType == TicketType.BookingCompletion
            ? Ticket.CreateBookingCompletion(
                Guid.NewGuid(),
                firstName,
                lastName,
                email,
                phoneNumber,
                command.BookingId ?? 0,
                command.PlannedStartTime ?? default,
                command.PlannedEndTime ?? default,
                command.TripStartedAt ?? default,
                command.TripCompletedAt ?? default,
                command.LatePenaltyAmount,
                completionPhotos ?? [],
                DateTime.UtcNow)
            : new Ticket(
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
                command.CarYear,
                command.LicensePlate,
                command.Transmission,
                command.FuelType,
                command.Seats,
                command.Doors,
                command.BodyType,
                command.Horsepower,
                command.SelectedTags,
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
        IReadOnlyCollection<string>? imageTypes,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        if (imageTypes is null || imageTypes.Count != imageFiles.Count)
        {
            throw new ValidationException("Each partner car image must include a matching image type.");
        }

        var imageTypeList = imageTypes.ToArray();
        var uploadedImages = new List<PartnerCarTicketImageData>(imageFiles.Count);
        var index = 0;
        foreach (var imageFile in imageFiles)
        {
            var uploaded = await _imageStorageClient.UploadAsync(imageFile, authorizationHeader, cancellationToken);
            uploadedImages.Add(new PartnerCarTicketImageData
            {
                ImageId = uploaded.ImageId,
                ImageUrl = uploaded.ImageUrl,
                ImageType = imageTypeList[index]
            });
            index += 1;
        }

        return uploadedImages;
    }

    private async Task<IReadOnlyCollection<BookingCompletionTicketPhotoData>> UploadBookingCompletionPhotosAsync(
        CreateTicketCommand command,
        CancellationToken cancellationToken)
    {
        return new[]
        {
            new BookingCompletionTicketPhotoData
            {
                Slot = "front",
                FileName = await _fileStorageClient.UploadFileAsync(command.CompletionFrontPhotoFile!, cancellationToken)
            },
            new BookingCompletionTicketPhotoData
            {
                Slot = "back",
                FileName = await _fileStorageClient.UploadFileAsync(command.CompletionBackPhotoFile!, cancellationToken)
            },
            new BookingCompletionTicketPhotoData
            {
                Slot = "side_left",
                FileName = await _fileStorageClient.UploadFileAsync(command.CompletionSideLeftPhotoFile!, cancellationToken)
            },
            new BookingCompletionTicketPhotoData
            {
                Slot = "side_right",
                FileName = await _fileStorageClient.UploadFileAsync(command.CompletionSideRightPhotoFile!, cancellationToken)
            },
            new BookingCompletionTicketPhotoData
            {
                Slot = "interior",
                FileName = await _fileStorageClient.UploadFileAsync(command.CompletionInteriorPhotoFile!, cancellationToken)
            }
        };
    }

    private static void Validate(CreateTicketCommand command)
    {
        if (command.TicketType != TicketType.PartnerCar && string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (command.TicketType is not TicketType.Client and not TicketType.Partner and not TicketType.PartnerCar and not TicketType.BookingCompletion)
        {
            throw new ValidationException("Ticket type is invalid.");
        }

        if (command.TicketType == TicketType.BookingCompletion)
        {
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

            if (!command.BookingId.HasValue || command.BookingId.Value <= 0)
            {
                throw new ValidationException("BookingId is required.");
            }

            if (!command.PlannedStartTime.HasValue || !command.PlannedEndTime.HasValue)
            {
                throw new ValidationException("Planned start and end times are required.");
            }

            if (!command.TripStartedAt.HasValue || !command.TripCompletedAt.HasValue)
            {
                throw new ValidationException("Trip started and completed timestamps are required.");
            }

            if (command.PlannedEndTime.Value < command.PlannedStartTime.Value)
            {
                throw new ValidationException("PlannedEndTime must be greater than or equal to PlannedStartTime.");
            }

            if (command.TripStartedAt.Value < command.PlannedStartTime.Value.AddMinutes(-15))
            {
                throw new ValidationException("TripStartedAt cannot be earlier than 15 minutes before PlannedStartTime.");
            }

            if (command.TripCompletedAt.Value < command.TripStartedAt.Value)
            {
                throw new ValidationException("TripCompletedAt cannot be earlier than TripStartedAt.");
            }

            ValidateOptionalFine(command.LatePenaltyAmount, nameof(command.LatePenaltyAmount));

            if (command.CompletionFrontPhotoFile is null ||
                command.CompletionBackPhotoFile is null ||
                command.CompletionSideLeftPhotoFile is null ||
                command.CompletionSideRightPhotoFile is null ||
                command.CompletionInteriorPhotoFile is null)
            {
                throw new ValidationException("All 5 completion photos are required.");
            }

            ValidateImage(command.CompletionFrontPhotoFile, nameof(command.CompletionFrontPhotoFile));
            ValidateImage(command.CompletionBackPhotoFile, nameof(command.CompletionBackPhotoFile));
            ValidateImage(command.CompletionSideLeftPhotoFile, nameof(command.CompletionSideLeftPhotoFile));
            ValidateImage(command.CompletionSideRightPhotoFile, nameof(command.CompletionSideRightPhotoFile));
            ValidateImage(command.CompletionInteriorPhotoFile, nameof(command.CompletionInteriorPhotoFile));
            return;
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

            if (!command.CarYear.HasValue)
            {
                throw new ValidationException("Car year is required for partner car tickets.");
            }

            var maxAllowedCarYear = DateTime.UtcNow.Year + 1;
            if (command.CarYear.Value < 1886 || command.CarYear.Value > maxAllowedCarYear)
            {
                throw new ValidationException($"Car year must be between 1886 and {maxAllowedCarYear}.");
            }

            if (command.OwnershipDocumentFile is null)
            {
                throw new ValidationException($"{nameof(command.OwnershipDocumentFile)} is required.");
            }

            if (command.CarImageFiles is null || command.CarImageFiles.Count == 0)
            {
                throw new ValidationException("At least one partner car image is required.");
            }

            if (command.CarImageTypes is null || command.CarImageTypes.Count != command.CarImageFiles.Count)
            {
                throw new ValidationException("Each partner car image must include a matching image type.");
            }

            ValidatePdf(command.OwnershipDocumentFile, nameof(command.OwnershipDocumentFile));

            foreach (var file in command.CarImageFiles)
            {
                ValidateImage(file, nameof(command.CarImageFiles));
            }

            foreach (var imageType in command.CarImageTypes)
            {
                ValidatePartnerCarImageType(imageType);
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

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - command.BirthDate.Value.Year;
            if (command.BirthDate.Value > today.AddYears(-age)) age--;
            if (age < 18)
            {
                throw new ValidationException("You must be at least 18 years old to register.");
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

    private static void ValidatePartnerCarImageType(string? imageType)
    {
        var normalized = imageType?.Trim().ToLowerInvariant();
        if (normalized is not "front" and not "back" and not "side" and not "interior" and not "general")
        {
            throw new ValidationException("Partner car image type must be one of: front, back, side, interior, general.");
        }
    }

    private static void ValidateOptionalFine(decimal? value, string fieldName)
    {
        if (!value.HasValue)
        {
            return;
        }

        if (value.Value < 0m)
        {
            throw new ValidationException($"{fieldName} cannot be negative.");
        }

        if (value.Value > 10_000_000m)
        {
            throw new ValidationException($"{fieldName} must not exceed 10000000.");
        }
    }
}
