using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces
{
    public interface ITelemetryEventRepository
    {
        Task AddAsync(TelemetryEvent telemetryEvent, CancellationToken cancellationToken = default);
        Task<IEnumerable<TelemetryEvent>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TelemetryEvent>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    }
}
