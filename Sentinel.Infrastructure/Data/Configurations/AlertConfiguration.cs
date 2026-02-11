using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure.Data.Configurations
{
    public class AlertConfiguration : IEntityTypeConfiguration<Alert>
    {
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            builder.ToTable("Alerts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(a => a.Metadata)
                .HasColumnType("nvarchar(max)");

            // Indexes for common queries
            builder.HasIndex(a => a.CreatedAt);
            builder.HasIndex(a => a.Severity);
            builder.HasIndex(a => new { a.TenantId, a.DeviceId, a.Type, a.CreatedAt });

            // Relationships
            builder.HasOne(a => a.Tenant)
                .WithMany(t => t.Alerts)
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid multiple cascade paths - will cascade via Device

            builder.HasOne(a => a.Device)
                .WithMany(d => d.Alerts)
                .HasForeignKey(a => a.DeviceId)
                .OnDelete(DeleteBehavior.Cascade); // Keep cascade - deleting Device deletes its Alerts
        }
    }
}
