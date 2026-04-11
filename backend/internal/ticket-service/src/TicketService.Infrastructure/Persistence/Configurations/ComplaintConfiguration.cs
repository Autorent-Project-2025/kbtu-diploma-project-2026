using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations;

public sealed class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("complaints");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(c => c.ChargeId).HasColumnName("charge_id");

        builder.Property(c => c.ReporterActorType).HasColumnName("reporter_actor_type").HasConversion<int>().IsRequired();
        builder.Property(c => c.TargetType).HasColumnName("target_type").HasConversion<int>().IsRequired();
        builder.Property(c => c.Category).HasColumnName("category").HasConversion<int>().IsRequired();
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(c => c.Priority).HasColumnName("priority").HasConversion<int>().IsRequired();

        builder.Property(c => c.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(c => c.Subject).HasColumnName("subject").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(4000).IsRequired();

        builder.Property(c => c.AssignedToManagerId).HasColumnName("assigned_to_manager_id");

        builder.Property(c => c.InfoRequestText).HasColumnName("info_request_text").HasMaxLength(4000);
        builder.Property(c => c.InfoRequestAt).HasColumnName("info_request_at");
        builder.Property(c => c.InfoRequestBy).HasColumnName("info_request_by");
        builder.Property(c => c.InfoResponseText).HasColumnName("info_response_text").HasMaxLength(4000);
        builder.Property(c => c.InfoResponseAt).HasColumnName("info_response_at");

        builder.Property(c => c.ManagerNote).HasColumnName("manager_note").HasMaxLength(4000);
        builder.Property(c => c.ManagerNoteAt).HasColumnName("manager_note_at");
        builder.Property(c => c.ManagerNoteBy).HasColumnName("manager_note_by");

        builder.Property(c => c.ResolutionType).HasColumnName("resolution_type").HasConversion<int?>();
        builder.Property(c => c.ResolutionNote).HasColumnName("resolution_note").HasMaxLength(4000);
        builder.Property(c => c.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(c => c.ResolvedBy).HasColumnName("resolved_by");

        builder.Property(c => c.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(4000);
        builder.Property(c => c.RejectedAt).HasColumnName("rejected_at");
        builder.Property(c => c.RejectedBy).HasColumnName("rejected_by");

        var snapshotConverter = new ValueConverter<BookingSnapshotData, string>(
            v => JsonSerializer.Serialize(v, SerializerOptions),
            v => JsonSerializer.Deserialize<BookingSnapshotData>(v, SerializerOptions) ?? new BookingSnapshotData());

        var snapshotComparer = new ValueComparer<BookingSnapshotData>(
            (l, r) => JsonSerializer.Serialize(l, SerializerOptions) == JsonSerializer.Serialize(r, SerializerOptions),
            v => JsonSerializer.Serialize(v, SerializerOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<BookingSnapshotData>(
                JsonSerializer.Serialize(v, SerializerOptions), SerializerOptions) ?? new BookingSnapshotData());

        var snapshotProperty = builder.Property(c => c.SnapshotData)
            .HasColumnName("snapshot_data")
            .HasColumnType("jsonb")
            .HasConversion(snapshotConverter)
            .IsRequired();

        snapshotProperty.Metadata.SetValueComparer(snapshotComparer);

        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasMany(c => c.Attachments)
            .WithOne()
            .HasForeignKey(a => a.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Attachments).AutoInclude();

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.BookingId);
        builder.HasIndex(c => c.CreatedByUserId);
        builder.HasIndex(c => c.CreatedAt);
    }
}
