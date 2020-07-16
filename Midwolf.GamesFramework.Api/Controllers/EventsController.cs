using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Midwolf.Api.Infrastructure;
using Midwolf.GamesFramework.Api.Infrastructure;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Midwolf.GamesFramework.Api.Controllers
{
    [Route("api/games/{gameId:int}/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [TypeFilter(typeof(ValidateRouteEntitiesFilter))]
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(int), 400)]
    [ProducesResponseType(typeof(int), 401)]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IMapper _mapperService;
        
        public EventsController(IEventService eventService, IMapper mapperService)
        {
            _eventService = eventService;
            _mapperService = mapperService;
        }

        /// <summary>
        /// Get an event by id for a game.
        /// </summary>
        /// <remarks>
        /// Return an event object by id for a game.  For example if you wanted to check start and end dates.
        /// </remarks>
        /// <param name="gameId"></param>
        /// <param name="eventId"></param>
        /// <returns>An event</returns>
        [Authorize(Policy = "Minimal Restriction")]
        [SwaggerResponse(200, "Event successfully returned.", Type = typeof(Event))]
        [HttpGet("{eventId:int}")]
        public async Task<IActionResult> GetEventAsync([FromRoute] int gameId, [FromRoute] int eventId)
        {
            var eventDto = await _eventService.GetEventAsync(gameId, eventId);

            return Ok(eventDto);
        }

        /// <summary>
        /// Get all the events for a game.
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns>Collection of events</returns>
        [Authorize(Policy = "Minimal Restriction")]
        [SwaggerResponse(200, "All Events for this game successfully returned.", Type = typeof(ICollection<Event>))]
        [HttpGet]
        public async Task<IActionResult> GetAllEventsAsync([FromRoute] int gameId)
        {
            var eventDto = await _eventService.GetAllEventsAsync(gameId);

            return Ok(eventDto);
        }

        /// <summary>
        /// Add an event to a game.
        /// </summary>
        /// <remarks>
        /// Add an event to a game, please see the example json object for what is required as minimum to do this.
        /// Event types can be submission, moderate, randomdraw or custom.
        /// Please see the model explanations for Required rules for 'submission' and 'randomdraw' types.
        /// </remarks>
        /// <param name="gameId"></param>
        /// <param name="eventDto"></param>
        /// <returns></returns>
        [Authorize(Policy = "Administrators")]
        [HttpPost]
        [SwaggerResponse(200, "Event successfully created.", Type = typeof(Event))]
        [SwaggerOperation(Consumes = new string[] { "application/json" })]
        [SwaggerRequestExample(typeof(Event), typeof(EventRequestExample))]
        public async Task<IActionResult> AddEventAsync([FromRoute] int gameId, Event eventDto)
        {
            // add event to game 
            var eventAdded = await _eventService.AddEventAsync(gameId, eventDto);

            return Ok(eventAdded);
        }

        [Authorize(Policy = "Administrators")]
        [HttpPost("{eventId:int}/advance")]
        [SwaggerResponse(204, "Advance complete.")]
        public async Task<IActionResult> ManuallyAdvanceEventEntries([FromRoute] int gameId, [FromRoute] int eventId)
        {
            await _eventService.AdvanceEntriesForEvent(gameId, eventId);

            return Ok();
        }

        /// <summary>
        /// Patch an existing event. 
        /// </summary>
        /// <remarks>
        /// You cannot update an event type once created and when an event is added into a chain you are unable to update the start and end dates.
        /// Please see the following guide for using JsonPatch [http://jsonpatch.com/](http://jsonpatch.com/)
        /// </remarks>
        /// <param name="gameId">The Id of the game you want to update.</param>
        /// <param name="patch">The patch json object which holds all of your updates.</param>
        /// <param name="eventId">The event id you wish to patch.</param>
        /// <returns>Event</returns>
        [Authorize(Policy = "Administrators")]
        [HttpPatch("{eventId:int}")]
        [SwaggerOperation(Consumes = new string[] { "application/json-patch+json" })]
        [SwaggerResponse(200, "Event successfully patched.", Type = typeof(Event))]
        public async Task<IActionResult> UpdateEventAsync([FromRoute] int gameId, JsonPatchDocument<Event> patch, [FromRoute] int eventId)
        {
            var eventDb = await _eventService.GetEventAsync(gameId, eventId);
            var baseDto = _mapperService.Map<Event>(eventDb);
            
            patch.ApplyTo(baseDto); // apply json patch

            if (!TryValidateModel(baseDto))
                return new BadRequestObjectResult(ModelState);
            else if (_eventService.HasErrors)
                return new BadRequestObjectResult(new ApiError(_eventService.Errors));
            else
            {
                var eventUpdated = await _eventService.UpdateEventAsync(baseDto);

                return Ok(eventUpdated);
            }
        }

        /// <summary>
        /// Delete an event.
        /// </summary>
        /// <remarks>
        /// Delete an event from a game.
        /// WARNING: Deleting an event that is in the chain will also delete the games chain.  Any existing entries for this game will also be deleted.
        /// </remarks>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Administrators")]
        [HttpDelete("{eventId:int}")]
        [SwaggerResponse(204, "Event successfully deleted.", Type = typeof(Event))]
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
