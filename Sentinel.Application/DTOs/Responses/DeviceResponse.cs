using Sentinel.Domain.Enums;

namespace Sentinel.Application.DTOs.Responses
{
    public class DeviceResponse
    {
        public Guid Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DeviceStatus Status { get; set; }
        public DateTimeOffset LastSeenAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
