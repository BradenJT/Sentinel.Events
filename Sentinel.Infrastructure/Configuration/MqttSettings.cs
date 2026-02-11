namespace Sentinel.Infrastructure.Configuration
{
    public class MqttSettings
    {
        public string BrokerHost { get; set; } = "broker.hivemq.com";
        public int BrokerPort { get; set; } = 1883;
        public string ClientId { get; set; } = "SentinelEvents_Worker";
        public string Topic { get; set; } = "sentinel/+/device/+/telemetry";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
