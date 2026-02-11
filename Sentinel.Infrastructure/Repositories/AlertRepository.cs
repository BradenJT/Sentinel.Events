using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Data;

namespace Sentinel.Infrastructure.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly SentinelDbContext _context;

        public AlertRepository(SentinelDbContext context)
        {
            _context = context;
        }

        public async Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Alerts
                .Include(a => a.Device) // Prevent N+1
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Alert>> GetRecentAlertsAsync(int count = 50, CancellationToken cancellationToken = default)
        {
            return await _context.Alerts
                .Include(a => a.Device) // Prevent N+1
                .AsNoTracking()
                .Where(a => !a.IsAcknowledged) // Only show unacknowledged alerts
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Alert>> GetBySeverityAsync(AlertSeverity severity, CancellationToken cancellationToken = default)
        {
            return await _context.Alerts
                .Include(a => a.Device)
                .AsNoTracking()
                .Where(a => a.Severity == severity && !a.IsAcknowledged) // Only show unacknowledged alerts
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Alert>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
        {
            return await _context.Alerts
                .AsNoTracking()
                .Where(a => a.DeviceId == deviceId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasRecentDuplicateAsync(
            Guid deviceId,
            AlertType type,
            int windowMinutes,
            CancellationToken cancellationToken = default)
        {
            var threshold = DateTimeOffset.UtcNow.AddMinutes(-windowMinutes);
            return await _context.Alerts
                .AnyAsync(a =>
                    a.DeviceId == deviceId &&
                    a.Type == type &&
                    a.CreatedAt > threshold,
                    cancellationToken);
        }

        public async Task AddAsync(Alert alert, CancellationToken cancellationToken = default)
        {
            await _context.Alerts.AddAsync(alert, cancellationToken);
        }

        public void Update(Alert alert)
        {
            _context.Alerts.Update(alert);
        }
    }
}
