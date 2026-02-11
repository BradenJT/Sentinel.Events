using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Data;

namespace Sentinel.Infrastructure.Repositories
{
    public class TelemetryEventRepository : ITelemetryEventRepository
    {
        private readonly SentinelDbContext _context;

        public TelemetryEventRepository(SentinelDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TelemetryEvent telemetryEvent, CancellationToken cancellationToken = default)
        {
            await _context.TelemetryEvents.AddAsync(telemetryEvent, cancellationToken);
        }

        public async Task<IEnumerable<TelemetryEvent>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
        {
            return await _context.TelemetryEvents
                .Where(t => t.DeviceId == deviceId)
                .OrderByDescending(t => t.ReceivedAt)
                .Take(100)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TelemetryEvent>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _context.TelemetryEvents
                .OrderByDescending(t => t.ReceivedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
