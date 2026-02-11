using Microsoft.EntityFrameworkCore;
using Sentinel.Infrastructure.Data;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Api;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SentinelDbContext>();

        // Check if we already have data
        if (await context.Tenants.AnyAsync())
        {
            Console.WriteLine("Database already seeded");
            return;
        }

        Console.WriteLine("Seeding database...");

        // Create test tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Organization",
            ApiKey = "test-api-key-12345",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        Console.WriteLine($"Created tenant: {tenant.Name} (ID: {tenant.Id})");
        Console.WriteLine($"API Key: {tenant.ApiKey}");

        // Create test devices
        var devices = new[]
        {
            new Device
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                DeviceId = "sensor-001",
                Name = "Temperature Sensor - Building A",
                Status = DeviceStatus.Active,
                Metadata = "{\"location\":\"Building A\",\"floor\":1,\"type\":\"temperature\"}",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastSeenAt = DateTimeOffset.UtcNow
            },
            new Device
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                DeviceId = "sensor-002",
                Name = "Temperature Sensor - Building B",
                Status = DeviceStatus.Active,
                Metadata = "{\"location\":\"Building B\",\"floor\":2,\"type\":\"temperature\"}",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastSeenAt = DateTimeOffset.UtcNow
            },
            new Device
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                DeviceId = "sensor-003",
                Name = "Fire Detector - Data Center",
                Status = DeviceStatus.Active,
                Metadata = "{\"location\":\"Data Center\",\"zone\":\"critical\",\"type\":\"fire\"}",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastSeenAt = DateTimeOffset.UtcNow
            },
            new Device
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                DeviceId = "sensor-004",
                Name = "Motion Detector - Main Entrance",
                Status = DeviceStatus.Active,
                Metadata = "{\"location\":\"Main Entrance\",\"sensitivity\":\"high\",\"type\":\"motion\"}",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastSeenAt = DateTimeOffset.UtcNow
            }
        };

        context.Devices.AddRange(devices);
        await context.SaveChangesAsync();

        Console.WriteLine($"Created {devices.Length} test devices:");
        foreach (var device in devices)
        {
            Console.WriteLine($"  - {device.DeviceId}: {device.Name}");
        }

        Console.WriteLine("Database seeded successfully!");
    }
}
