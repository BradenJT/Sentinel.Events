using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Configuration;
using System.Text.Json;

namespace Sentinel.Infrastructure.BackgroundWorkers
{
    public class MqttTelemetryWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttTelemetryWorker> _logger;
        private readonly MqttSettings _mqttSettings;
        private IMqttClient? _mqttClient;

        public MqttTelemetryWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<MqttTelemetryWorker> logger,
            IOptions<MqttSettings> mqttSettings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _mqttSettings = mqttSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MQTT Telemetry Worker starting - connecting to {Broker}:{Port}",
                _mqttSettings.BrokerHost, _mqttSettings.BrokerPort);

            try
            {
                await ConnectToBrokerAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker. Running in simulation mode.");
                await RunSimulationModeAsync(stoppingToken);
                return;
            }

            // Keep the worker running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                if (_mqttClient?.IsConnected == false)
                {
                    _logger.LogWarning("MQTT client disconnected. Attempting to reconnect...");
                    try
                    {
                        await ConnectToBrokerAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to reconnect to MQTT broker");
                    }
                }
            }

            _logger.LogInformation("MQTT Telemetry Worker stopped");
        }

        private async Task ConnectToBrokerAsync(CancellationToken cancellationToken)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttSettings.BrokerHost, _mqttSettings.BrokerPort)
                .WithClientId(_mqttSettings.ClientId)
                .WithCleanSession()
                .Build();

            if (!string.IsNullOrEmpty(_mqttSettings.Username))
            {
                options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_mqttSettings.BrokerHost, _mqttSettings.BrokerPort)
                    .WithClientId(_mqttSettings.ClientId)
                    .WithCredentials(_mqttSettings.Username, _mqttSettings.Password)
                    .WithCleanSession()
                    .Build();
            }

            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;

            await _mqttClient.ConnectAsync(options, cancellationToken);

            _logger.LogInformation("✅ Connected to MQTT broker at {Broker}:{Port}",
                _mqttSettings.BrokerHost, _mqttSettings.BrokerPort);

            // Subscribe to telemetry topic
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(_mqttSettings.Topic))
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);

            _logger.LogInformation("📡 Subscribed to topic: {Topic}", _mqttSettings.Topic);
            _logger.LogInformation("💡 Send test messages to: sentinel/{{tenantId}}/device/{{deviceId}}/telemetry");
        }

        private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                var payload = System.Text.Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
                var topic = args.ApplicationMessage.Topic;

                _logger.LogInformation("📨 Received message on {Topic}: {Payload}", topic, payload);

                // Parse topic: sentinel/{tenantId}/device/{deviceId}/telemetry
                var parts = topic.Split('/');
                if (parts.Length != 5)
                {
                    _logger.LogWarning("Invalid topic format: {Topic}", topic);
                    return;
                }

                var tenantIdStr = parts[1];
                var deviceIdStr = parts[3];

                // Parse payload
                var telemetry = JsonSerializer.Deserialize<TelemetryMessage>(payload);
                if (telemetry == null)
                {
                    _logger.LogWarning("Failed to parse telemetry payload");
                    return;
                }

                await ProcessTelemetryAsync(tenantIdStr, deviceIdStr, telemetry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message");
            }
        }

        private async Task ProcessTelemetryAsync(string tenantIdStr, string deviceIdStr, TelemetryMessage telemetry)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

            // Set tenant context
            if (!Guid.TryParse(tenantIdStr, out var tenantId))
            {
                _logger.LogWarning("Invalid tenant ID: {TenantId}", tenantIdStr);
                return;
            }

            tenantContext.SetTenant(tenantId);

            // Find device by deviceId string
            var device = await unitOfWork.Devices.GetByDeviceIdAsync(deviceIdStr, CancellationToken.None);
            if (device == null)
            {
                _logger.LogWarning("Device not found: {DeviceId}", deviceIdStr);
                return;
            }

            // Update device heartbeat
            device.UpdateHeartbeat();
            unitOfWork.Devices.Update(device);

            // Store telemetry event
            var telemetryEvent = new TelemetryEvent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                DeviceId = device.Id,
                EventType = telemetry.Type ?? "telemetry",
                Payload = JsonSerializer.Serialize(telemetry.Data),
                ReceivedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await unitOfWork.TelemetryEvents.AddAsync(telemetryEvent, CancellationToken.None);

            // Check for alert conditions
            await CheckAlertConditionsAsync(device, telemetry, unitOfWork);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("✅ Processed telemetry for device {DeviceId} ({DeviceName})",
                device.DeviceId, device.Name);
        }

        private async Task CheckAlertConditionsAsync(Device device, TelemetryMessage telemetry, IUnitOfWork unitOfWork)
        {
            try
            {
                _logger.LogWarning("🔍 Checking alert conditions for device {DeviceId}", device.DeviceId);

                // Extract temperature from the data object
                if (telemetry.Data == null)
                {
                    _logger.LogWarning("⚠️ No data in telemetry message");
                    return;
                }

                _logger.LogWarning("📊 Data has {Count} keys", telemetry.Data.Count);

                double temperature = 0;
                bool hasTemperature = false;

                // Try to get temperature value - handle both JsonElement and direct values
                if (telemetry.Data.TryGetValue("temperature", out var tempValue))
                {
                    if (tempValue is JsonElement jsonElement)
                    {
                        hasTemperature = jsonElement.TryGetDouble(out temperature);
                    }
                    else if (tempValue is double d)
                    {
                        temperature = d;
                        hasTemperature = true;
                    }
                    else if (tempValue is int i)
                    {
                        temperature = i;
                        hasTemperature = true;
                    }
                    else if (tempValue != null && double.TryParse(tempValue.ToString(), out var parsed))
                    {
                        temperature = parsed;
                        hasTemperature = true;
                    }
                }

                if (!hasTemperature)
                {
                    _logger.LogDebug("No temperature value found in telemetry data");
                    return;
                }

                _logger.LogInformation("Temperature reading: {Temperature}°F from device {DeviceId}",
                    temperature, device.DeviceId);

                // Check if temperature exceeds threshold
                if (temperature > 80) // Example threshold
                {
                    // Check for duplicate
                    var hasDuplicate = await unitOfWork.Alerts.HasRecentDuplicateAsync(
                        device.Id,
                        AlertType.TemperatureThreshold,
                        5,
                        CancellationToken.None);

                    if (!hasDuplicate)
                    {
                        var alert = new Alert
                        {
                            Id = Guid.NewGuid(),
                            TenantId = device.TenantId,
                            DeviceId = device.Id,
                            Type = AlertType.TemperatureThreshold,
                            Severity = temperature > 90 ? AlertSeverity.Critical : AlertSeverity.High,
                            Message = $"Temperature {temperature}°F exceeds threshold on {device.Name}",
                            IsPublic = true,
                            Metadata = JsonSerializer.Serialize(new { temperature, threshold = 80 }),
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        };

                        await unitOfWork.Alerts.AddAsync(alert, CancellationToken.None);
                        _logger.LogWarning("🚨 Generated temperature alert for device {DeviceId}: {Temp}°F",
                            device.DeviceId, temperature);
                    }
                    else
                    {
                        _logger.LogInformation("Skipped duplicate alert for device {DeviceId}", device.DeviceId);
                    }
                }
                else
                {
                    _logger.LogDebug("Temperature {Temp}°F is below threshold", temperature);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking alert conditions for device {DeviceId}", device.DeviceId);
            }
        }

        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            _logger.LogWarning("❌ Disconnected from MQTT broker: {Reason}", args.Reason);
            return Task.CompletedTask;
        }

        private async Task RunSimulationModeAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Running in SIMULATION MODE - generating random telemetry every 10 seconds");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSimulatedTelemetryAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in simulation mode");
                }
            }
        }

        private async Task ProcessSimulatedTelemetryAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var devices = await unitOfWork.Devices.GetAllAsync(cancellationToken);

            foreach (var device in devices.Take(3))
            {
                device.UpdateHeartbeat();
                unitOfWork.Devices.Update(device);

                var random = new Random();
                if (random.Next(0, 10) > 8)
                {
                    var hasDuplicate = await unitOfWork.Alerts.HasRecentDuplicateAsync(
                        device.Id,
                        AlertType.TemperatureThreshold,
                        5,
                        cancellationToken);

                    if (!hasDuplicate)
                    {
                        var alert = new Alert
                        {
                            Id = Guid.NewGuid(),
                            TenantId = device.TenantId,
                            DeviceId = device.Id,
                            Type = AlertType.TemperatureThreshold,
                            Severity = AlertSeverity.High,
                            Message = $"Simulated alert for {device.Name}",
                            IsPublic = true,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        };

                        await unitOfWork.Alerts.AddAsync(alert, cancellationToken);
                        _logger.LogWarning("Generated simulated alert for device {DeviceId}", device.DeviceId);
                    }
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed simulation batch at {Time}", DateTimeOffset.UtcNow);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_mqttClient != null)
            {
                await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
                _mqttClient.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }

    public class TelemetryMessage
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string? Type { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public Dictionary<string, object>? Data { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}
