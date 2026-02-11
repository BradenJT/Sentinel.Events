using Sentinel.Application.Interfaces;
using Sentinel.Infrastructure.Data;

namespace Sentinel.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SentinelDbContext _context;
        private IDeviceRepository? _devices;
        private IAlertRepository? _alerts;
        private ITelemetryEventRepository? _telemetryEvents;

        public UnitOfWork(SentinelDbContext context)
        {
            _context = context;
        }

        public IDeviceRepository Devices => _devices ??= new DeviceRepository(_context);
        public IAlertRepository Alerts => _alerts ??= new AlertRepository(_context);
        public ITelemetryEventRepository TelemetryEvents => _telemetryEvents ??= new TelemetryEventRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
