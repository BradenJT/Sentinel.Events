using Sentinel.Application.DTOs.Requests;
using Sentinel.Application.DTOs.Responses;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Services.Interfaces
{
    public interface IAlertService
    {
        Task<ApiResponse<AlertResponse>> CreateAlertAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<AlertResponse>>> GetRecentAlertsAsync(int count = 50, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<AlertResponse>>> GetAlertsBySeverityAsync(AlertSeverity severity, CancellationToken cancellationToken = default);
        Task<ApiResponse<AlertResponse>> AcknowledgeAlertAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
