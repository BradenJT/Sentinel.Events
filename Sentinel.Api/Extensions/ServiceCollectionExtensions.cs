using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Application.Services;
using Sentinel.Application.Services.Interfaces;
using Sentinel.Infrastructure.BackgroundWorkers;
using Sentinel.Infrastructure.Configuration;
using Sentinel.Infrastructure.Data;
using Sentinel.Infrastructure.MultiTenancy;
using Sentinel.Infrastructure.Repositories;

namespace Sentinel.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSentinelServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<SentinelDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Multi-tenancy
            services.AddScoped<ITenantContext, TenantContext>();

            // Unit of Work & Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IAlertService, AlertService>();

            // Configuration
            services.Configure<MqttSettings>(configuration.GetSection("Mqtt"));

            // Background Workers
            services.AddHostedService<MqttTelemetryWorker>();
            services.AddHostedService<DeviceHealthMonitorWorker>();

            return services;
        }
    }
}
