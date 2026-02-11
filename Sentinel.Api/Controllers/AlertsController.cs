using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.DTOs.Requests;
using Sentinel.Application.Services.Interfaces;
using Sentinel.Domain.Enums;

namespace Sentinel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(IAlertService alertService, ILogger<AlertsController> logger)
        {
            _alertService = alertService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new alert manually
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAlert([FromBody] CreateAlertRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating alert for device: {DeviceId}, Type: {Type}",
                request.DeviceId, request.Type);

            var result = await _alertService.CreateAlertAsync(request, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetRecentAlerts), new { count = 1 }, result);
        }

        /// <summary>
        /// Get recent alerts
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentAlerts([FromQuery] int count = 50, CancellationToken cancellationToken = default)
        {
            var result = await _alertService.GetRecentAlertsAsync(count, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get alerts by severity
        /// </summary>
        [HttpGet("severity/{severity}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAlertsBySeverity(AlertSeverity severity, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching alerts with severity: {Severity}", severity);

            var result = await _alertService.GetAlertsBySeverityAsync(severity, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Acknowledge an alert
        /// </summary>
        [HttpPost("{id:guid}/acknowledge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcknowledgeAlert(Guid id, CancellationToken cancellationToken)
        {
            var result = await _alertService.AcknowledgeAlertAsync(id, cancellationToken);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
