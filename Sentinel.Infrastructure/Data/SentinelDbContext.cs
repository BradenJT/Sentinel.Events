using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Data.Configurations;

namespace Sentinel.Infrastructure.Data
{
    public class SentinelDbContext : DbContext
    {
        private readonly ITenantContext _tenantContext;

        public SentinelDbContext(DbContextOptions<SentinelDbContext> options, ITenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<TelemetryEvent> TelemetryEvents { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfiguration(new TenantConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceConfiguration());
            modelBuilder.ApplyConfiguration(new AlertConfiguration());
            modelBuilder.ApplyConfiguration(new TelemetryEventConfiguration());
            modelBuilder.ApplyConfiguration(new CommunityPostConfiguration());

            // CRITICAL: Multi-tenant global query filters
            modelBuilder.Entity<Device>().HasQueryFilter(d => d.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<Alert>().HasQueryFilter(a => a.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<TelemetryEvent>().HasQueryFilter(t => t.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<CommunityPost>().HasQueryFilter(c => c.TenantId == _tenantContext.TenantId);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-set TenantId on new entities
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity.GetType().GetProperty("TenantId") != null);

            foreach (var entry in entries)
            {
                entry.Property("TenantId").CurrentValue = _tenantContext.TenantId;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
