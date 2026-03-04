using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static readonly ValueConverter<TicketData, string> TicketDataConverter = new(
        data => SerializeTicketData(data),
        json => DeserializeTicketData(json));

    private static readonly ValueComparer<TicketData> TicketDataComparer = new(
        (left, right) => SerializeTicketData(left) == SerializeTicketData(right),
        value => StringComparer.Ordinal.GetHashCode(SerializeTicketData(value)),
        value => DeserializeTicketData(SerializeTicketData(value)));

    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(ticket => ticket.Id);
        builder.Property(ticket => ticket.Id).HasColumnName("id");
        builder.Property(ticket => ticket.TicketType).HasColumnName("ticket_type").HasConversion<int>().IsRequired();
        builder.Property(ticket => ticket.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(ticket => ticket.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(ticket => ticket.CreatedAt).HasColumnName("created_at").IsRequired();

        var dataProperty = builder.Property(ticket => ticket.Data)
            .HasColumnName("data")
            .HasColumnType("jsonb")
            .HasConversion(TicketDataConverter)
            .IsRequired();

        dataProperty.Metadata.SetValueComparer(TicketDataComparer);

        builder.Ignore(ticket => ticket.FirstName);
        builder.Ignore(ticket => ticket.LastName);
        builder.Ignore(ticket => ticket.FullName);
        builder.Ignore(ticket => ticket.BirthDate);
        builder.Ignore(ticket => ticket.PhoneNumber);
        builder.Ignore(ticket => ticket.IdentityDocumentFileName);
        builder.Ignore(ticket => ticket.DriverLicenseFileName);
        builder.Ignore(ticket => ticket.AvatarUrl);
        builder.Ignore(ticket => ticket.CompanyName);
        builder.Ignore(ticket => ticket.ContactEmail);
        builder.Ignore(ticket => ticket.RelatedPartnerUserId);
        builder.Ignore(ticket => ticket.CarBrand);
        builder.Ignore(ticket => ticket.CarModel);
        builder.Ignore(ticket => ticket.LicensePlate);
        builder.Ignore(ticket => ticket.OwnershipDocumentFileName);
        builder.Ignore(ticket => ticket.CarImages);
        builder.Ignore(ticket => ticket.DecisionReason);
        builder.Ignore(ticket => ticket.ReviewedByManagerId);
        builder.Ignore(ticket => ticket.ReviewedAt);

        builder.HasIndex(ticket => ticket.Status);
        builder.HasIndex(ticket => ticket.TicketType);
        builder.HasIndex(ticket => ticket.Email);
        builder.HasIndex(ticket => ticket.CreatedAt);
    }

    private static string SerializeTicketData(TicketData? data)
    {
        return JsonSerializer.Serialize(data ?? new ClientTicketData(), SerializerOptions);
    }

    private static TicketData DeserializeTicketData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ClientTicketData();
        }

        try
        {
            return JsonSerializer.Deserialize<TicketData>(json, SerializerOptions) ?? DeserializeLegacyTicketData(json);
        }
        catch (JsonException)
        {
            return DeserializeLegacyTicketData(json);
        }
        catch (NotSupportedException)
        {
            return DeserializeLegacyTicketData(json);
        }
    }

    private static TicketData DeserializeLegacyTicketData(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var firstName = GetString(root, "firstName");
        var lastName = GetString(root, "lastName");
        var fullName = GetString(root, "fullName");
        var phoneNumber = GetString(root, "phoneNumber");
        var identityDocumentFileName = GetOptionalString(root, "identityDocumentFileName");
        var decisionReason = GetOptionalString(root, "decisionReason");
        var reviewedByManagerId = GetOptionalGuid(root, "reviewedByManagerId");
        var reviewedAt = GetOptionalDateTime(root, "reviewedAt");

        if (TryGetDateOnly(root, "birthDate", out var birthDate))
        {
            return new ClientTicketData
            {
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                IdentityDocumentFileName = identityDocumentFileName,
                BirthDate = birthDate,
                DriverLicenseFileName = GetOptionalString(root, "driverLicenseFileName"),
                AvatarUrl = GetOptionalString(root, "avatarUrl"),
                DecisionReason = decisionReason,
                ReviewedByManagerId = reviewedByManagerId,
                ReviewedAt = reviewedAt
            };
        }

        var relatedPartnerUserId = GetOptionalGuid(root, "relatedPartnerUserId");
        var ownershipDocumentFileName = GetOptionalString(root, "ownershipDocumentFileName");
        var carBrand = GetOptionalString(root, "carBrand");
        var carModel = GetOptionalString(root, "carModel");
        var licensePlate = GetOptionalString(root, "licensePlate");
        if (relatedPartnerUserId.HasValue ||
            !string.IsNullOrWhiteSpace(ownershipDocumentFileName) ||
            !string.IsNullOrWhiteSpace(carBrand) ||
            !string.IsNullOrWhiteSpace(carModel) ||
            !string.IsNullOrWhiteSpace(licensePlate))
        {
            return new PartnerCarTicketData
            {
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                IdentityDocumentFileName = identityDocumentFileName,
                RelatedPartnerUserId = relatedPartnerUserId ?? Guid.Empty,
                CarBrand = carBrand ?? string.Empty,
                CarModel = carModel ?? string.Empty,
                LicensePlate = licensePlate ?? string.Empty,
                OwnershipDocumentFileName = ownershipDocumentFileName ?? string.Empty,
                CarImages = GetPartnerCarImages(root),
                DecisionReason = decisionReason,
                ReviewedByManagerId = reviewedByManagerId,
                ReviewedAt = reviewedAt
            };
        }

        return new PartnerTicketData
        {
            FirstName = firstName,
            LastName = lastName,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            IdentityDocumentFileName = identityDocumentFileName,
            CompanyName = GetString(root, "companyName", fullName),
            ContactEmail = GetString(root, "contactEmail"),
            DecisionReason = decisionReason,
            ReviewedByManagerId = reviewedByManagerId,
            ReviewedAt = reviewedAt
        };
    }

    private static string GetString(JsonElement root, string propertyName, string defaultValue = "")
    {
        if (root.TryGetProperty(propertyName, out var value) &&
            value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? defaultValue;
        }

        return defaultValue;
    }

    private static string? GetOptionalString(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var value) &&
            value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static IReadOnlyCollection<PartnerCarTicketImageData> GetPartnerCarImages(JsonElement root)
    {
        if (!root.TryGetProperty("carImages", out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var images = new List<PartnerCarTicketImageData>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            images.Add(new PartnerCarTicketImageData
            {
                ImageId = GetString(item, "imageId"),
                ImageUrl = GetString(item, "imageUrl")
            });
        }

        return images;
    }

    private static Guid? GetOptionalGuid(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var value) &&
            value.ValueKind == JsonValueKind.String &&
            Guid.TryParse(value.GetString(), out var guidValue))
        {
            return guidValue;
        }

        return null;
    }

    private static DateTime? GetOptionalDateTime(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var value))
        {
            if (value.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(value.GetString(), out var dateTimeValue))
            {
                return dateTimeValue;
            }

            if (value.ValueKind == JsonValueKind.Number &&
                value.TryGetInt64(out var unixMilliseconds))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;
            }
        }

        return null;
    }

    private static bool TryGetDateOnly(JsonElement root, string propertyName, out DateOnly dateOnly)
    {
        dateOnly = default;
        if (!root.TryGetProperty(propertyName, out var value))
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.String &&
            DateOnly.TryParse(value.GetString(), out var parsedDate))
        {
            dateOnly = parsedDate;
            return true;
        }

        return false;
    }
}
