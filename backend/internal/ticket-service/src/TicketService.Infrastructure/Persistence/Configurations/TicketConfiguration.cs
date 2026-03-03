using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(ticket => ticket.Id);
        builder.Property(ticket => ticket.Id).HasColumnName("id");
        builder.Property(ticket => ticket.FullName).HasColumnName("full_name").HasMaxLength(300).IsRequired();
        builder.Property(ticket => ticket.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(ticket => ticket.BirthDate).HasColumnName("birth_date").HasColumnType("date").IsRequired();
        builder.Property(ticket => ticket.PhoneNumber).HasColumnName("phone_number").HasMaxLength(32).IsRequired();
        builder.Property(ticket => ticket.IdentityDocumentFileName).HasColumnName("identity_document_file_name").HasMaxLength(255);
        builder.Property(ticket => ticket.DriverLicenseFileName).HasColumnName("driver_license_file_name").HasMaxLength(255);
        builder.Property(ticket => ticket.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(1024);
        builder.Property(ticket => ticket.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(ticket => ticket.DecisionReason).HasColumnName("decision_reason").HasMaxLength(1000);
        builder.Property(ticket => ticket.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(ticket => ticket.ReviewedByManagerId).HasColumnName("reviewed_by_manager_id");
        builder.Property(ticket => ticket.ReviewedAt).HasColumnName("reviewed_at");

        builder.HasIndex(ticket => ticket.Status);
        builder.HasIndex(ticket => ticket.Email);
        builder.HasIndex(ticket => ticket.CreatedAt);
    }
}
