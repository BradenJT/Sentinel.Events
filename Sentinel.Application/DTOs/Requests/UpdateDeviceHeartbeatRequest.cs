namespace Sentinel.Application.DTOs.Requests
{
    public class UpdateDeviceHeartbeatRequest
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
    }
}
