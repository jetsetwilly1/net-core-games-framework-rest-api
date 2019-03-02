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
    public class FlowController : Controller
    {
        private readonly IFlowService _flowService;
        private readonly IMapper _mapperService;

        public FlowController(IFlowService flowService, IMapper mapperService)
        {
            _flowService = flowService;
            _mapperService = mapperService;
        }

        [Authorize(Policy = "Minimal Restriction")]
        [HttpGet]
        public async Task<IActionResult> GetFlowAsync([FromRoute] int gameId)
        {
            var flowDto = await _flowService.GetFlowAsync(gameId);

            return Ok(flowDto);
        }

        [Authorize(Policy = "Administrators")]
        [HttpPost]
        public async Task<IActionResult> AddFlowAsync([FromRoute] int gameId, [ValidateCollection] ICollection<Flow> flowDto)
        {
            // add flow to game 
            var flowAdded = await _flowService.AddFlowAsync(gameId, flowDto);

            if (_flowService.HasErrors) // return a bad request with errors.
                return new BadRequestObjectResult(new ApiError(_flowService.Errors));
            
            return Ok(flowAdded);
        }

        [Authorize(Policy = "Administrators")]
        [HttpDelete]
        public async Task<IActionResult> DeleteFlowAsync([FromRoute] int gameId)
        {
            var result = await _flowService.DeleteFlowAsync(gameId);

            if (result)
                return NoContent();
            else
                return BadRequest();
        }
    }
}
