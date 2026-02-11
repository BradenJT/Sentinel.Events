using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Infrastructure.Data;

namespace Sentinel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private readonly SentinelDbContext _context;
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(SentinelDbContext context, ILogger<CommunityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get community feed (public alerts)
        /// </summary>
        [HttpGet("feed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCommunityFeed(CancellationToken cancellationToken)
        {
            var feed = await _context.CommunityPosts
                .Include(p => p.Alert)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Content,
                    p.Location,
                    p.CreatedAt,
                    p.ViewCount,
                    Alert = p.Alert != null ? new
                    {
                        p.Alert.Type,
                        p.Alert.Severity,
                        p.Alert.Message
                    } : null
                })
                .ToListAsync(cancellationToken);

            return Ok(new { Success = true, Data = feed });
        }
    }
}
