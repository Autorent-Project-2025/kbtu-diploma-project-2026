using TicketService.Domain.Enums;

namespace TicketService.Domain.Entities;

public sealed class Ticket
{
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
    public Guid? RelatedPartnerUserId => Data is PartnerCarTicketData partnerCarData ? partnerCarData.RelatedPartnerUserId : null;
    public string? CarBrand => Data is PartnerCarTicketData partnerCarData ? partnerCarData.CarBrand : null;
    public string? CarModel => Data is PartnerCarTicketData partnerCarData ? partnerCarData.CarModel : null;
    public int? CarYear => Data is PartnerCarTicketData partnerCarData ? partnerCarData.CarYear : null;
    public string? LicensePlate => Data is PartnerCarTicketData partnerCarData ? partnerCarData.LicensePlate : null;
    public decimal? PriceHour => Data is PartnerCarTicketData partnerCarData ? partnerCarData.PriceHour : null;
    public decimal? PriceDay => Data is PartnerCarTicketData partnerCarData ? partnerCarData.PriceDay : null;
    public string? OwnershipDocumentFileName => Data is PartnerCarTicketData partnerCarData ? partnerCarData.OwnershipDocumentFileName : null;
    public IReadOnlyCollection<PartnerCarTicketImageData> CarImages => Data is PartnerCarTicketData partnerCarData
        ? partnerCarData.CarImages
        : [];
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
        decimal? priceHour,
        decimal? priceDay,
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
            priceHour,
            priceDay,
            ownershipDocumentFileName,
            carImages,
            Email);
        Status = TicketStatus.Pending;
        CreatedAt = createdAt;
    }

    public void UpdatePartnerCarDetailsForReview(
        string? carBrand,
        string? carModel,
        int? carYear,
        string? licensePlate,
        decimal? priceHour,
        decimal? priceDay,
        string? email)
    {
        EnsurePendingStatus();

        if (TicketType != TicketType.PartnerCar || Data is not PartnerCarTicketData partnerCarData)
        {
            throw new InvalidOperationException("Partner car review fields can be updated only for partner car tickets.");
        }

        var shouldUpdateData =
            carBrand is not null ||
            carModel is not null ||
            carYear is not null ||
            licensePlate is not null ||
            priceHour is not null ||
            priceDay is not null;
        if (shouldUpdateData)
        {
            Data = partnerCarData with
            {
                CarBrand = carBrand is null ? partnerCarData.CarBrand : NormalizeCarBrand(carBrand),
                CarModel = carModel is null ? partnerCarData.CarModel : NormalizeCarModel(carModel),
                CarYear = carYear is null ? partnerCarData.CarYear : NormalizeCarYear(carYear),
                LicensePlate = licensePlate is null ? partnerCarData.LicensePlate : NormalizeLicensePlate(licensePlate),
                PriceHour = priceHour is null ? partnerCarData.PriceHour : NormalizePrice(priceHour, nameof(priceHour)),
                PriceDay = priceDay is null ? partnerCarData.PriceDay : NormalizePrice(priceDay, nameof(priceDay))
            };
        }

        if (email is not null)
        {
            SetEmail(email);
        }
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
        decimal? priceHour,
        decimal? priceDay,
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
            PriceHour = NormalizePrice(priceHour, nameof(priceHour)),
            PriceDay = NormalizePrice(priceDay, nameof(priceDay)),
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
                ImageUrl = NormalizeImageUrl(image.ImageUrl, nameof(image.ImageUrl))
            })
            .ToArray();
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
