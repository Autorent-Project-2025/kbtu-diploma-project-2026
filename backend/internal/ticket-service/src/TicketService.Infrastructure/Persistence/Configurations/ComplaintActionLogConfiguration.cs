using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations;

public sealed class ComplaintActionLogConfiguration : IEntityTypeConfiguration<ComplaintActionLog>
{
    public void Configure(EntityTypeBuilder<ComplaintActionLog> builder)
    {
        builder.ToTable("complaint_action_logs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ComplaintId).HasColumnName("complaint_id").IsRequired();
        builder.Property(e => e.ActionType).HasColumnName("action_type").HasMaxLength(100).IsRequired();
        builder.Property(e => e.PerformedBy).HasColumnName("performed_by").IsRequired();
        builder.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(4000);
        builder.Property(e => e.TargetEntityType).HasColumnName("target_entity_type").HasMaxLength(100);
        builder.Property(e => e.TargetEntityId).HasColumnName("target_entity_id").HasMaxLength(200);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(e => e.ComplaintId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
