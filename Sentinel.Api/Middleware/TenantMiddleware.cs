using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Infrastructure.Data;

namespace Sentinel.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-Key";

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        SentinelDbContext dbContext,
        ITenantContext tenantContext)
    {
        // Extract API key from header
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyValues) ||
            string.IsNullOrWhiteSpace(apiKeyValues))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "API key is required. Please provide X-API-Key header."
            });
            return;
        }

        var apiKey = apiKeyValues.ToString();

        // Look up tenant by API key
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ApiKey == apiKey && t.IsActive);

        if (tenant == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Invalid or inactive API key"
            });
            return;
        }

        // Set tenant context for this request
        tenantContext.SetTenant(tenant.Id);

        await _next(context);
    }
}
