using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public sealed class ProcessedIntegrationEventConfiguration : IEntityTypeConfiguration<ProcessedIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedIntegrationEvent> builder)
    {
        builder.ToTable("processed_integration_events");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.EventId).HasColumnName("event_id").HasMaxLength(200).IsRequired();
        builder.Property(item => item.RoutingKey).HasColumnName("routing_key").HasMaxLength(200).IsRequired();
        builder.Property(item => item.ProcessedAt).HasColumnName("processed_at").HasColumnType("timestamp with time zone").IsRequired();

        builder.HasIndex(item => item.EventId).IsUnique();
    }
}
