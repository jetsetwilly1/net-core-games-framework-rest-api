using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
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
    public class EntriesController : Controller
    {
        private readonly IEntryService _entryService;
        private readonly IMapper _mapperService;
        
        // TODO ADD PERMISSIONS ON THESE METHODS

        public EntriesController(IEntryService entryService, IMapper mapperService)
        {
            _entryService = entryService;
            _mapperService = mapperService;
        }

        [HttpGet("{entryId:int}")]
        public async Task<IActionResult> GetEntryAsync([FromRoute] int gameId, [FromRoute] int entryId)
        {
            var entryDto = await _entryService.GetEntryAsync(gameId, entryId);

            return Ok(entryDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEntriesAsync([FromRoute] int gameId)
        {
            var entryDto = await _entryService.GetAllEntriesAsync(gameId);

            return Ok(entryDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddEntryAsync([FromRoute] int gameId, Entry entryDto)
        {
            // add event to game 
            var entryAdded = await _entryService.AddEntryAsync(gameId, entryDto);

            if (_entryService.HasErrors)
                return new BadRequestObjectResult(new ApiError(_entryService.Errors));
            
            return Ok(entryAdded);
        }

        [HttpPatch("{entryId:int}")]
        public async Task<IActionResult> UpdateEntryAsync([FromRoute] int gameId, JsonPatchDocument<Entry> patch, [FromRoute] int entryId)
        {
            var entryDb = await _entryService.GetEntryAsync(gameId, entryId);
            var baseDto = _mapperService.Map<Entry>(entryDb);
            
            patch.ApplyTo(baseDto); // apply json patch

            var entryUpdated = await _entryService.UpdateEntryAsync(baseDto);

            if (!TryValidateModel(baseDto))
                return new BadRequestObjectResult(ModelState);
            else if (_entryService.HasErrors)
                return new BadRequestObjectResult(new ApiError(_entryService.Errors));
            else
            {
                return Ok(entryUpdated);
            }
        }

        [HttpDelete("{entryId:int}")]
        public async Task<IActionResult> DeleteEntryAsync([FromRoute] int entryId)
        {
            var result = await _entryService.DeleteEntryAsync(entryId);

            if (result)
                return NoContent();
            else
                return BadRequest();

        }
    }
}
