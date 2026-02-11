using System.ComponentModel.DataAnnotations;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.DTOs.Requests
{
    public class CreateAlertRequest
    {
        [Required]
        public Guid DeviceId { get; set; }

        [Required]
        public AlertType Type { get; set; }

        [Required]
        public AlertSeverity Severity { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public bool IsPublic { get; set; } = false;

        public string? Metadata { get; set; }
    }
}
