#!/usr/bin/env dotnet-script
#r "nuget: MQTTnet, 4.3.7.1207"

using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;

// Configuration
var BROKER = "broker.hivemq.com";
var PORT = 1883;
var TENANT_ID = "3aa6163f-f889-48e9-a6fe-6d2ea42d57b4";  // From SeedData
var DEVICE_ID = "sensor-001";  // Your provisioned device
var TOPIC = $"sentinel/{TENANT_ID}/device/{DEVICE_ID}/telemetry";

Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("🚀 MQTT Test Publisher for Sentinel.Events (.NET)");
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine($"Broker: {BROKER}:{PORT}");
Console.WriteLine($"Topic: {TOPIC}");
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine();

// Create MQTT client
var factory = new MqttClientFactory();
var mqttClient = factory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithTcpServer(BROKER, PORT)
    .WithClientId("SentinelEvents_TestPublisher_DotNet")
    .WithCleanSession()
    .Build();

try
{
    await mqttClient.ConnectAsync(options);
    Console.WriteLine("✅ Connected to MQTT broker");
    Console.WriteLine("🔄 Sending telemetry messages (Ctrl+C to stop)...");
    Console.WriteLine();

    var messageCount = 0;
    var random = new Random();

    while (true)
    {
        var temperature = Math.Round(65 + random.NextDouble() * 30, 2);  // 65-95°F

        var telemetry = new
        {
            type = "temperature",
            data = new
            {
                temperature,
                humidity = Math.Round(30 + random.NextDouble() * 40, 2),
                pressure = Math.Round(980 + random.NextDouble() * 40, 2),
                battery = Math.Round(70 + random.NextDouble() * 30, 2)
            },
            timestamp = DateTime.UtcNow
        };

        var payload = JsonSerializer.Serialize(telemetry);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(TOPIC)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await mqttClient.PublishAsync(message);
        messageCount++;

        var emoji = temperature > 80 ? "🚨" : "✓";
        Console.WriteLine($"{emoji} [{DateTime.Now:HH:mm:ss}] Message #{messageCount}");
        Console.WriteLine($"   Temperature: {temperature}°F");
        Console.WriteLine($"   Payload: {payload}");

        if (temperature > 80)
            Console.WriteLine("   ⚠️  This should trigger an alert!");

        Console.WriteLine();

        await Task.Delay(5000);  // Send every 5 seconds
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}
finally
{
    await mqttClient.DisconnectAsync();
}
