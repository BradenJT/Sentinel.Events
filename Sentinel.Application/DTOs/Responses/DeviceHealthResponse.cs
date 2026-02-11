using Sentinel.Domain.Enums;

namespace Sentinel.Application.DTOs.Responses
{
    public class DeviceHealthResponse
    {
        public Guid Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public DeviceStatus Status { get; set; }
        public DateTimeOffset LastSeenAt { get; set; }
        public bool IsOffline { get; set; }
        public int MinutesSinceLastSeen { get; set; }
    }
}
