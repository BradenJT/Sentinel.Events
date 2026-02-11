using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure.Data.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable("Devices");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.DeviceId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.Metadata)
                .HasColumnType("nvarchar(max)"); // SQL Server JSON

            // Indexes
            builder.HasIndex(d => new { d.TenantId, d.DeviceId })
                .IsUnique();

            builder.HasIndex(d => d.LastSeenAt);
            builder.HasIndex(d => d.Status);

            // Relationships
            builder.HasOne(d => d.Tenant)
                .WithMany(t => t.Devices)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
