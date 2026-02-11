using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Interfaces
{
    public interface IAlertRepository
    {
        Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Alert>> GetRecentAlertsAsync(int count = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<Alert>> GetBySeverityAsync(AlertSeverity severity, CancellationToken cancellationToken = default);
        Task<IEnumerable<Alert>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default);
        Task<bool> HasRecentDuplicateAsync(Guid deviceId, AlertType type, int windowMinutes, CancellationToken cancellationToken = default);
        Task AddAsync(Alert alert, CancellationToken cancellationToken = default);
        void Update(Alert alert);
    }
}
