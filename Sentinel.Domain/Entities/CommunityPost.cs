using Sentinel.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentinel.Domain.Entities
{
    public class CommunityPost : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid? AlertId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ViewCount { get; set; }

        public Tenant Tenant { get; set; } = null!;
        public Alert? Alert { get; set; }
    }
}
