using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Midwolf.GamesFramework.Api.Infrastructure;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Api.Controllers
{
    [Route("api/games/{gameId:int}/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [TypeFilter(typeof(ValidateRouteEntitiesFilter))]
    [ApiController]
    public class PlayersController : Controller
    {

        // TODO MORE WORK NEEDED AROUND PERMISSIONS FOR EDITING AND ADDING PLAYERS

        private readonly IPlayerService _playerService;
        private readonly IMapper _mapperService;

        public PlayersController(IPlayerService playerService, IMapper mapperService)
        {
            _playerService = playerService;
            _mapperService = mapperService;
        }

        [Authorize(Policy = "Administrators")]
        [HttpGet("{playerId:int}")]
        public async Task<IActionResult> GetPlayerAsync([FromRoute] int gameId, [FromRoute] int playerId)
        {
            var eventDto = await _playerService.GetPlayerByIdAsync(gameId, playerId);

            return Ok(eventDto);
        }

        [Authorize(Policy = "Minimal Restriction")]
        [HttpGet]
        public async Task<IActionResult> GetAllPlayersAsync([FromRoute] int gameId)
        {
            var eventDto = await _playerService.GetAllPlayersAsync(gameId);

            return Ok(eventDto);
        }

        [Authorize(Policy = "Administrators")]
        [HttpPost]
        public async Task<IActionResult> AddPlayerAsync([FromRoute] int gameId, Player playerDto)
        {
            // add event to game 
            var eventAdded = await _playerService.AddPlayerAsync(gameId, playerDto);

            if (_playerService.HasErrors) // return a bad request with errors.
                return new BadRequestObjectResult(new ApiError(_playerService.Errors));

            return Ok(eventAdded);
        }

        [HttpPatch("{playerId:int}")]
        public async Task<IActionResult> UpdatePlayerAsync([FromRoute] int gameId, JsonPatchDocument<Player> patch, [FromRoute] int playerId)
        {
            var playerDb = await _playerService.GetPlayerByIdAsync(gameId, playerId);
            var baseDto = _mapperService.Map<Player>(playerDb);

            // removes any email patches as its not allowed.
            var emailPatch = patch.Operations.Where(x => x.path.ToLower().Contains("email")).FirstOrDefault();
            patch.Operations.Remove(emailPatch);
            patch.ApplyTo(baseDto);

            if (!TryValidateModel(baseDto))
                return new BadRequestObjectResult(ModelState);
            else
            {
                var eventUpdated = await _playerService.UpdatePlayerAsync(baseDto);

                if (_playerService.HasErrors) // return a bad request with errors.
                    return new BadRequestObjectResult(new ApiError(_playerService.Errors));

                return Ok(eventUpdated);
            }
        }

        [HttpDelete("{playerId:int}")]
        public async Task<IActionResult> DeletePlayerAsync([FromRoute] int playerId)
        {
            var result = await _playerService.DeletePlayerAsync(playerId);

            if (result)
                return NoContent();
            else
                return BadRequest();

        }
    }
}
