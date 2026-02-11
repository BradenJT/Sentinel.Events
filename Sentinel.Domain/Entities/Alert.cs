using Sentinel.Domain.Common;
using Sentinel.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentinel.Domain.Entities
{
    public class Alert : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid DeviceId { get; set; }
        public AlertType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsPublic { get; set; } // For community feed
        public bool IsAcknowledged { get; set; }
        public DateTimeOffset? AcknowledgedAt { get; set; }
        public string? Metadata { get; set; }

        public Tenant Tenant { get; set; } = null!;
        public Device Device { get; set; } = null!;
    }
}
