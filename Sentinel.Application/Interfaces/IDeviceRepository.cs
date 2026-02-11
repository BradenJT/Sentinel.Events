using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Interfaces
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Device?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Device>> GetByStatusAsync(DeviceStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Device>> GetOfflineDevicesAsync(int thresholdMinutes, CancellationToken cancellationToken = default);
        Task AddAsync(Device device, CancellationToken cancellationToken = default);
        void Update(Device device);
        void Delete(Device device);
    }
}
