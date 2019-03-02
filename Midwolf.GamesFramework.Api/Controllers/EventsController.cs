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
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IMapper _mapperService;
        
        public EventsController(IEventService eventService, IMapper mapperService)
        {
            _eventService = eventService;
            _mapperService = mapperService;
        }

        [Authorize(Policy = "Minimal Restriction")]
        [HttpGet("{eventId:int}")]
        public async Task<IActionResult> GetEventAsync([FromRoute] int gameId, [FromRoute] int eventId)
        {
            var eventDto = await _eventService.GetEventAsync(gameId, eventId);

            return Ok(eventDto);
        }

        [Authorize(Policy = "Minimal Restriction")]
        [HttpGet]
        public async Task<IActionResult> GetAllEventsAsync([FromRoute] int gameId)
        {
            var eventDto = await _eventService.GetAllEventsAsync(gameId);

            return Ok(eventDto);
        }

        [Authorize(Policy = "Administrators")]
        [HttpPost]
        public async Task<IActionResult> AddEventAsync([FromRoute] int gameId, Event eventDto)
        {
            // add event to game 
            var eventAdded = await _eventService.AddEventAsync(gameId, eventDto);

            return Ok(eventAdded);
        }

        [Authorize(Policy = "Administrators")]
        [HttpPatch("{eventId:int}")]
        public async Task<IActionResult> UpdateEventAsync([FromRoute] int gameId, JsonPatchDocument<Event> patch, [FromRoute] int eventId)
        {
            var eventDb = await _eventService.GetEventAsync(gameId, eventId);
            var baseDto = _mapperService.Map<Event>(eventDb);
            
            patch.ApplyTo(baseDto); // apply json patch

            if (!TryValidateModel(baseDto))
                return new BadRequestObjectResult(ModelState);
            else
            {
                var eventUpdated = await _eventService.UpdateEventAsync(baseDto);

                return Ok(eventUpdated);
            }
        }

        [Authorize(Policy = "Administrators")]
        [HttpDelete("{eventId:int}")]
        public async Task<IActionResult> DeleteEventAsync([FromRoute] int eventId)
        {
            var result = await _eventService.DeleteEventAsync(eventId);

            if (result)
                return NoContent();
            else
                return BadRequest();

        }

        //[HttpPatch("{eventId:int}")]
        //public async Task<IActionResult> UpdateEventAsync([FromRoute] int gameId, EventPatch eventDto, [FromRoute] int eventId)
        //{
        //    // update event
        //    var eventPatchedDto = await _defaultService.ApplyPatchAsync<EventEntity, EventPatch, Event>(eventId, eventDto);

        //    if (TryValidateModel(eventPatchedDto))
        //    {
        //        // if model is ok then update db.
        //        var eventAdded = await _eventService.UpdateEventAsync(eventPatchedDto, eventDto);
        //        return Ok(eventAdded);
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}
    }
}
