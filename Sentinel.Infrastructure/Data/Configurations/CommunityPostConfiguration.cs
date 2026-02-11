using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure.Data.Configurations
{
    public class CommunityPostConfiguration : IEntityTypeConfiguration<CommunityPost>
    {
        public void Configure(EntityTypeBuilder<CommunityPost> builder)
        {
            builder.ToTable("CommunityPosts");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(c => c.Location)
                .HasMaxLength(200);

            builder.Property(c => c.ViewCount)
                .HasDefaultValue(0);

            // Indexes
            builder.HasIndex(c => c.TenantId);
            builder.HasIndex(c => c.CreatedAt);
            builder.HasIndex(c => c.AlertId);

            // Relationships
            builder.HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid multiple cascade paths

            builder.HasOne(c => c.Alert)
                .WithMany()
                .HasForeignKey(c => c.AlertId)
                .OnDelete(DeleteBehavior.SetNull); // Preserve posts when alert deleted
        }
    }
}
