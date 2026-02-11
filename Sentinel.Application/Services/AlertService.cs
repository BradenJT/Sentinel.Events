using Sentinel.Application.DTOs.Requests;
using Sentinel.Application.DTOs.Responses;
using Sentinel.Application.Interfaces;
using Sentinel.Application.Services.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Services
{
    public class AlertService : IAlertService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public AlertService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<ApiResponse<AlertResponse>> CreateAlertAsync(
            CreateAlertRequest request,
            CancellationToken cancellationToken = default)
        {
            // Verify device exists and belongs to tenant
            var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId, cancellationToken);
            if (device == null)
            {
                return ApiResponse<AlertResponse>.FailureResult("Device not found");
            }

            // Check for recent duplicate alerts (5-minute window)
            var hasDuplicate = await _unitOfWork.Alerts.HasRecentDuplicateAsync(
                request.DeviceId,
                request.Type,
                5,
                cancellationToken);

            if (hasDuplicate)
            {
                return ApiResponse<AlertResponse>.FailureResult(
                    "Duplicate alert detected within 5-minute window");
            }

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId,
                DeviceId = request.DeviceId,
                Type = request.Type,
                Severity = request.Severity,
                Message = request.Message,
                IsPublic = request.IsPublic,
                Metadata = request.Metadata,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Alerts.AddAsync(alert, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AlertResponse
            {
                Id = alert.Id,
                DeviceId = device.DeviceId,
                DeviceName = device.Name,
                Type = alert.Type,
                Severity = alert.Severity,
                Message = alert.Message,
                CreatedAt = alert.CreatedAt,
                IsAcknowledged = alert.IsAcknowledged
            };

            return ApiResponse<AlertResponse>.SuccessResult(response, "Alert created successfully");
        }

        public async Task<ApiResponse<IEnumerable<AlertResponse>>> GetRecentAlertsAsync(
            int count = 50,
            CancellationToken cancellationToken = default)
        {
            var alerts = await _unitOfWork.Alerts.GetRecentAlertsAsync(count, cancellationToken);

            var response = alerts.Select(a => new AlertResponse
            {
                Id = a.Id,
                DeviceId = a.Device.DeviceId,
                DeviceName = a.Device.Name,
                Type = a.Type,
                Severity = a.Severity,
                Message = a.Message,
                CreatedAt = a.CreatedAt,
                IsAcknowledged = a.IsAcknowledged
            });

            return ApiResponse<IEnumerable<AlertResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<IEnumerable<AlertResponse>>> GetAlertsBySeverityAsync(
            AlertSeverity severity,
            CancellationToken cancellationToken = default)
        {
            var alerts = await _unitOfWork.Alerts.GetBySeverityAsync(severity, cancellationToken);

            var response = alerts.Select(a => new AlertResponse
            {
                Id = a.Id,
                DeviceId = a.Device.DeviceId,
                DeviceName = a.Device.Name,
                Type = a.Type,
                Severity = a.Severity,
                Message = a.Message,
                CreatedAt = a.CreatedAt,
                IsAcknowledged = a.IsAcknowledged
            });

            return ApiResponse<IEnumerable<AlertResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<AlertResponse>> AcknowledgeAlertAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var alert = await _unitOfWork.Alerts.GetByIdAsync(id, cancellationToken);
            if (alert == null)
            {
                return ApiResponse<AlertResponse>.FailureResult("Alert not found");
            }

            alert.IsAcknowledged = true;
            alert.AcknowledgedAt = DateTimeOffset.UtcNow;

            _unitOfWork.Alerts.Update(alert);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AlertResponse
            {
                Id = alert.Id,
                DeviceId = alert.Device.DeviceId,
                DeviceName = alert.Device.Name,
                Type = alert.Type,
                Severity = alert.Severity,
                Message = alert.Message,
                CreatedAt = alert.CreatedAt,
                IsAcknowledged = alert.IsAcknowledged
            };

            return ApiResponse<AlertResponse>.SuccessResult(response, "Alert acknowledged");
        }
    }
}
