using Chingoo.Services.FootballData;
using Microsoft.AspNetCore.Mvc;

namespace Chingoo.Controllers
{
    [Route("api/football-data")]
    [ApiController]
    public class FootballDataController : ControllerBase
    {
        private readonly IFootballDataService _footballDataService;

        public FootballDataController(IFootballDataService footballDataService)
        {
            _footballDataService = footballDataService;
        }

        [HttpGet("world-cup/matches")]
        public async Task<IActionResult> GetWorldCupMatches([FromQuery] int season = 2026, CancellationToken cancellationToken = default)
        {
            var matches = await _footballDataService.GetWorldCupMatchesAsync(season, cancellationToken);
            return Ok(matches);
        }
    }
}
