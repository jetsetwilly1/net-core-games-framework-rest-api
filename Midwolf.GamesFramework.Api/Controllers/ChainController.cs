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
        public async Task<IActionResult> GetChainAsync([FromRoute] int gameId)
        {
            var ChainDto = await _ChainService.GetChainAsync(gameId);

            return Ok(ChainDto);
        }

        [Authorize(Policy = "Administrators")]
        [HttpPost]
        public async Task<IActionResult> AddChainAsync([FromRoute] int gameId, [ValidateCollection] ICollection<Chain> ChainDto)
        {
            // add Chain to game 
            var ChainAdded = await _ChainService.AddChainAsync(gameId, ChainDto);

            if (_ChainService.HasErrors) // return a bad request with errors.
                return new BadRequestObjectResult(new ApiError(_ChainService.Errors));
            
            return Ok(ChainAdded);
        }

        [Authorize(Policy = "Administrators")]
        [HttpDelete]
        public async Task<IActionResult> DeleteChainAsync([FromRoute] int gameId)
        {
            var result = await _ChainService.DeleteChainAsync(gameId);

            if (result)
                return NoContent();
            else
                return BadRequest();
        }
    }
}
