using Sentinel.Application.Interfaces;

namespace Sentinel.Infrastructure.MultiTenancy
{
    public class TenantContext : ITenantContext
    {
        private Guid _tenantId;

        public Guid TenantId => _tenantId;

        public void SetTenant(Guid tenantId)
        {
            _tenantId = tenantId;
        }
    }
}
