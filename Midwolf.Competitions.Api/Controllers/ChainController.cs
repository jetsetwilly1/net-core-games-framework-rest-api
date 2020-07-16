using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Midwolf.Api.Infrastructure;
using Midwolf.Competitions.Api.Infrastructure;
using Midwolf.GamesFramework.Api.Infrastructure;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;

namespace Midwolf.Competitions.Api.Controllers
{
    [Route("api/competitions/{competitionId:int}/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [TypeFilter(typeof(ValidateRouteEntitiesCompetitionsFilter))]
    [ApiController]
    public class ChainController : Controller
    {
        private readonly IChainService _ChainService;
        private readonly IMapper _mapperService;

        public ChainController(IChainService ChainService, IMapper mapperService)
        {
            _ChainService = ChainService;
            _mapperService = mapperService;
        }

        [Authorize(Policy = "Minimal Restriction")]
        [HttpGet]
        public async Task<IActionResult> GetChainAsync([FromRoute] int competitionId)
        {
            var ChainDto = await _ChainService.GetChainAsync(competitionId);

            return Ok(ChainDto);
        }

        [Authorize(Policy = "Administrators")]
        [HttpPost]
        public async Task<IActionResult> AddChainAsync([FromRoute] int competitionId, [ValidateCollection] ICollection<Chain> ChainDto)
        {
            // add Chain to game 
            var ChainAdded = await _ChainService.AddChainAsync(competitionId, ChainDto);

            if (_ChainService.HasErrors) // return a bad request with errors.
                return new BadRequestObjectResult(new ApiError(_ChainService.Errors));
            
            return Ok(ChainAdded);
        }

        [Authorize(Policy = "Administrators")]
        [HttpDelete]
        public async Task<IActionResult> DeleteChainAsync([FromRoute] int competitionId)
        {
            var result = await _ChainService.DeleteChainAsync(competitionId);

            if (result)
                return NoContent();
            else
                return BadRequest();
        }
    }
}
