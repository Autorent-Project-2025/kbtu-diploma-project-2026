using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientService.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(client => client.Id);

        builder.Property(client => client.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(client => client.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(client => client.CreatedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(client => client.BirthDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(client => client.IdentityDocumentFileName)
            .HasMaxLength(255);

        builder.Property(client => client.DriverLicenseFileName)
            .HasMaxLength(255);

        builder.Property(client => client.RelatedUserId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(client => client.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(client => client.AvatarUrl)
            .HasMaxLength(1024);

        builder.HasIndex(client => client.RelatedUserId)
            .IsUnique();
    }
}
