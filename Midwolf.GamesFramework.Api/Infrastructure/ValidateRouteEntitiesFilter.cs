using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Api.Infrastructure
{
    /// <summary>
    /// If another entity is included on the route then it will be checked here to see if it exists.
    /// This filter should be applied to routes which include a reference to a game id.
    /// </summary>
    public class ValidateRouteEntitiesFilter : IAsyncActionFilter, IOrderedFilter
    {
        public int Order => int.MinValue;

        private readonly IGameService _gameService;
        private readonly IMapper _mapperService;

        public ValidateRouteEntitiesFilter(IGameService gameService, IMapper mapperService)
        {
            _gameService = gameService;
            _mapperService = mapperService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerName = context.RouteData.Values["controller"].ToString();

            var gameId = Convert.ToInt32(context.RouteData.Values["gameId"]);

            if (gameId > 0)
            {
                // then we need to check the game exists.
                var game = await _gameService.GetGameAsync(gameId);

                if (game == null)
                {
                    context.Result = new BadRequestObjectResult(new ApiError("Invalid Game Id."));
                    return;
                }
                else
                {
                    // ensure user has access to the game.
                    var userId = Convert.ToInt32(context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value);

                    if (game.UserId != userId)
                    {
                        context.Result = new UnauthorizedObjectResult(new ApiError("Unauthorised"));
                        return;
                    }
                }
            }

            // check events route
            if (controllerName == "Events")
            {
                if (context.RouteData.Values.Keys.Contains("eventId"))
                {
                    var controllerService = context.HttpContext.RequestServices.GetService(typeof(IEventService));

                    var paramId = Convert.ToInt32(context.RouteData.Values["eventId"]);

                    var entityExists = await ((IEventService)controllerService).EventExists(gameId, paramId);

                    if (!entityExists)
                    {
                        context.Result = new BadRequestObjectResult(new ApiError("Invalid Event Id."));
                        return;
                    }
                }
            }

            // check players route
            if (controllerName == "Players")
            {
                if (context.RouteData.Values.Keys.Contains("playerId"))
                {
                    var controllerService = context.HttpContext.RequestServices.GetService(typeof(IPlayerService));

                    var paramId = Convert.ToInt32(context.RouteData.Values["playerId"]);

                    var entityExists = await ((IPlayerService)controllerService).PlayerExists(gameId, paramId);

                    if (!entityExists)
                    {
                        context.Result = new BadRequestObjectResult(new ApiError("Invalid Player Id."));
                        return;
                    }
                }
            }

            // check entries route
            if (controllerName == "Entries")
            {
                if (context.RouteData.Values.Keys.Contains("entryId"))
                {
                    var controllerService = context.HttpContext.RequestServices.GetService(typeof(IEntryService));

                    var paramId = Convert.ToInt32(context.RouteData.Values["entryId"]);

                    var entityExists = await ((IEntryService)controllerService).EntryExists(gameId, paramId);

                    if (!entityExists)
                    {
                        context.Result = new BadRequestObjectResult(new ApiError("Invalid Entry Id."));
                        return;
                    }
                }
            }

            if (controllerName == "Flow")
            {
                // simple check to confirm this game has events
                var controllerService = context.HttpContext.RequestServices.GetService(typeof(IEventService));
                
                var events = await ((IEventService)controllerService).GetAllEventsAsync(gameId);

                if (events == null || (events != null && events.Count == 0))
                {
                    context.Result = new BadRequestObjectResult(new ApiError("Create events before adding the flow for this game."));
                    return;
                }
            }

            // everything ok so continue processing...
            await next();
        }
    }
}
