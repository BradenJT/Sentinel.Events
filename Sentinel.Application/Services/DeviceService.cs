using Sentinel.Application.DTOs.Requests;
using Sentinel.Application.DTOs.Responses;
using Sentinel.Application.Interfaces;
using Sentinel.Application.Services.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public DeviceService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<ApiResponse<DeviceResponse>> ProvisionDeviceAsync(
            DeviceProvisionRequest request,
            CancellationToken cancellationToken = default)
        {
            // Check if device already exists
            var existingDevice = await _unitOfWork.Devices.GetByDeviceIdAsync(request.DeviceId, cancellationToken);
            if (existingDevice != null)
            {
                return ApiResponse<DeviceResponse>.FailureResult("Device with this ID already exists");
            }

            var device = new Device
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId,
                DeviceId = request.DeviceId,
                Name = request.Name,
                Status = DeviceStatus.Active,
                LastSeenAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                Metadata = request.Metadata
            };

            await _unitOfWork.Devices.AddAsync(device, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeviceResponse
            {
                Id = device.Id,
                DeviceId = device.DeviceId,
                Name = device.Name,
                Status = device.Status,
                LastSeenAt = device.LastSeenAt,
                CreatedAt = device.CreatedAt
            };

            return ApiResponse<DeviceResponse>.SuccessResult(response, "Device provisioned successfully");
        }

        public async Task<ApiResponse<DeviceHealthResponse>> GetDeviceHealthAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(id, cancellationToken);
            if (device == null)
            {
                return ApiResponse<DeviceHealthResponse>.FailureResult("Device not found");
            }

            var minutesSinceLastSeen = (int)(DateTimeOffset.UtcNow - device.LastSeenAt).TotalMinutes;

            var response = new DeviceHealthResponse
            {
                Id = device.Id,
                DeviceId = device.DeviceId,
                Status = device.Status,
                LastSeenAt = device.LastSeenAt,
                IsOffline = device.IsOffline(),
                MinutesSinceLastSeen = minutesSinceLastSeen
            };

            return ApiResponse<DeviceHealthResponse>.SuccessResult(response);
        }

        public async Task<ApiResponse<IEnumerable<DeviceResponse>>> GetAllDevicesAsync(
            CancellationToken cancellationToken = default)
        {
            var devices = await _unitOfWork.Devices.GetAllAsync(cancellationToken);

            var response = devices.Select(d => new DeviceResponse
            {
                Id = d.Id,
                DeviceId = d.DeviceId,
                Name = d.Name,
                Status = d.Status,
                LastSeenAt = d.LastSeenAt,
                CreatedAt = d.CreatedAt
            });

            return ApiResponse<IEnumerable<DeviceResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<IEnumerable<DeviceResponse>>> GetDevicesByStatusAsync(
            DeviceStatus status,
            CancellationToken cancellationToken = default)
        {
            var devices = await _unitOfWork.Devices.GetByStatusAsync(status, cancellationToken);

            var response = devices.Select(d => new DeviceResponse
            {
                Id = d.Id,
                DeviceId = d.DeviceId,
                Name = d.Name,
                Status = d.Status,
                LastSeenAt = d.LastSeenAt,
                CreatedAt = d.CreatedAt
            });

            return ApiResponse<IEnumerable<DeviceResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<bool>> UpdateHeartbeatAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(id, cancellationToken);
            if (device == null)
            {
                return ApiResponse<bool>.FailureResult("Device not found");
            }

            device.UpdateHeartbeat();
            _unitOfWork.Devices.Update(device);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.SuccessResult(true, "Heartbeat updated");
        }
    }
}
