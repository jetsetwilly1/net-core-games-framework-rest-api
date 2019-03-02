using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("{eventId:int}")]
        public async Task<IActionResult> GetEntriesForModerationIdAsync([FromRoute] int gameId, [FromRoute] int eventId)
        {
            var entriesDto = await _moderateService.GetEntriesForModerationIdAsync(gameId, eventId);

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
        [HttpPost]
        public async Task<IActionResult> ModerateAsync([FromRoute] int gameId, ICollection<ModerateEntry> moderateDto)
        {
            // add event to game 
            var entryAdded = await _moderateService.ModerateAsync(gameId, moderateDto);

            return Ok(entryAdded);
        }
    }
}
