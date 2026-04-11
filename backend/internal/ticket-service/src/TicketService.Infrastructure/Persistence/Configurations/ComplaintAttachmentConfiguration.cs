using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations;

public sealed class ComplaintAttachmentConfiguration : IEntityTypeConfiguration<ComplaintAttachment>
{
    public void Configure(EntityTypeBuilder<ComplaintAttachment> builder)
    {
        builder.ToTable("complaint_attachments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.ComplaintId).HasColumnName("complaint_id").IsRequired();
        builder.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.FileType).HasColumnName("file_type").HasMaxLength(100).IsRequired();
        builder.Property(a => a.UploadedByUserId).HasColumnName("uploaded_by_user_id").IsRequired();
        builder.Property(a => a.AttachmentPhase).HasColumnName("attachment_phase").HasConversion<int>().IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(a => a.ComplaintId);
    }
}
