using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations;

public sealed class ComplaintReopenRequestConfiguration : IEntityTypeConfiguration<ComplaintReopenRequest>
{
    public void Configure(EntityTypeBuilder<ComplaintReopenRequest> builder)
    {
        builder.ToTable("complaint_reopen_requests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.ComplaintId).HasColumnName("complaint_id").IsRequired();
        builder.Property(r => r.RequestedByUserId).HasColumnName("requested_by_user_id").IsRequired();
        builder.Property(r => r.Reason).HasColumnName("reason").HasMaxLength(4000).IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(r => r.ReviewedByManagerId).HasColumnName("reviewed_by_manager_id");
        builder.Property(r => r.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(r => r.DecisionNote).HasColumnName("decision_note").HasMaxLength(4000);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(r => r.ComplaintId);
        builder.HasIndex(r => r.Status);
    }
}
