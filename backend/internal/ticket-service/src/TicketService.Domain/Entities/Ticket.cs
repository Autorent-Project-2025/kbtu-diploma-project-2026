using TicketService.Domain.Enums;

namespace TicketService.Domain.Entities;

public sealed class Ticket
{
    private const string PartnerCarRequestKindCreate = "create";
    private const string PartnerCarRequestKindUpdate = "update";

    private static readonly HashSet<string> AllowedSemanticTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "econom",
        "comfort",
        "business",
        "sport",
        "suv",
        "electric",
        "family"
    };

    public Guid Id { get; private set; }
    public TicketType TicketType { get; private set; }
    public TicketStatus Status { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public TicketData Data { get; private set; } = new ClientTicketData();

    public string FirstName => Data.FirstName;
    public string LastName => Data.LastName;
    public string FullName => Data.FullName;
    public DateOnly? BirthDate => Data is ClientTicketData clientData ? clientData.BirthDate : null;
    public string PhoneNumber => Data.PhoneNumber;
    public string? IdentityDocumentFileName => Data.IdentityDocumentFileName;
    public string? DriverLicenseFileName => Data is ClientTicketData clientData ? clientData.DriverLicenseFileName : null;
    public string? AvatarUrl => Data is ClientTicketData clientData ? clientData.AvatarUrl : null;
    public string? CompanyName => Data is PartnerTicketData partnerData ? partnerData.CompanyName : null;
    public string? ContactEmail => Data is PartnerTicketData partnerData ? partnerData.ContactEmail : null;
    public string? PartnerCarRequestKind => Data is PartnerCarTicketData partnerCarData
        ? partnerCarData.RequestKind
        : null;
    public int? PartnerCarId => Data is PartnerCarTicketData partnerCarData
        ? partnerCarData.PartnerCarId
        : null;
    public Guid? RelatedPartnerUserId => Data switch
    {
        PartnerCarTicketData partnerCarData => partnerCarData.RelatedPartnerUserId,
        PartnerBookingCancellationTicketData bookingCancellationData => bookingCancellationData.RelatedPartnerUserId,
        _ => null
    };
    public string? CarBrand => Data switch
    {
        PartnerCarTicketData partnerCarData => partnerCarData.CarBrand,
        PartnerBookingCancellationTicketData bookingCancellationData => bookingCancellationData.CarBrand,
        _ => null
    };
    public string? CarModel => Data switch
    {
        PartnerCarTicketData partnerCarData => partnerCarData.CarModel,
        PartnerBookingCancellationTicketData bookingCancellationData => bookingCancellationData.CarModel,
        _ => null
    };
    public int? CarYear => Data is PartnerCarTicketData partnerCarData ? partnerCarData.CarYear : null;
    public string? LicensePlate => Data is PartnerCarTicketData partnerCarData ? partnerCarData.LicensePlate : null;
    public string? Color => Data is PartnerCarTicketData partnerCarData ? partnerCarData.Color : null;
    public int? RequestedPartnerCarStatus => Data is PartnerCarTicketData partnerCarData
        ? partnerCarData.RequestedStatus
        : null;
    public bool? IsActive => Data is PartnerCarTicketData partnerCarData ? partnerCarData.IsActive : null;
    public string? Transmission => Data is PartnerCarTicketData partnerCarData ? partnerCarData.Transmission : null;
    public string? FuelType => Data is PartnerCarTicketData partnerCarData ? partnerCarData.FuelType : null;
    public int? Seats => Data is PartnerCarTicketData partnerCarData ? partnerCarData.Seats : null;
    public int? Doors => Data is PartnerCarTicketData partnerCarData ? partnerCarData.Doors : null;
    public string? BodyType => Data is PartnerCarTicketData partnerCarData ? partnerCarData.BodyType : null;
    public int? Horsepower => Data is PartnerCarTicketData partnerCarData ? partnerCarData.Horsepower : null;
    public IReadOnlyCollection<string> SelectedTags => Data is PartnerCarTicketData partnerCarData ? partnerCarData.SelectedTags : [];
    public IReadOnlyCollection<string> SuggestedTags => Data is PartnerCarTicketData partnerCarData ? partnerCarData.SuggestedTags : [];
    public IReadOnlyCollection<string> ConfirmedTags => Data is PartnerCarTicketData partnerCarData ? partnerCarData.ConfirmedTags : [];
    public string? OwnershipDocumentFileName => Data is PartnerCarTicketData partnerCarData ? partnerCarData.OwnershipDocumentFileName : null;
    public IReadOnlyCollection<PartnerCarTicketImageData> CarImages => Data is PartnerCarTicketData partnerCarData
        ? partnerCarData.CarImages
        : [];
    public int? BookingId => Data switch
    {
        BookingCompletionTicketData bookingCompletionData => bookingCompletionData.BookingId,
        PartnerBookingCancellationTicketData bookingCancellationData => bookingCancellationData.BookingId,
        _ => null
    };
    public DateTimeOffset? PlannedStartTime => Data is BookingCompletionTicketData bookingCompletionData ? bookingCompletionData.PlannedStartTime : null;
    public DateTimeOffset? PlannedEndTime => Data is BookingCompletionTicketData bookingCompletionData ? bookingCompletionData.PlannedEndTime : null;
    public DateTimeOffset? TripStartedAt => Data is BookingCompletionTicketData bookingCompletionData ? bookingCompletionData.TripStartedAt : null;
    public DateTimeOffset? TripCompletedAt => Data is BookingCompletionTicketData bookingCompletionData ? bookingCompletionData.TripCompletedAt : null;
    public decimal? LatePenaltyAmount => Data is BookingCompletionTicketData bookingCompletionData ? bookingCompletionData.LatePenaltyAmount : null;
    public decimal? DamageFineAmount => Data is BookingCompletionTicketData bookingCompletionData ? bookingCompletionData.DamageFineAmount : null;
    public IReadOnlyCollection<BookingCompletionTicketPhotoData> CompletionPhotos => Data is BookingCompletionTicketData bookingCompletionData
        ? bookingCompletionData.CompletionPhotos
        : [];
    public string? BookingStatusSnapshot => Data is PartnerBookingCancellationTicketData bookingCancellationData
        ? bookingCancellationData.BookingStatus
        : null;
    public DateTimeOffset? BookingStartTime => Data is PartnerBookingCancellationTicketData bookingCancellationData
        ? bookingCancellationData.BookingStartTime
        : null;
    public DateTimeOffset? BookingEndTime => Data is PartnerBookingCancellationTicketData bookingCancellationData
        ? bookingCancellationData.BookingEndTime
        : null;
    public string? PartnerReason => Data is PartnerBookingCancellationTicketData bookingCancellationData
        ? bookingCancellationData.PartnerReason
        : null;
    public string? DecisionReason => Data.DecisionReason;
    public Guid? ReviewedByManagerId => Data.ReviewedByManagerId;
    public DateTime? ReviewedAt => Data.ReviewedAt;

    private Ticket() { }

    public Ticket(
        Guid id,
        TicketType ticketType,
        string firstName,
        string lastName,
        string email,
        DateOnly? birthDate,
        string phoneNumber,
        string? identityDocumentFileName,
        string? driverLicenseFileName,
        string? avatarUrl,
        string? companyName,
        string? contactEmail,
        Guid? relatedPartnerUserId,
        string? carBrand,
        string? carModel,
        int? carYear,
        string? licensePlate,
        string? transmission,
        string? fuelType,
        int? seats,
        int? doors,
        string? bodyType,
        int? horsepower,
        IReadOnlyCollection<string>? selectedTags,
        string? ownershipDocumentFileName,
        IReadOnlyCollection<PartnerCarTicketImageData>? carImages,
        DateTime createdAt)
    {
        ValidateTicketType(ticketType);

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        TicketType = ticketType;
        SetEmail(email);
        Data = BuildData(
            ticketType,
            firstName,
            lastName,
            birthDate,
            phoneNumber,
            identityDocumentFileName,
            driverLicenseFileName,
            avatarUrl,
            companyName,
            contactEmail,
            relatedPartnerUserId,
            carBrand,
            carModel,
            carYear,
            licensePlate,
            transmission,
            fuelType,
            seats,
            doors,
            bodyType,
            horsepower,
            selectedTags,
            ownershipDocumentFileName,
            carImages,
            Email);
        Status = TicketStatus.Pending;
        CreatedAt = createdAt;
    }

    public static Ticket CreateBookingCompletion(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        int bookingId,
        DateTimeOffset plannedStartTime,
        DateTimeOffset plannedEndTime,
        DateTimeOffset tripStartedAt,
        DateTimeOffset tripCompletedAt,
        decimal? latePenaltyAmount,
        IReadOnlyCollection<BookingCompletionTicketPhotoData> completionPhotos,
        DateTime createdAt)
    {
        var ticket = new Ticket
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            TicketType = TicketType.BookingCompletion,
            CreatedAt = createdAt
        };

        ticket.SetEmail(email);
        var normalizedName = NormalizeName(firstName, lastName);
        ticket.Data = new BookingCompletionTicketData
        {
            FirstName = normalizedName.FirstName,
            LastName = normalizedName.LastName,
            FullName = normalizedName.FullName,
            PhoneNumber = NormalizePhoneNumber(phoneNumber),
            BookingId = NormalizeBookingId(bookingId),
            PlannedStartTime = plannedStartTime,
            PlannedEndTime = NormalizeTripWindow(plannedStartTime, plannedEndTime, nameof(plannedEndTime)),
            TripStartedAt = NormalizeTripStartedAt(plannedStartTime, tripStartedAt),
            TripCompletedAt = NormalizeTripWindow(tripStartedAt, tripCompletedAt, nameof(tripCompletedAt)),
            LatePenaltyAmount = NormalizeOptionalFineAmount(latePenaltyAmount, nameof(latePenaltyAmount)),
            DamageFineAmount = null,
            CompletionPhotos = NormalizeCompletionPhotos(completionPhotos),
            DecisionReason = null,
            ReviewedByManagerId = null,
            ReviewedAt = null
        };
        ticket.Status = TicketStatus.Pending;
        return ticket;
    }

    public static Ticket CreatePartnerBookingCancellation(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        Guid relatedPartnerUserId,
        int bookingId,
        string carBrand,
        string carModel,
        string bookingStatus,
        DateTimeOffset bookingStartTime,
        DateTimeOffset bookingEndTime,
        string partnerReason,
        DateTime createdAt)
    {
        var ticket = new Ticket
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            TicketType = TicketType.PartnerBookingCancellation,
            CreatedAt = createdAt
        };

        ticket.SetEmail(email);
        var normalizedName = NormalizeName(firstName, lastName);
        ticket.Data = new PartnerBookingCancellationTicketData
        {
            FirstName = normalizedName.FirstName,
            LastName = normalizedName.LastName,
            FullName = normalizedName.FullName,
            PhoneNumber = NormalizePhoneNumber(phoneNumber),
            RelatedPartnerUserId = NormalizePartnerUserId(relatedPartnerUserId),
            BookingId = NormalizeBookingId(bookingId),
            CarBrand = NormalizeCarBrand(carBrand),
            CarModel = NormalizeCarModel(carModel),
            BookingStatus = NormalizeBookingStatusSnapshot(bookingStatus),
            BookingStartTime = NormalizeBookingCancellationWindowStart(bookingStartTime),
            BookingEndTime = NormalizeBookingCancellationWindowEnd(bookingStartTime, bookingEndTime),
            PartnerReason = NormalizeRequired(partnerReason, nameof(partnerReason), 1000),
            DecisionReason = null,
            ReviewedByManagerId = null,
            ReviewedAt = null
        };
        ticket.Status = TicketStatus.Pending;
        return ticket;
    }

    public static Ticket CreatePartnerCar(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        Guid relatedPartnerUserId,
        string requestKind,
        int? partnerCarId,
        string carBrand,
        string carModel,
        int? carYear,
        string licensePlate,
        string? color,
        int? requestedStatus,
        bool? isActive,
        string? transmission,
        string? fuelType,
        int? seats,
        int? doors,
        string? bodyType,
        int? horsepower,
        IReadOnlyCollection<string>? selectedTags,
        string? ownershipDocumentFileName,
        IReadOnlyCollection<PartnerCarTicketImageData>? carImages,
        DateTime createdAt)
    {
        var normalizedRequestKind = NormalizePartnerCarRequestKind(requestKind);
        var ticket = new Ticket
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            TicketType = TicketType.PartnerCar,
            CreatedAt = createdAt
        };

        ticket.SetEmail(email);
        var normalizedName = NormalizeName(firstName, lastName);
        var normalizedFuelType = NormalizeFuelType(fuelType);
        var normalizedTransmission = NormalizeTransmission(transmission);
        var normalizedSeats = NormalizeSeats(seats);
        var normalizedBodyType = NormalizeBodyType(bodyType);
        var normalizedHorsepower = NormalizeHorsepower(horsepower);

        ticket.Data = new PartnerCarTicketData
        {
            FirstName = normalizedName.FirstName,
            LastName = normalizedName.LastName,
            FullName = normalizedName.FullName,
            PhoneNumber = NormalizePhoneNumber(phoneNumber),
            IdentityDocumentFileName = null,
            RequestKind = normalizedRequestKind,
            PartnerCarId = NormalizeOptionalPartnerCarId(partnerCarId, normalizedRequestKind),
            RelatedPartnerUserId = NormalizePartnerUserId(relatedPartnerUserId),
            CarBrand = NormalizeCarBrand(carBrand),
            CarModel = NormalizeCarModel(carModel),
            CarYear = NormalizeCarYear(carYear),
            LicensePlate = NormalizeLicensePlate(licensePlate),
            Color = NormalizeColor(color),
            RequestedStatus = NormalizeRequestedPartnerCarStatus(requestedStatus, normalizedRequestKind),
            IsActive = NormalizePartnerCarIsActive(isActive, normalizedRequestKind),
            Transmission = normalizedTransmission,
            FuelType = normalizedFuelType,
            Seats = normalizedSeats,
            Doors = NormalizeDoors(doors),
            BodyType = normalizedBodyType,
            Horsepower = normalizedHorsepower,
            SelectedTags = NormalizeSemanticTags(selectedTags, nameof(selectedTags)),
            SuggestedTags = SuggestSemanticTags(
                normalizedFuelType,
                normalizedSeats,
                normalizedBodyType,
                normalizedHorsepower),
            ConfirmedTags = [],
            OwnershipDocumentFileName = normalizedRequestKind == PartnerCarRequestKindCreate
                ? NormalizeOwnershipDocumentFileName(ownershipDocumentFileName)
                : NormalizeOptional(ownershipDocumentFileName, nameof(ownershipDocumentFileName), 255) ?? string.Empty,
            CarImages = NormalizePartnerCarImages(carImages),
            DecisionReason = null,
            ReviewedByManagerId = null,
            ReviewedAt = null
        };
        ticket.Status = TicketStatus.Pending;
        return ticket;
    }

    public void UpdatePartnerCarDetailsForReview(
        string? carBrand,
        string? carModel,
        int? carYear,
        string? licensePlate,
        string? color,
        int? requestedStatus,
        bool? isActive,
        string? transmission,
        string? fuelType,
        int? seats,
        int? doors,
        string? bodyType,
        int? horsepower,
        IReadOnlyCollection<string>? confirmedTags)
    {
        EnsurePendingStatus();

        if (TicketType != TicketType.PartnerCar || Data is not PartnerCarTicketData partnerCarData)
        {
            throw new InvalidOperationException("Partner car review fields can be updated only for partner car tickets.");
        }

        var nextCarBrand = carBrand is null ? partnerCarData.CarBrand : NormalizeCarBrand(carBrand);
        var nextCarModel = carModel is null ? partnerCarData.CarModel : NormalizeCarModel(carModel);
        var nextCarYear = carYear is null ? partnerCarData.CarYear : NormalizeCarYear(carYear);
        var nextLicensePlate = licensePlate is null ? partnerCarData.LicensePlate : NormalizeLicensePlate(licensePlate);
        var nextColor = color is null ? partnerCarData.Color : NormalizeColor(color);
        var nextRequestedStatus = requestedStatus is null
            ? partnerCarData.RequestedStatus
            : NormalizeRequestedPartnerCarStatus(requestedStatus, partnerCarData.RequestKind);
        var nextIsActive = isActive.HasValue
            ? NormalizePartnerCarIsActive(isActive, partnerCarData.RequestKind)
            : partnerCarData.IsActive;
        var nextTransmission = transmission is null ? partnerCarData.Transmission : NormalizeTransmission(transmission);
        var nextFuelType = fuelType is null ? partnerCarData.FuelType : NormalizeFuelType(fuelType);
        var nextSeats = seats is null ? partnerCarData.Seats : NormalizeSeats(seats);
        var nextDoors = doors is null ? partnerCarData.Doors : NormalizeDoors(doors);
        var nextBodyType = bodyType is null ? partnerCarData.BodyType : NormalizeBodyType(bodyType);
        var nextHorsepower = horsepower is null ? partnerCarData.Horsepower : NormalizeHorsepower(horsepower);
        var nextSuggestedTags = SuggestSemanticTags(nextFuelType, nextSeats, nextBodyType, nextHorsepower);

        Data = partnerCarData with
        {
            CarBrand = nextCarBrand,
            CarModel = nextCarModel,
            CarYear = nextCarYear,
            LicensePlate = nextLicensePlate,
            Color = nextColor,
            RequestedStatus = nextRequestedStatus,
            IsActive = nextIsActive,
            Transmission = nextTransmission,
            FuelType = nextFuelType,
            Seats = nextSeats,
            Doors = nextDoors,
            BodyType = nextBodyType,
            Horsepower = nextHorsepower,
            SuggestedTags = nextSuggestedTags,
            ConfirmedTags = confirmedTags is null
                ? partnerCarData.ConfirmedTags
                : NormalizeSemanticTags(confirmedTags, nameof(confirmedTags))
        };
    }

    public void Approve(Guid managerId, DateTime reviewedAt)
    {
        EnsurePendingStatus();
        EnsureManagerId(managerId);

        Status = TicketStatus.Approved;
        Data = Data with
        {
            DecisionReason = null,
            ReviewedByManagerId = managerId,
            ReviewedAt = reviewedAt
        };
    }

    public void Reject(Guid managerId, string reason, DateTime reviewedAt)
    {
        EnsurePendingStatus();
        EnsureManagerId(managerId);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Rejection reason is required.", nameof(reason));
        }

        var normalizedReason = reason.Trim();
        if (normalizedReason.Length > 1000)
        {
            throw new ArgumentException("Rejection reason length must not exceed 1000.", nameof(reason));
        }

        Status = TicketStatus.Rejected;
        Data = Data with
        {
            DecisionReason = normalizedReason,
            ReviewedByManagerId = managerId,
            ReviewedAt = reviewedAt
        };
    }

    public void IssueFine(Guid managerId, decimal amount, string comment, DateTime reviewedAt)
    {
        EnsurePendingStatus();
        EnsureManagerId(managerId);

        if (TicketType != TicketType.BookingCompletion || Data is not BookingCompletionTicketData bookingCompletionData)
        {
            throw new InvalidOperationException("Fine can be issued only for booking completion tickets.");
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException("Fine comment is required.", nameof(comment));
        }

        var normalizedComment = comment.Trim();
        if (normalizedComment.Length > 1000)
        {
            throw new ArgumentException("Fine comment length must not exceed 1000.", nameof(comment));
        }

        Status = TicketStatus.FineIssued;
        Data = bookingCompletionData with
        {
            DamageFineAmount = NormalizeRequiredFineAmount(amount, nameof(amount)),
            DecisionReason = normalizedComment,
            ReviewedByManagerId = managerId,
            ReviewedAt = reviewedAt
        };
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > 255)
        {
            throw new ArgumentException("Email length must not exceed 255.", nameof(email));
        }

        Email = normalized;
    }

    private static TicketData BuildData(
        TicketType ticketType,
        string firstName,
        string lastName,
        DateOnly? birthDate,
        string phoneNumber,
        string? identityDocumentFileName,
        string? driverLicenseFileName,
        string? avatarUrl,
        string? companyName,
        string? contactEmail,
        Guid? relatedPartnerUserId,
        string? carBrand,
        string? carModel,
        int? carYear,
        string? licensePlate,
        string? transmission,
        string? fuelType,
        int? seats,
        int? doors,
        string? bodyType,
        int? horsepower,
        IReadOnlyCollection<string>? selectedTags,
        string? ownershipDocumentFileName,
        IReadOnlyCollection<PartnerCarTicketImageData>? carImages,
        string normalizedEmail)
    {
        var normalizedName = NormalizeName(firstName, lastName);
        var normalizedPhoneNumber = NormalizePhoneNumber(phoneNumber);
        var normalizedIdentityDocumentFileName = NormalizeOptional(identityDocumentFileName, nameof(identityDocumentFileName), 255);

        if (ticketType == TicketType.Client)
        {
            return new ClientTicketData
            {
                FirstName = normalizedName.FirstName,
                LastName = normalizedName.LastName,
                FullName = normalizedName.FullName,
                BirthDate = NormalizeClientBirthDate(birthDate),
                PhoneNumber = normalizedPhoneNumber,
                IdentityDocumentFileName = normalizedIdentityDocumentFileName,
                DriverLicenseFileName = NormalizeOptional(driverLicenseFileName, nameof(driverLicenseFileName), 255),
                AvatarUrl = NormalizeAvatarUrl(avatarUrl),
                DecisionReason = null,
                ReviewedByManagerId = null,
                ReviewedAt = null
            };
        }

        if (ticketType == TicketType.Partner)
        {
            return new PartnerTicketData
            {
                FirstName = normalizedName.FirstName,
                LastName = normalizedName.LastName,
                FullName = normalizedName.FullName,
                PhoneNumber = normalizedPhoneNumber,
                IdentityDocumentFileName = normalizedIdentityDocumentFileName,
                CompanyName = NormalizeCompanyName(companyName, normalizedName.FullName),
                ContactEmail = NormalizeContactEmail(contactEmail, normalizedEmail),
                DecisionReason = null,
                ReviewedByManagerId = null,
                ReviewedAt = null
            };
        }

        if (ticketType != TicketType.PartnerCar)
        {
            throw new ArgumentException("Ticket type is invalid.", nameof(ticketType));
        }

        return new PartnerCarTicketData
        {
            FirstName = normalizedName.FirstName,
            LastName = normalizedName.LastName,
            FullName = normalizedName.FullName,
            PhoneNumber = normalizedPhoneNumber,
            IdentityDocumentFileName = null,
            RelatedPartnerUserId = NormalizePartnerUserId(relatedPartnerUserId),
            CarBrand = NormalizeCarBrand(carBrand),
            CarModel = NormalizeCarModel(carModel),
            CarYear = NormalizeCarYear(carYear),
            LicensePlate = NormalizeLicensePlate(licensePlate),
            Transmission = NormalizeTransmission(transmission),
            FuelType = NormalizeFuelType(fuelType),
            Seats = NormalizeSeats(seats),
            Doors = NormalizeDoors(doors),
            BodyType = NormalizeBodyType(bodyType),
            Horsepower = NormalizeHorsepower(horsepower),
            SelectedTags = NormalizeSemanticTags(selectedTags, nameof(selectedTags)),
            SuggestedTags = SuggestSemanticTags(
                NormalizeFuelType(fuelType),
                NormalizeSeats(seats),
                NormalizeBodyType(bodyType),
                NormalizeHorsepower(horsepower)),
            ConfirmedTags = [],
            OwnershipDocumentFileName = NormalizeOwnershipDocumentFileName(ownershipDocumentFileName),
            CarImages = NormalizePartnerCarImages(carImages),
            DecisionReason = null,
            ReviewedByManagerId = null,
            ReviewedAt = null
        };
    }

    private static (string FirstName, string LastName, string FullName) NormalizeName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.", nameof(lastName));
        }

        var normalizedFirstName = firstName.Trim();
        var normalizedLastName = lastName.Trim();
        if (normalizedFirstName.Length > 100)
        {
            throw new ArgumentException("First name length must not exceed 100.", nameof(firstName));
        }

        if (normalizedLastName.Length > 100)
        {
            throw new ArgumentException("Last name length must not exceed 100.", nameof(lastName));
        }

        var fullName = $"{normalizedFirstName} {normalizedLastName}".Trim();
        if (fullName.Length > 300)
        {
            throw new ArgumentException("Full name length must not exceed 300.", nameof(lastName));
        }

        return (normalizedFirstName, normalizedLastName, fullName);
    }

    private static DateOnly NormalizeClientBirthDate(DateOnly? birthDate)
    {
        if (birthDate is null || birthDate == default)
        {
            throw new ArgumentException("Birth date is required.", nameof(birthDate));
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));
        }

        return birthDate.Value;
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));
        }

        var normalized = phoneNumber.Trim();
        if (normalized.Length > 32)
        {
            throw new ArgumentException("Phone number length must not exceed 32.", nameof(phoneNumber));
        }

        return normalized;
    }

    private static string NormalizeCompanyName(string? companyName, string fallbackFullName)
    {
        var candidate = string.IsNullOrWhiteSpace(companyName) ? fallbackFullName : companyName.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
        {
            throw new ArgumentException("Company name is required for partner tickets.", nameof(companyName));
        }

        if (candidate.Length > 300)
        {
            throw new ArgumentException("Company name length must not exceed 300.", nameof(companyName));
        }

        return candidate;
    }

    private static string NormalizeContactEmail(string? contactEmail, string fallbackEmail)
    {
        var candidate = string.IsNullOrWhiteSpace(contactEmail) ? fallbackEmail : contactEmail.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(candidate))
        {
            throw new ArgumentException("Contact email is required for partner tickets.", nameof(contactEmail));
        }

        if (candidate.Length > 255)
        {
            throw new ArgumentException("Contact email length must not exceed 255.", nameof(contactEmail));
        }

        return candidate;
    }

    private static Guid NormalizePartnerUserId(Guid? relatedPartnerUserId)
    {
        if (relatedPartnerUserId is null || relatedPartnerUserId == Guid.Empty)
        {
            throw new ArgumentException("Related partner user id is required for partner car tickets.", nameof(relatedPartnerUserId));
        }

        return relatedPartnerUserId.Value;
    }

    private static string NormalizeCarBrand(string? carBrand)
    {
        return NormalizeRequired(carBrand, nameof(carBrand), 100);
    }

    private static string NormalizeCarModel(string? carModel)
    {
        return NormalizeRequired(carModel, nameof(carModel), 100);
    }

    private static int NormalizeCarYear(int? carYear)
    {
        if (!carYear.HasValue)
        {
            throw new ArgumentException("carYear is required.", nameof(carYear));
        }

        var maxAllowedCarYear = DateTime.UtcNow.Year + 1;
        if (carYear.Value < 1886 || carYear.Value > maxAllowedCarYear)
        {
            throw new ArgumentException($"carYear must be between 1886 and {maxAllowedCarYear}.", nameof(carYear));
        }

        return carYear.Value;
    }

    private static string NormalizeLicensePlate(string? licensePlate)
    {
        return NormalizeRequired(licensePlate, nameof(licensePlate), 20).ToUpperInvariant();
    }

    private static string NormalizePartnerCarRequestKind(string? requestKind)
    {
        var normalized = NormalizeOptional(requestKind, nameof(requestKind), 20)?.ToLowerInvariant();
        return normalized switch
        {
            null or "" or PartnerCarRequestKindCreate => PartnerCarRequestKindCreate,
            PartnerCarRequestKindUpdate => PartnerCarRequestKindUpdate,
            _ => throw new ArgumentException("partnerCarRequestKind must be either 'create' or 'update'.", nameof(requestKind))
        };
    }

    private static int? NormalizeOptionalPartnerCarId(int? partnerCarId, string requestKind)
    {
        if (requestKind != PartnerCarRequestKindUpdate)
        {
            return null;
        }

        if (!partnerCarId.HasValue || partnerCarId.Value <= 0)
        {
            throw new ArgumentException("partnerCarId is required for partner car update requests.", nameof(partnerCarId));
        }

        return partnerCarId.Value;
    }

    private static string? NormalizeColor(string? color)
    {
        return NormalizeOptional(color, nameof(color), 50);
    }

    private static int? NormalizeRequestedPartnerCarStatus(int? requestedStatus, string requestKind)
    {
        if (!requestedStatus.HasValue)
        {
            return requestKind == PartnerCarRequestKindCreate ? 0 : null;
        }

        if (requestedStatus.Value < 0 || requestedStatus.Value > 3)
        {
            throw new ArgumentException("requestedStatus must be between 0 and 3.", nameof(requestedStatus));
        }

        return requestedStatus.Value;
    }

    private static bool? NormalizePartnerCarIsActive(bool? isActive, string requestKind)
    {
        if (isActive.HasValue)
        {
            return isActive.Value;
        }

        return requestKind == PartnerCarRequestKindCreate ? true : null;
    }

    private static string? NormalizeTransmission(string? transmission)
    {
        var normalized = NormalizeOptional(transmission, nameof(transmission), 50);
        return normalized?.ToLowerInvariant();
    }

    private static string? NormalizeFuelType(string? fuelType)
    {
        var normalized = NormalizeOptional(fuelType, nameof(fuelType), 50);
        return normalized?.ToLowerInvariant();
    }

    private static int? NormalizeSeats(int? seats)
    {
        if (!seats.HasValue)
        {
            return null;
        }

        if (seats.Value <= 0 || seats.Value > 20)
        {
            throw new ArgumentException("seats must be between 1 and 20.", nameof(seats));
        }

        return seats.Value;
    }

    private static int? NormalizeDoors(int? doors)
    {
        if (!doors.HasValue)
        {
            return null;
        }

        if (doors.Value <= 0 || doors.Value > 6)
        {
            throw new ArgumentException("doors must be between 1 and 6.", nameof(doors));
        }

        return doors.Value;
    }

    private static string? NormalizeBodyType(string? bodyType)
    {
        var normalized = NormalizeOptional(bodyType, nameof(bodyType), 50);
        if (normalized is null)
        {
            return null;
        }

        return normalized.Trim().ToLowerInvariant() switch
        {
            "внедорожник" => "suv",
            "кроссовер" => "crossover",
            "минивэн" => "minivan",
            _ => normalized.ToLowerInvariant()
        };
    }

    private static int? NormalizeHorsepower(int? horsepower)
    {
        if (!horsepower.HasValue)
        {
            return null;
        }

        if (horsepower.Value <= 0 || horsepower.Value > 3000)
        {
            throw new ArgumentException("horsepower must be between 1 and 3000.", nameof(horsepower));
        }

        return horsepower.Value;
    }

    private static IReadOnlyCollection<string> NormalizeSemanticTags(
        IReadOnlyCollection<string>? tags,
        string paramName)
    {
        if (tags is null || tags.Count == 0)
        {
            return [];
        }

        var normalized = new List<string>(tags.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in tags)
        {
            var normalizedTag = NormalizeSemanticTag(tag, paramName);
            if (seen.Add(normalizedTag))
            {
                normalized.Add(normalizedTag);
            }
        }

        if (normalized.Count > AllowedSemanticTags.Count)
        {
            throw new ArgumentException($"No more than {AllowedSemanticTags.Count} semantic tags are allowed.", paramName);
        }

        return normalized;
    }

    private static string NormalizeSemanticTag(string? tag, string paramName)
    {
        var normalized = NormalizeRequired(tag, paramName, 50).ToLowerInvariant();
        normalized = normalized switch
        {
            "эконом" => "econom",
            "комфорт" => "comfort",
            "бизнес" => "business",
            "спортивная" => "sport",
            "внедорожник" => "suv",
            "электро" => "electric",
            "семейная" => "family",
            _ => normalized
        };

        if (!AllowedSemanticTags.Contains(normalized))
        {
            throw new ArgumentException($"Unsupported semantic tag '{tag}'.", paramName);
        }

        return normalized;
    }

    private static IReadOnlyCollection<string> SuggestSemanticTags(
        string? fuelType,
        int? seats,
        string? bodyType,
        int? horsepower)
    {
        var suggested = new List<string>();

        void Add(string tag)
        {
            if (!suggested.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                suggested.Add(tag);
            }
        }

        if (string.Equals(fuelType, "electric", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fuelType, "ev", StringComparison.OrdinalIgnoreCase))
        {
            Add("electric");
        }

        if (string.Equals(bodyType, "suv", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(bodyType, "crossover", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(bodyType, "offroad", StringComparison.OrdinalIgnoreCase))
        {
            Add("suv");
        }

        if (horsepower.HasValue && horsepower.Value >= 250)
        {
            Add("sport");
        }

        if ((seats.HasValue && seats.Value >= 5) ||
            string.Equals(bodyType, "minivan", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(bodyType, "suv", StringComparison.OrdinalIgnoreCase))
        {
            Add("family");
        }

        return suggested;
    }

    private static decimal NormalizePrice(decimal? value, string paramName)
    {
        if (!value.HasValue)
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        if (value.Value <= 0m)
        {
            throw new ArgumentException($"{paramName} must be greater than 0.", paramName);
        }

        if (value.Value > 1_000_000m)
        {
            throw new ArgumentException($"{paramName} must not exceed 1000000.", paramName);
        }

        var normalized = decimal.Round(value.Value, 2, MidpointRounding.AwayFromZero);
        if (normalized <= 0m)
        {
            throw new ArgumentException($"{paramName} must be greater than 0.", paramName);
        }

        return normalized;
    }

    private static string NormalizeOwnershipDocumentFileName(string? ownershipDocumentFileName)
    {
        return NormalizeRequired(ownershipDocumentFileName, nameof(ownershipDocumentFileName), 255);
    }

    private static IReadOnlyCollection<PartnerCarTicketImageData> NormalizePartnerCarImages(
        IReadOnlyCollection<PartnerCarTicketImageData>? carImages)
    {
        if (carImages is null || carImages.Count == 0)
        {
            throw new ArgumentException("At least one partner car image is required.", nameof(carImages));
        }

        if (carImages.Count > 12)
        {
            throw new ArgumentException("No more than 12 partner car images are allowed.", nameof(carImages));
        }

        return carImages
            .Select(image => new PartnerCarTicketImageData
            {
                ImageId = NormalizeRequired(image.ImageId, nameof(image.ImageId), 255),
                ImageUrl = NormalizeImageUrl(image.ImageUrl, nameof(image.ImageUrl)),
                ImageType = NormalizePartnerCarImageType(image.ImageType, nameof(image.ImageType))
            })
            .ToArray();
    }

    private static string NormalizePartnerCarImageType(string? imageType, string paramName)
    {
        var normalized = NormalizeRequired(imageType, paramName, 32).ToLowerInvariant();
        return normalized switch
        {
            "front" => "front",
            "back" => "back",
            "side" => "side",
            "interior" => "interior",
            "general" => "general",
            _ => throw new ArgumentException($"Unsupported partner car image type '{imageType}'.", paramName)
        };
    }

    private static string NormalizeImageUrl(string? value, string paramName)
    {
        var normalized = NormalizeRequired(value, paramName, 2048);
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"{paramName} must be a valid absolute URL.", paramName);
        }

        return normalized;
    }

    private static string? NormalizeAvatarUrl(string? avatarUrl)
    {
        var normalized = NormalizeOptional(avatarUrl, nameof(avatarUrl), 1024);
        if (normalized is not null && !Uri.TryCreate(normalized, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Avatar url must be a valid absolute URL.", nameof(avatarUrl));
        }

        return normalized;
    }

    private static int NormalizeBookingId(int bookingId)
    {
        if (bookingId <= 0)
        {
            throw new ArgumentException("bookingId must be greater than zero.", nameof(bookingId));
        }

        return bookingId;
    }

    private static string NormalizeBookingStatusSnapshot(string? bookingStatus)
    {
        var normalized = NormalizeRequired(bookingStatus, nameof(bookingStatus), 64).ToLowerInvariant();
        return normalized switch
        {
            "pending" => "pending",
            "confirmed" => "confirmed",
            "active" => "active",
            "awaitingreview" or "awaiting_review" => "awaitingreview",
            "completed" => "completed",
            "canceled" => "canceled",
            _ => throw new ArgumentException($"Unsupported booking status snapshot '{bookingStatus}'.", nameof(bookingStatus))
        };
    }

    private static DateTimeOffset NormalizeBookingCancellationWindowStart(DateTimeOffset bookingStartTime)
    {
        if (bookingStartTime == default)
        {
            throw new ArgumentException("bookingStartTime is required.", nameof(bookingStartTime));
        }

        return bookingStartTime;
    }

    private static DateTimeOffset NormalizeBookingCancellationWindowEnd(
        DateTimeOffset bookingStartTime,
        DateTimeOffset bookingEndTime)
    {
        if (bookingEndTime == default)
        {
            throw new ArgumentException("bookingEndTime is required.", nameof(bookingEndTime));
        }

        if (bookingEndTime < bookingStartTime)
        {
            throw new ArgumentException("bookingEndTime must be greater than or equal to bookingStartTime.", nameof(bookingEndTime));
        }

        return bookingEndTime;
    }

    private static DateTimeOffset NormalizeTripWindow(DateTimeOffset start, DateTimeOffset end, string paramName)
    {
        if (start == default)
        {
            throw new ArgumentException("Trip window start is required.", nameof(start));
        }

        if (end == default)
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        if (end < start)
        {
            throw new ArgumentException($"{paramName} must be greater than or equal to the previous trip timestamp.", paramName);
        }

        return end;
    }

    private static DateTimeOffset NormalizeTripStartedAt(DateTimeOffset plannedStartTime, DateTimeOffset tripStartedAt)
    {
        if (plannedStartTime == default)
        {
            throw new ArgumentException("plannedStartTime is required.", nameof(plannedStartTime));
        }

        if (tripStartedAt == default)
        {
            throw new ArgumentException("tripStartedAt is required.", nameof(tripStartedAt));
        }

        if (tripStartedAt < plannedStartTime.AddMinutes(-15))
        {
            throw new ArgumentException("tripStartedAt cannot be earlier than 15 minutes before plannedStartTime.", nameof(tripStartedAt));
        }

        return tripStartedAt;
    }

    private static decimal? NormalizeOptionalFineAmount(decimal? amount, string paramName)
    {
        if (!amount.HasValue)
        {
            return null;
        }

        return NormalizeRequiredFineAmount(amount.Value, paramName);
    }

    private static decimal NormalizeRequiredFineAmount(decimal amount, string paramName)
    {
        if (amount <= 0m)
        {
            throw new ArgumentException($"{paramName} must be greater than 0.", paramName);
        }

        if (amount > 10_000_000m)
        {
            throw new ArgumentException($"{paramName} must not exceed 10000000.", paramName);
        }

        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyCollection<BookingCompletionTicketPhotoData> NormalizeCompletionPhotos(
        IReadOnlyCollection<BookingCompletionTicketPhotoData>? completionPhotos)
    {
        if (completionPhotos is null || completionPhotos.Count != 5)
        {
            throw new ArgumentException("Exactly 5 completion photos are required.", nameof(completionPhotos));
        }

        var normalized = completionPhotos
            .Select(photo => new BookingCompletionTicketPhotoData
            {
                Slot = NormalizeRequired(photo.Slot, nameof(photo.Slot), 64),
                FileName = NormalizeRequired(photo.FileName, nameof(photo.FileName), 255)
            })
            .ToArray();

        var requiredSlots = new[]
        {
            "front",
            "back",
            "side_left",
            "side_right",
            "interior"
        };

        var slotSet = normalized
            .Select(photo => photo.Slot.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.Ordinal);
        if (!requiredSlots.All(slotSet.Contains))
        {
            throw new ArgumentException("Completion photos must include front, back, side_left, side_right and interior slots.", nameof(completionPhotos));
        }

        if (slotSet.Count != requiredSlots.Length)
        {
            throw new ArgumentException("Completion photo slots must be unique.", nameof(completionPhotos));
        }

        return normalized;
    }

    private static string NormalizeRequired(string? value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} length must not exceed {maxLength}.", paramName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} length must not exceed {maxLength}.", paramName);
        }

        return normalized;
    }

    private void EnsurePendingStatus()
    {
        if (Status != TicketStatus.Pending)
        {
            throw new InvalidOperationException("Only pending tickets can be reviewed.");
        }
    }

    private static void EnsureManagerId(Guid managerId)
    {
        if (managerId == Guid.Empty)
        {
            throw new ArgumentException("Manager id is required.", nameof(managerId));
        }
    }

    private static void ValidateTicketType(TicketType ticketType)
    {
        if (!Enum.IsDefined(ticketType))
        {
            throw new ArgumentException("Ticket type is invalid.", nameof(ticketType));
        }
    }
}
