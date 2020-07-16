using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Midwolf.Competitions.Api.Infrastructure;
using Midwolf.GamesFramework.CompetitionServices;
using Midwolf.GamesFramework.CompetitionServices.Models;
using Midwolf.GamesFramework.Services.Models;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Midwolf.Competitions.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [TypeFilter(typeof(ValidateRouteEntitiesCompetitionsFilter))]
    [Produces("application/json")]
    [ProducesResponseType(typeof(int), 400)]
    [ProducesResponseType(typeof(int), 401)]
    public class CompetitionsController : Controller
    {
        private readonly ICompetitionGameService _gameService;
        private readonly IMapper _mapperService;

        public CompetitionsController(ICompetitionGameService gameService, IMapper mapperService)
        {
            _gameService = gameService;
            _mapperService = mapperService;
        }

        /// <summary>
        /// Get a specific competition by Id.
        /// </summary>
        /// <remarks>
        /// To return a specific competition just add the competition id.
        /// </remarks>
        /// <param name="competitionId"></param>
        /// <returns>Competition</returns>
        [Authorize(Policy = "Minimal Restriction")]
        [HttpGet("{competitionId:int}")]
        [SwaggerResponse(200, "Game successfully returned.", Type = typeof(Competition))]
        public async Task<IActionResult> GetCompetitionAsync([FromRoute] int competitionId)
        {
            var competitionDto = await _gameService.GetGameAsync(competitionId);

            return Ok(competitionDto);
        }

        /// <summary>
        /// This returns all competitions you have created.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "User Only")]
        [HttpGet]
        [SwaggerResponse(200, "All Competitions successfully returned.", Type = typeof(Competition))]
        public async Task<IActionResult> GetAllGamesAsync()
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            var competitionDto = await _gameService.GetAllGamesAsync(userId);

            return Ok(competitionDto);
        }

        /// <summary>
        /// Create a competition container.
        /// </summary>
        /// <remarks>
        /// Create your competition ensuring the metadata for the competition is completed.
        /// 
        /// The simplest competition will just need a 'title'.
        /// 
        /// 'metadata' will store information about the competition.
        /// 
        /// When you post your competition successfully then you will be returned a competition with all the current ticket and player statistics.
        /// </remarks>
        /// <param name="competition">Build your competition container object.</param>
        /// <returns>Competition</returns>
        [Authorize(Policy = "User Only")]
        [HttpPost]
        [SwaggerResponse(200, "Competition successfully created.", Type = typeof(Competition))]
        [SwaggerOperation(Consumes = new string[] { "application/json" })]
        [SwaggerRequestExample(typeof(Competition), typeof(CompetitionRequestExample), null, typeof(StringEnumConverter))]
        public async Task<IActionResult> AddCompetitionAsync([SwaggerParameter("competition", Required = true)] Competition competition)
        {
            competition.UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.Name).Value);

            var gameDto = await _gameService.AddGameAsync(competition);

            return Ok(gameDto);
        }

        /// <summary>
        /// Patch an existing competition.
        /// </summary>
        /// <remarks>
        /// You can only update the title and competition metadata.
        /// Please see the following guide for using JsonPatch [http://jsonpatch.com/](http://jsonpatch.com/)
        /// </remarks>
        /// <param name="competitionId">The Id of the competition you want to update.</param>
        /// <param name="patch">The patch json object which holds all of your updates.</param>
        /// <returns>Competition</returns>
        /// <response code="400">Bad Request or Validation errors on model.</response>
        [Authorize(Policy = "Administrators")]
        [HttpPatch("{competitionId:int}")]
        [SwaggerOperation(Consumes = new string[] { "application/json-patch+json" })]
        [SwaggerResponse(200, "Competition successfully patched.", Type = typeof(Competition))]
        public async Task<IActionResult> UpdateGameAsync([FromRoute] int competitionId,
            [SwaggerParameter("patch", Required = true)] JsonPatchDocument<Game> patch)
        {
            var gameDb = await _gameService.GetGameAsync(competitionId);
            var baseDto = _mapperService.Map<Game>(gameDb);

            patch.ApplyTo(baseDto); // apply json patch

            if (!TryValidateModel(baseDto))
                return new BadRequestObjectResult(ModelState);
            else
            {
                var competitionUpdated = await _gameService.UpdateGameAsync(baseDto);

                return Ok(competitionUpdated);
            }
        }
    }
}
