using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Infrastructure.BackgroundWorkers
{
    public class DeviceHealthMonitorWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeviceHealthMonitorWorker> _logger;
        private const int OfflineThresholdMinutes = 5;

        public DeviceHealthMonitorWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<DeviceHealthMonitorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Device Health Monitor started at {Time}", DateTimeOffset.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckDeviceHealthAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking device health");
                }
            }

            _logger.LogInformation("Device Health Monitor stopped at {Time}", DateTimeOffset.UtcNow);
        }

        private async Task CheckDeviceHealthAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var offlineDevices = await unitOfWork.Devices.GetOfflineDevicesAsync(
                OfflineThresholdMinutes,
                cancellationToken);

            foreach (var device in offlineDevices)
            {
                // Generate offline alert
                var hasDuplicate = await unitOfWork.Alerts.HasRecentDuplicateAsync(
                    device.Id,
                    AlertType.DeviceOffline,
                    15, // 15-minute deduplication window
                    cancellationToken);

                if (!hasDuplicate)
                {
                    var alert = new Alert
                    {
                        Id = Guid.NewGuid(),
                        TenantId = device.TenantId,
                        DeviceId = device.Id,
                        Type = AlertType.DeviceOffline,
                        Severity = AlertSeverity.Medium,
                        Message = $"Device {device.Name} has been offline for over {OfflineThresholdMinutes} minutes",
                        IsPublic = false,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await unitOfWork.Alerts.AddAsync(alert, cancellationToken);
                    _logger.LogWarning("Device {DeviceId} marked as offline", device.DeviceId);
                }

                // Update device status
                device.Status = DeviceStatus.Offline;
                unitOfWork.Devices.Update(device);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Health check completed. {Count} offline devices found", offlineDevices.Count());
        }
    }
}
