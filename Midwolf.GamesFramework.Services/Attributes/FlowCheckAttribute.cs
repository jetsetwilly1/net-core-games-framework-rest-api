using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Midwolf.GamesFramework.Services.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Midwolf.GamesFramework.Services.Attributes
{
    public class FlowCheckAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;

            var eventService = (IEventService)validationContext.GetService(typeof(IEventService));
            var httpContextAccessor = (IHttpContextAccessor)validationContext.GetService(typeof(IHttpContextAccessor));

            var routingVars = httpContextAccessor.HttpContext.Features.Get<IRoutingFeature>();

            var gameId = Convert.ToInt32(routingVars.RouteData.Values["gameId"]);

            if (gameId <= 0)
                return new ValidationResult(ErrorMessage);

            var events = eventService.GetAllEventsAsync(gameId);

            // TODO check the flow submitted and ensure it is correct.
            // now check the event has not alreasdy been submitted
            foreach (var e in events.Result)
            {
                if (e.Type == (string)value)
                    return new ValidationResult(ErrorMessage);
            }
            
            return ValidationResult.Success;
        }
    }
}
