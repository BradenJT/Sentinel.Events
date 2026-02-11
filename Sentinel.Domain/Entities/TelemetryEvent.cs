using Sentinel.Domain.Common;

namespace Sentinel.Domain.Entities
{
    public class TelemetryEvent : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid DeviceId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty; // JSON
        public DateTimeOffset ReceivedAt { get; set; }

        public Tenant Tenant { get; set; } = null!;
        public Device Device { get; set; } = null!;
    }
}
