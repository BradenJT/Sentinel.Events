using Sentinel.Domain.Common;
using Sentinel.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentinel.Domain.Entities
{
    public class Device : BaseEntity
    {
        public Guid TenantId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DeviceStatus Status { get; set; }
        public DateTimeOffset LastSeenAt { get; set; }
        public string? Metadata { get; set; }

        public Tenant Tenant { get; set; } = null!;
        public ICollection<TelemetryEvent> TelemetryEvents { get; set; } = [];
        public ICollection<Alert> Alerts { get; set; } = [];

        public bool IsOffline(int thresholdMinutes = 5)
        {
            return DateTimeOffset.UtcNow - LastSeenAt > TimeSpan.FromMinutes(thresholdMinutes);
        }

        public void UpdateHeartbeat()
        {
            LastSeenAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
