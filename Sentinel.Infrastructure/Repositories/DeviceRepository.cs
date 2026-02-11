using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Data;

namespace Sentinel.Infrastructure.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly SentinelDbContext _context;

        public DeviceRepository(SentinelDbContext context)
        {
            _context = context;
        }

        public async Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<Device?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            return await _context.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken);
        }

        public async Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Devices
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Device>> GetByStatusAsync(DeviceStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Devices
                .AsNoTracking()
                .Where(d => d.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Device>> GetOfflineDevicesAsync(int thresholdMinutes, CancellationToken cancellationToken = default)
        {
            var threshold = DateTimeOffset.UtcNow.AddMinutes(-thresholdMinutes);
            return await _context.Devices
                .AsNoTracking()
                .Where(d => d.LastSeenAt < threshold && d.Status == DeviceStatus.Active)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Device device, CancellationToken cancellationToken = default)
        {
            await _context.Devices.AddAsync(device, cancellationToken);
        }

        public void Update(Device device)
        {
            _context.Devices.Update(device);
        }

        public void Delete(Device device)
        {
            _context.Devices.Remove(device);
        }
    }
}
