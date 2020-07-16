using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Midwolf.Api.Infrastructure;
using Midwolf.GamesFramework.Api.Infrastructure;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;

namespace Midwolf.GamesFramework.Api.Controllers
{
    [Route("api/games/{gameId:int}/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [TypeFilter(typeof(ValidateRouteEntitiesFilter))]
    [ApiController]
    public class ModerationController : Controller
    {
        private readonly IModerateService _moderateService;
        private readonly IMapper _mapperService;
        
        public ModerationController(IModerateService moderateService, IMapper mapperService)
        {
            _moderateService = moderateService;
            _mapperService = mapperService;
        }

        [Authorize(Policy = "Moderate")]
        [HttpGet("{moderationEventId:int}")]
        public async Task<IActionResult> GetEntriesForModerationIdAsync([FromRoute] int gameId, [FromRoute] int moderationEventId)
        {
            var entriesDto = await _moderateService.GetEntriesForModerationIdAsync(gameId, moderationEventId);

            return Ok(entriesDto);
        }

        [Authorize(Policy = "Moderate")]
        [HttpGet]
        public async Task<IActionResult> GetAllEntriesInModerationAsync([FromRoute] int gameId)
        {
            var entriesDto = await _moderateService.GetAllEntriesInModerationAsync(gameId);

            return Ok(entriesDto);
        }

        [Authorize(Policy = "Moderate")]
        [HttpPost("{moderationEventId:int}")]
        public async Task<IActionResult> ModerateAsync([FromRoute] int gameId, [FromRoute] int moderationEventId, ICollection<ModerateEntry> moderateDto)
        {
            var results = await _moderateService.ModerateAsync(gameId, moderationEventId, moderateDto);

            return Ok(results);
        }
    }
}
