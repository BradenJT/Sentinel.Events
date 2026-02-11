using Sentinel.Application.DTOs.Requests;
using Sentinel.Application.DTOs.Responses;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<ApiResponse<DeviceResponse>> ProvisionDeviceAsync(DeviceProvisionRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<DeviceHealthResponse>> GetDeviceHealthAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<DeviceResponse>>> GetAllDevicesAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<DeviceResponse>>> GetDevicesByStatusAsync(DeviceStatus status, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> UpdateHeartbeatAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
