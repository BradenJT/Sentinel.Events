using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.DTOs.Requests;
using Sentinel.Application.Services.Interfaces;
using Sentinel.Domain.Enums;

namespace Sentinel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(IDeviceService deviceService, ILogger<DevicesController> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        /// <summary>
        /// Provision a new IoT device
        /// </summary>
        [HttpPost("provision")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProvisionDevice([FromBody] DeviceProvisionRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Provisioning device: {DeviceId}", request.DeviceId);

            var result = await _deviceService.ProvisionDeviceAsync(request, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetDeviceHealth), new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Get device health status
        /// </summary>
        [HttpGet("{id:guid}/health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDeviceHealth(Guid id, CancellationToken cancellationToken)
        {
            var result = await _deviceService.GetDeviceHealthAsync(id, cancellationToken);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all devices for the current tenant
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDevices(CancellationToken cancellationToken)
        {
            var result = await _deviceService.GetAllDevicesAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get devices by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDevicesByStatus(DeviceStatus status, CancellationToken cancellationToken)
        {
            var result = await _deviceService.GetDevicesByStatusAsync(status, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Update device heartbeat
        /// </summary>
        [HttpPost("{id:guid}/heartbeat")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateHeartbeat(Guid id, CancellationToken cancellationToken)
        {
            var result = await _deviceService.UpdateHeartbeatAsync(id, cancellationToken);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
