using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Microsoft.Extensions.Logging;
using Midwolf.GamesFramework.Services.Storage;
using Midwolf.GamesFramework.Services.Interfaces;

namespace Midwolf.GamesFramework.Services
{
    public class DefaultFlowService : ErrorService, IFlowService
    {
        private class CheckEventFlow
        {
            public List<int> CurrentEventReferences { get; set; }
            public EventEntity CurrentEvent { get; set; }
            public EventEntity PassEvent { get; set; }
            public EventEntity FailEvent { get; set; }
            public bool IsValid { get; set; }

            public bool CurrentEventIsStart { get; set; }
            public bool PassEventNotFound { get; set; }
            public bool FailEventNotFound { get; set; }
        }

        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IEventService _eventService;
        private int _gameId;

        public DefaultFlowService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper, IEventService eventService)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultEventService>();
            _eventService = eventService;
        }

        public async Task<ICollection<Flow>> AddFlowAsync(int gameId, ICollection<Flow> flowDto)
        {
            _gameId = gameId;
            var game = await _context.Games.FindAsync(_gameId);

            if ((game.Flow != null) || (game.Flow != null && game.Flow.Count > 0))
            {
                AddErrorToCollection(new Error { Key = "Flow", Message = "A Flow already exists for this game." });
                return null;
            }
            else
            {
                var isValid = await ValidateFlow(flowDto);
                
                if (isValid)
                {
                    // order the list by date
                    var allevents = await _eventService.GetAllEventsAsync(_gameId);

                    var ordered = allevents.OrderBy(x => x.EndDate);

                    var flowforsaving = new List<Flow>();

                    foreach (var o in ordered)
                    {
                        var f = flowDto.SingleOrDefault(x => x.Id == o.Id);
                        if (f != null)
                            flowforsaving.Add(f);
                    }

                    game.Flow = _mapper.Map<ICollection<FlowEntity>>(flowforsaving);

                    await _context.SaveChangesAsync();
                    return flowforsaving;
                }
            }

            return null;
        }
        
        private EventEntity GetEventEntity(int? id)
        {
            if (!id.HasValue)
                return null;

            var e = _eventService.GetEventAsync(_gameId, id.Value).Result;
            var m = _mapper.Map<EventEntity>(e);

            if(m != null)
                m.Id = id.Value; // set the value as its not mapped normally.

            return m;
        }

        private CheckEventFlow SetFlowProperties(ICollection<Flow> flowCollection, Flow flow)
        {
            var eventFlow = new CheckEventFlow()
            {
                CurrentEvent = GetEventEntity(flow.Id.Value),
                PassEvent = GetEventEntity(flow.SuccessEvent),
                FailEvent = GetEventEntity(flow.FailEvent)
            };

            eventFlow.PassEventNotFound = flow.SuccessEvent.HasValue && eventFlow.PassEvent == null;
            eventFlow.FailEventNotFound = flow.FailEvent.HasValue && eventFlow.FailEvent == null;

            eventFlow.CurrentEventIsStart = flow.IsStart;

            var references = flowCollection.Where(x => x.SuccessEvent == flow.Id.Value
            || x.FailEvent == flow.Id.Value).Select(x => x.Id.Value);

            eventFlow.CurrentEventReferences = new List<int>();
            eventFlow.CurrentEventReferences.AddRange(references);
            return eventFlow;
        }

        public async Task<bool> ValidateFlow(ICollection<Flow> flowCollection)
        {
            // set all tasks off asyncronosly..
            var tasks = new List<Task<bool>>();
            
            // for every event in the flow validate each one..
            foreach (var flow in flowCollection)
            {
                var checkEventFlow = SetFlowProperties(flowCollection, flow);

                tasks.Add(Validate(checkEventFlow));
            }

            await Task.WhenAll(tasks);

            var eventsFailedCount = tasks.Where(x => x.Result == false).Count();

            if (eventsFailedCount > 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Validate a flow object depending on its type. It will also check its success/fail events if any.
        /// </summary>
        /// <param name="flowEvent">This object contains the current event and its success/fail events if any plus any
        ///  references to the current object.</param>
        /// <returns></returns>
        private async Task<bool> Validate(CheckEventFlow flowEvent)
        {
            var eventDto = _mapper.Map<EventEntity>(flowEvent.CurrentEvent);

            if (eventDto == null || flowEvent.PassEventNotFound || flowEvent.FailEventNotFound)
            {
                AddErrorToCollection(new Error
                {
                    Key = "Bad Event Reference",
                    Message = "An event referenced in your flow does not exist for this game."
                });

                return false;
            }

            var type = eventDto.Type;
            var valid = true;

            // depending on the event type apply different rules
            if (type == EventType.Submission)
            {
                // then make sure its ok it should be set as a start event
                // it must have at least a pass round
                var hasPassEvent = flowEvent.PassEvent != null ? true : false;
                var hasFailEvent = flowEvent.FailEvent != null ? true : false;
                var passTimeCheck = false;
                var failTimeCheck = false;

                if (hasPassEvent)
                    // check start dates of pass or fail events are after the current eventdto.
                    passTimeCheck = flowEvent.PassEvent.StartDate > eventDto.EndDate ? true : false;

                if (hasFailEvent)
                    failTimeCheck = flowEvent.FailEvent.StartDate > eventDto.EndDate ? true : false;
                
                if (hasPassEvent && (!passTimeCheck)) // if it has a pass event but has failed the time check.
                {
                    valid = false;
                    AddErrorToCollection(new Error { Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + flowEvent.PassEvent.Id + ". Please check times don't overlap." });
                }

                if (hasFailEvent && (!failTimeCheck))
                {
                    valid = false;
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + flowEvent.FailEvent.Id + ". Please check times don't overlap."
                    });
                }


                if (flowEvent.CurrentEventReferences.Count > 0)
                {
                    // iterate over references to this event to make sure dates dont collide.
                    foreach (var id in flowEvent.CurrentEventReferences)
                    {
                        var eventEntity = GetEventEntity(id);

                        // now check end dates dont clash with start dates.
                        if (eventEntity.EndDate > eventDto.StartDate)
                        {
                            valid = false;
                            AddErrorToCollection(new Error
                            {
                                Key = "Event Id " + eventDto.Id,
                                Message = "Event of type '"
                                + eventDto.Type + "'. Dates clashing with another event."
                            });
                        }
                    }
                }

                flowEvent.IsValid = valid;
            }
            else if (type == EventType.Moderate)
            {
                // it MUST sit inbetween two other events.
                if (flowEvent.CurrentEventReferences.Count > 0)
                    valid = true;
                else
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '" + eventDto.Type + "' must follow another event in the flow."
                    });

                    return false;
                }

                if (flowEvent.CurrentEventIsStart)
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '" + eventDto.Type + "' cannot start a flow."
                    });

                    return false;
                }
                    

                // so check other events that this event id is used in either a pass or fail event
                // MIGHT NOT NEED TO DO THIS AS I CHECK ALL OTHER PASS FAIL EVENTS SO IT SHOULD BE PICKED UP
                //foreach (var eventRef in flowEvent.CurrentEventReferences)
                //{
                //    // this eventref MUST happen before the current one we are validating
                //    var e = GetEventEntity(eventRef);

                //    var timeCheck = false;
                //    // check end dates of pass or fail events are BEFORE the current eventdto startdate
                //    timeCheck = e.EndDate < eventDto.StartDate ? true : false;

                //    valid = timeCheck;

                //    if (!valid)
                //    {
                //        AddErrorToCollection(new Error
                //        {
                //            Key = "Event Id " + eventDto.Id,
                //            Message = "Flow for event Id " + eventDto.Id + " type of "
                //                + eventDto.Type + ". Dates clashing with another event."
                //        });

                //        break;
                //    }
                        
                //}

                // it must have at least a pass round
                var hasPassEvent = flowEvent.PassEvent != null ? true : false;
                var hasFailEvent = flowEvent.FailEvent != null ? true : false;
                var passTimeCheck = false;
                var failTimeCheck = false;

                if (hasPassEvent)
                    // check start dates of pass or fail events are after the current eventdto.
                    passTimeCheck = flowEvent.PassEvent.StartDate > eventDto.EndDate ? true : false;

                if (hasFailEvent)
                    failTimeCheck = flowEvent.FailEvent.StartDate > eventDto.EndDate ? true : false;

                if (hasPassEvent && passTimeCheck)
                    valid = true;
                else
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Flow for event Id " + eventDto.Id + " type of '"
                        + eventDto.Type + "'. Clashes with event id " + flowEvent.PassEvent.Id + ". Please check times don't overlap."
                    });
                }

                if (hasFailEvent && (!failTimeCheck))
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + flowEvent.FailEvent.Id + ". Please check times don't overlap."
                    });
                    
                    valid = false;
                }
                    

                flowEvent.IsValid = valid;
            }
            else if (type == EventType.RandomDraw)
            {
             
                // it MUST have a previous evnet.
                if (flowEvent.CurrentEventReferences.Count > 0)
                    valid = true;
                else
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                                + eventDto.Type + "' must follow another event in the flow."
                    });
                }


                if (flowEvent.CurrentEventIsStart)
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '" + eventDto.Type + "' cannot start a flow."
                    });

                    return false;
                }
                // so check other events that this event id is used in either a pass or fail event
                // MIGHT NOT NEED TO DO THIS AS I CHECK ALL OTHER PASS FAIL EVENTS SO IT SHOULD BE PICKED UP
                //foreach (var eventRef in flowEvent.CurrentEventReferences)
                //{
                //    // this eventref MUST happen before the current one we are validating
                //    var e = GetEventEntity(eventRef);

                //    var timeCheck = false;
                //    // check end dates of pass or fail events are BEFORE the current eventdto startdate
                //    timeCheck = e.EndDate < eventDto.StartDate ? true : false;

                //    valid = timeCheck;

                //    if (!valid)
                //    {
                //        AddErrorToCollection(new Error
                //        {
                //            Key = "Event Id " + eventDto.Id,
                //            Message = "Flow for event Id " + eventDto.Id + " type of "
                //                + eventDto.Type + ". Dates clashing with another event."
                //        });

                //        break;
                //    }
                //}

                // check any pass or fail events that times dont collide.
                var hasPassEvent = flowEvent.PassEvent != null ? true : false;
                var hasFailEvent = flowEvent.FailEvent != null ? true : false;
                var passTimeCheck = false;
                var failTimeCheck = false;

                if (hasPassEvent)
                    // check start dates of pass or fail events are after the current eventdto.
                    passTimeCheck = flowEvent.PassEvent.StartDate > eventDto.EndDate ? true : false;

                if (hasFailEvent)
                    failTimeCheck = flowEvent.FailEvent.StartDate > eventDto.EndDate ? true : false;

                // FAIL THE TIME CHECKS IF THEY COLLIDE WITH THIS CURRENT EVENT.
                if (hasPassEvent && (!passTimeCheck))
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + flowEvent.PassEvent.Id + ". Please check times don't overlap."
                    });

                    valid = false;
                }

                if (hasFailEvent && (!failTimeCheck))
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + flowEvent.FailEvent.Id + ". Please check times don't overlap."
                    });

                    valid = false;
                }


                flowEvent.IsValid = valid;
            }

            return valid;
        }

        public async Task<bool> DeleteFlowAsync(int gameId)
        {
            var success = true;

            var gameEntity = _context.Find(typeof(GameEntity), gameId) as GameEntity;

            if (gameEntity != null)
                gameEntity.Flow = null;
            else
                success = false;

            await _context.SaveChangesAsync();

            return success;
        }

        public async Task<ICollection<Flow>> GetFlowAsync(int gameId)
        {
            var gameEntity = await _context.FindAsync(typeof(GameEntity), gameId) as GameEntity;

            var flowDto = _mapper.Map<ICollection<Flow>>(gameEntity.Flow);

            return flowDto;
        }
    }
}
