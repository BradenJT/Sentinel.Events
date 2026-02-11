using System.ComponentModel.DataAnnotations;

namespace Sentinel.Application.DTOs.Requests
{
    public class DeviceProvisionRequest
    {
        [Required]
        [StringLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Metadata { get; set; }
    }
}
