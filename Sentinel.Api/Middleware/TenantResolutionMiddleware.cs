using Sentinel.Application.Interfaces;

namespace Sentinel.Api.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            // Extract tenant from header, claim, or subdomain
            var tenantId = ExtractTenantId(context);

            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("No tenant ID found in request");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { Error = "Tenant ID required" });
                return;
            }

            tenantContext.SetTenant(tenantId);
            _logger.LogInformation("Tenant {TenantId} resolved for request", tenantId);

            await _next(context);
        }

        private Guid ExtractTenantId(HttpContext context)
        {
            // Option 1: From header
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
            {
                if (Guid.TryParse(headerValue, out var tenantId))
                {
                    return tenantId;
                }
            }

            // Option 2: From claims (after authentication)
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var claimTenantId))
            {
                return claimTenantId;
            }

            // Option 3: Default tenant for development
            if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                return Guid.Parse("00000000-0000-0000-0000-000000000001"); // Dev tenant
            }

            return Guid.Empty;
        }
    }
}
