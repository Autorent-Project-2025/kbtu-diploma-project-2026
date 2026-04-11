using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations;

public sealed class ComplaintBookingAccessRequestConfiguration
    : IEntityTypeConfiguration<ComplaintBookingAccessRequest>
{
    public void Configure(EntityTypeBuilder<ComplaintBookingAccessRequest> builder)
    {
        builder.ToTable("complaint_booking_access_requests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.ComplaintId).HasColumnName("complaint_id").IsRequired();
        builder.Property(r => r.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(r => r.RequestedByManagerId).HasColumnName("requested_by_manager_id").IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(r => r.Reason).HasColumnName("reason").HasMaxLength(2000).IsRequired();
        builder.Property(r => r.RequestedAt).HasColumnName("requested_at").IsRequired();

        builder.Property(r => r.ReviewedBySupermanagerId).HasColumnName("reviewed_by_supermanager_id");
        builder.Property(r => r.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(r => r.DecisionNote).HasColumnName("decision_note").HasMaxLength(2000);
        builder.Property(r => r.ExpiresAt).HasColumnName("expires_at");

        builder.HasIndex(r => r.ComplaintId);
        builder.HasIndex(r => r.BookingId);
        builder.HasIndex(r => r.RequestedByManagerId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => new { r.RequestedByManagerId, r.BookingId, r.Status });
    }
}
