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
    [Route("api/competitions/{competitionId:int}/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [TypeFilter(typeof(ValidateRouteEntitiesCompetitionsFilter))]
    [ApiController]
    public class EntriesController : Controller
    {
        private readonly ICompetitionEntryService _entryService;
        private readonly IMapper _mapperService;

        public EntriesController(ICompetitionEntryService entryService, IMapper mapperService)
        {
            _entryService = entryService;
            _mapperService = mapperService;
        }

        [HttpGet("{entryId:int}")]
        public async Task<IActionResult> GetEntryAsync([FromRoute] int competitionId, [FromRoute] int entryId)
        {
            var entryDto = await _entryService.GetEntryAsync(competitionId, entryId);

            return Ok(entryDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEntriesAsync([FromRoute] int competitionId)
        {
            var entryDto = await _entryService.GetAllEntriesAsync(competitionId);

            return Ok(entryDto);
        }

        /// <summary>
        /// Add an entry to your competition.
        /// </summary>
        /// <remarks>
        /// Initially the entry should have its status set as 'reserved', include the tickets this entry is for and the player Id the entry belongs to.
        /// 
        /// The expiry time will be set on successfull post of the entry according to the expiry in seconds set in the competition container.
        /// </remarks>
        /// <param name="competitionId"></param>
        /// <param name="entryDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse(200, "Entry successfully created.", Type = typeof(CompetitionEntry))]
        [SwaggerOperation(Consumes = new string[] { "application/json" })]
        [SwaggerRequestExample(typeof(CompetitionEntry), typeof(EntryRequestExample), null, typeof(StringEnumConverter))]
        public async Task<IActionResult> AddEntryAsync([FromRoute] int competitionId, CompetitionEntry entryDto)
        {
            // add event to game 
            var entryAdded = await _entryService.AddEntryAsync(competitionId, entryDto);

            if (_entryService.HasErrors)
                return new BadRequestObjectResult(new ApiError(_entryService.Errors));

            return Ok(entryAdded);
        }

        [HttpPatch("{entryId:int}")]
        public async Task<IActionResult> UpdateEntryAsync([FromRoute] int competitionId, JsonPatchDocument<Entry> patch, [FromRoute] int entryId)
        {
            var entryDb = await _entryService.GetEntryAsync(competitionId, entryId);
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
    }
}
