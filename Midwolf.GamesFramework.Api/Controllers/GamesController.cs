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
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Midwolf.GamesFramework.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [TypeFilter(typeof(ValidateRouteEntitiesFilter))]
    [Produces("application/json")]
    [ProducesResponseType(typeof(int), 400)]
    [ProducesResponseType(typeof(int), 401)]
    public class GamesController : Controller
    {
        private readonly IGameService _gameService;
        private readonly IMapper _mapperService;


        public GamesController(IGameService gameService, IMapper mapperService)
        {
            _gameService = gameService;
            _mapperService = mapperService;
        }

        /// <summary>
        /// Get a specific game by Id.
        /// </summary>
        /// <remarks>
        /// To return a specific game just add the game id.
        /// </remarks>
        /// <param name="gameId"></param>
        /// <returns><see cref="Game"/>Game</returns>
        [Authorize(Policy = "Minimal Restriction")]
        [HttpGet("{gameId:int}")]
        [SwaggerResponse(200, "Game successfully returned.", Type = typeof(Game))]
        public async Task<IActionResult> GetGameAsync([FromRoute] int gameId)
        {
            var gameDto = await _gameService.GetGameAsync(gameId);

            return Ok(gameDto);
        }

        /// <summary>
        /// This returns all games you have created.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "User Only")]
        [HttpGet]
        [SwaggerResponse(200, "All Games successfully returned.", Type = typeof(Game))]
        public async Task<IActionResult> GetAllGamesAsync()
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            var gameDto = await _gameService.GetAllGamesAsync(userId);

            return Ok(gameDto);
        }

        /// <summary>
        /// Create a game container.
        /// </summary>
        /// <remarks>
        /// This is the first call you make when creating your promotion/game.
        /// The simplest game will just need a 'title'.
        /// 'metadata' is optional.
        /// </remarks>
        /// <param name="game">Build your game container object.</param>
        /// <returns>Game</returns>
        [Authorize(Policy = "User Only")]
        [HttpPost]
        [SwaggerResponse(200, "Game successfully created.", Type = typeof(Game))]
        [SwaggerOperation(Consumes = new string[]{"application/json"})]
        [SwaggerRequestExample(typeof(Game), typeof(GameRequestExample))]
        public async Task<IActionResult> AddGameAsync([SwaggerParameter("game", Required = true)] Game game)
        {
            game.UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.Name).Value);

            var gameDto = await _gameService.AddGameAsync(game);

            return Ok(gameDto);
        }

        /// <summary>
        /// Patch an existing game.
        /// </summary>
        /// <remarks>
        /// You can only update the title and metadata for a game.
        /// Please see the following guide for using JsonPatch [http://jsonpatch.com/](http://jsonpatch.com/)
        /// </remarks>
        /// <param name="gameId">The Id of the game you want to update.</param>
        /// <param name="patch">The patch json object which holds all of your updates.</param>
        /// <returns>Game</returns>
        /// <response code="400">Bad Request or Validation errors on model.</response>
        [Authorize(Policy = "Administrators")]
        [HttpPatch("{gameId:int}")]
        [SwaggerOperation(Consumes = new string[] { "application/json-patch+json" })]
        [SwaggerResponse(200, "Game successfully patched.", Type = typeof(Game))]
        public async Task<IActionResult> UpdateGameAsync([FromRoute] int gameId, 
            [SwaggerParameter("patch", Required = true)] JsonPatchDocument<Game> patch)
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
