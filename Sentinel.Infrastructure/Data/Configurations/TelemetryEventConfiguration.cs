using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure.Data.Configurations
{
    public class TelemetryEventConfiguration : IEntityTypeConfiguration<TelemetryEvent>
    {
        public void Configure(EntityTypeBuilder<TelemetryEvent> builder)
        {
            builder.ToTable("TelemetryEvents");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.EventType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Payload)
                .IsRequired()
                .HasColumnType("nvarchar(max)"); // SQL Server JSON

            builder.Property(t => t.ReceivedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.TenantId);
            builder.HasIndex(t => t.DeviceId);
            builder.HasIndex(t => t.ReceivedAt);
            builder.HasIndex(t => t.EventType);

            // Relationships
            builder.HasOne(t => t.Tenant)
                .WithMany()
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid multiple cascade paths - will cascade via Device

            builder.HasOne(t => t.Device)
                .WithMany(d => d.TelemetryEvents)
                .HasForeignKey(t => t.DeviceId)
                .OnDelete(DeleteBehavior.Cascade); // Keep cascade - deleting Device deletes its TelemetryEvents
        }
    }
}
