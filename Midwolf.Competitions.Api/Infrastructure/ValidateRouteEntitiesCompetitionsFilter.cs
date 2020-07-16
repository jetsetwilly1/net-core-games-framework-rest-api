using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Midwolf.GamesFramework.CompetitionServices;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Midwolf.Competitions.Api.Infrastructure
{
    /// <summary>
    /// If another entity is included on the route then it will be checked here to see if it exists.
    /// This filter should be applied to routes which include a reference to a competition id.
    /// This is based on the RouteEntitiesFilter in the core framework.
    /// </summary>
    public class ValidateRouteEntitiesCompetitionsFilter : IAsyncActionFilter, IOrderedFilter
    {
        public int Order => int.MinValue;

        private readonly IGameService _gameService;
        private readonly IMapper _mapperService;

        public ValidateRouteEntitiesCompetitionsFilter(IGameService gameService, IMapper mapperService)
        {
            _gameService = gameService;
            _mapperService = mapperService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerName = context.RouteData.Values["controller"].ToString();

            var competitionId = Convert.ToInt32(context.RouteData.Values["competitionId"]);

            if (competitionId > 0)
            {
                // then we need to check the game exists.
                var competition = await _gameService.GetGameAsync(competitionId);

                if (competition == null)
                {
                    context.Result = new BadRequestObjectResult(new ApiError("Invalid Competition Id."));
                    return;
                }
                else
                {
                    // ensure user has access to the game.
                    var userId = Convert.ToInt32(context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value);

                    if (competition.UserId != userId)
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

                    var entityExists = await ((IEventService)controllerService).EventExists(competitionId, paramId);

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

                    var entityExists = await ((IPlayerService)controllerService).PlayerExists(competitionId, paramId);

                    if (!entityExists)
                    {
                        context.Result = new BadRequestObjectResult(new ApiError("Invalid Player Id."));
                        return;
                    }
                }
            }

            // check entries route, check the entry expiry date.
            if (controllerName == "Entries")
            {
                var controllerDefaultService = context.HttpContext.RequestServices.GetService(typeof(IEntryService));
                var controllerCompetitionEventsService = context.HttpContext.RequestServices.GetService(typeof(ICompetitionEntryService));
                if (context.RouteData.Values.Keys.Contains("entryId"))
                {
                    var paramId = Convert.ToInt32(context.RouteData.Values["entryId"]);

                    var entityExists = await ((IEntryService)controllerDefaultService).EntryExists(competitionId, paramId);

                    if (!entityExists)
                    {
                        context.Result = new BadRequestObjectResult(new ApiError("Invalid Entry Id."));
                        return;
                    }

                    var hasEntryExpired = await ((ICompetitionEntryService)controllerDefaultService).CheckEntryExpired(paramId);

                    if (hasEntryExpired)
                    {
                        var error = new List<Error>
                        {
                            new Error { Key = "entryexpired", Message = "Competition entry has expired." }
                        };

                        context.Result = new BadRequestObjectResult(new ApiError(error));
                        return;
                    }

                    // update this particular entry state..it might need to be moved.
                    await ((IEntryService)controllerDefaultService).ProcessEntryStateAsync(paramId);

                    // DO NOT AWAIT THIS CALL, ITS JUST GOING TO UPDATE THE REST OF THE ENTRIES SEPERATLY
                    BackgroundJob.Enqueue(() => ((IEntryService)controllerDefaultService).ProcessAllEntriesStateForGame(competitionId));
                }
                else
                {
                    // they must be getting all entries so run the updates on state and expiry status and await..
                    await ((IEntryService)controllerDefaultService).ProcessAllEntriesStateForGame(competitionId);
                    await ((ICompetitionEntryService)controllerCompetitionEventsService).UpdateAllEntriesExpiryStatus(competitionId);
                }
            }

            if (controllerName == "Chain")
            {
                // simple check to confirm this game has events
                var controllerService = context.HttpContext.RequestServices.GetService(typeof(IEventService));

                var events = await ((IEventService)controllerService).GetAllEventsAsync(competitionId);

                if (events == null || (events != null && events.Count == 0))
                {
                    context.Result = new BadRequestObjectResult(new ApiError("Create events before adding the Chain for this game."));
                    return;
                }
            }

            // everything ok so continue processing...
            await next();
        }
    }
}
