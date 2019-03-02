using System;
using System.Security.Claims;
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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [TypeFilter(typeof(ValidateRouteEntitiesFilter))]
    public class GamesController : Controller
    {
        private readonly IGameService _gameService;
        private readonly IMapper _mapperService;


        public GamesController(IGameService gameService, IMapper mapperService)
        {
            _gameService = gameService;
            _mapperService = mapperService;
        }

        [Authorize(Policy = "Minimal Restriction")]
        [HttpGet("{gameId:int}")]
        public async Task<IActionResult> GetGameAsync([FromRoute] int gameId)
        {
            var gameDto = await _gameService.GetGameAsync(gameId);

            return Ok(gameDto);
        }

        [Authorize(Policy = "User Only")]
        [HttpGet]
        public async Task<IActionResult> GetAllGamesAsync()
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            var gameDto = await _gameService.GetAllGamesAsync(userId);

            return Ok(gameDto);
        }

        [Authorize(Policy = "User Only")]
        [HttpPost]
        public async Task<IActionResult> AddGameAsync(Game game)
        {
            game.UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.Name).Value);

            var gameDto = await _gameService.AddGameAsync(game);

            return Ok(gameDto);
        }

        [Authorize(Policy = "Administrators")]
        [HttpPatch("{gameId:int}")]
        public async Task<IActionResult> UpdateEventAsync([FromRoute] int gameId, JsonPatchDocument<Game> patch)
        {
            var gameDb = await _gameService.GetGameAsync(gameId);
            var baseDto = _mapperService.Map<Game>(gameDb);

            patch.ApplyTo(baseDto); // apply json patch

            if (!TryValidateModel(baseDto))
                return new BadRequestObjectResult(ModelState);
            else
            {
                var eventUpdated = await _gameService.UpdateGameAsync(baseDto);

                return Ok(eventUpdated);
            }
        }

    }
}
