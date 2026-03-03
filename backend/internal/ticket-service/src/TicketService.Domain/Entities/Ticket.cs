using TicketService.Domain.Enums;

namespace TicketService.Domain.Entities;

public sealed class Ticket
{
    public Guid Id { get; private set; }
    public TicketType TicketType { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateOnly? BirthDate { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? IdentityDocumentFileName { get; private set; }
    public string? DriverLicenseFileName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public TicketStatus Status { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ReviewedByManagerId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

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
        DateTime createdAt)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        TicketType = ticketType;
        SetName(firstName, lastName);
        SetEmail(email);
        SetBirthDate(ticketType, birthDate);
        SetPhoneNumber(phoneNumber);
        IdentityDocumentFileName = NormalizeOptional(identityDocumentFileName, nameof(identityDocumentFileName), 255);
        DriverLicenseFileName = ticketType == TicketType.Client
            ? NormalizeOptional(driverLicenseFileName, nameof(driverLicenseFileName), 255)
            : null;
        SetAvatarUrl(avatarUrl);
        Status = TicketStatus.Pending;
        CreatedAt = createdAt;
    }

    public void Approve(Guid managerId, DateTime reviewedAt)
    {
        EnsurePendingStatus();
        EnsureManagerId(managerId);

        Status = TicketStatus.Approved;
        DecisionReason = null;
        ReviewedByManagerId = managerId;
        ReviewedAt = reviewedAt;
    }

    public void Reject(Guid managerId, string reason, DateTime reviewedAt)
    {
        EnsurePendingStatus();
        EnsureManagerId(managerId);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Rejection reason is required.", nameof(reason));
        }

        Status = TicketStatus.Rejected;
        DecisionReason = reason.Trim();
        ReviewedByManagerId = managerId;
        ReviewedAt = reviewedAt;
    }

    private void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.", nameof(lastName));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        if (FirstName.Length > 100)
        {
            throw new ArgumentException("First name length must not exceed 100.", nameof(firstName));
        }

        if (LastName.Length > 100)
        {
            throw new ArgumentException("Last name length must not exceed 100.", nameof(lastName));
        }

        FullName = $"{FirstName} {LastName}".Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
    }

    private void SetBirthDate(TicketType ticketType, DateOnly? birthDate)
    {
        if (ticketType == TicketType.Partner)
        {
            BirthDate = birthDate;
            return;
        }

        if (birthDate is null || birthDate == default)
        {
            throw new ArgumentException("Birth date is required.", nameof(birthDate));
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));
        }

        BirthDate = birthDate;
    }

    private void SetPhoneNumber(string phoneNumber)
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

        PhoneNumber = normalized;
    }

    private void SetAvatarUrl(string? avatarUrl)
    {
        var normalized = NormalizeOptional(avatarUrl, nameof(avatarUrl), 1024);
        if (normalized is not null && !Uri.TryCreate(normalized, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Avatar url must be a valid absolute URL.", nameof(avatarUrl));
        }

        AvatarUrl = normalized;
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
}
