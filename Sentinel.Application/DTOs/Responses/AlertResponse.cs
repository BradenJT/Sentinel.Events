using Sentinel.Domain.Enums;

namespace Sentinel.Application.DTOs.Responses
{
    public class AlertResponse
    {
        public Guid Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public AlertType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsAcknowledged { get; set; }
    }
}
