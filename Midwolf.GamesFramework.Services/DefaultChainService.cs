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
    public class DefaultChainService : ErrorService, IChainService
    {
        private class CheckEventChain
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
        private readonly IRandomDrawEventService _randomDrawEventService;
        private int _gameId;

        public DefaultChainService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper, 
            IEventService eventService, IRandomDrawEventService randomDrawEventService)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultEventService>();
            _eventService = eventService;
            _randomDrawEventService = randomDrawEventService;
        }

        public async Task<ICollection<Chain>> AddChainAsync(int gameId, ICollection<Chain> chainDto)
        {
            _gameId = gameId;
            var game = await _context.Games.FindAsync(_gameId);

            if ((game.Chain != null) || (game.Chain != null && game.Chain.Count > 0))
            {
                AddErrorToCollection(new Error { Key = "Chain", Message = "A Chain already exists for this game." });
                return null;
            }
            else
            {
                var isValid = await ValidateChain(chainDto);
                
                if (isValid)
                {
                    // order the list by date
                    var allevents = await _eventService.GetAllEventsAsync(_gameId);

                    var ordered = allevents.OrderBy(x => x.EndDate);

                    var Chainforsaving = new List<Chain>();

                    foreach (var o in ordered)
                    {
                        if (o.Type == EventType.RandomDraw)
                            // set the scheduler to execute the random draw at the end date
                            await _randomDrawEventService.UpdateDrawExecutionJobByEndDate(o.Id);

                        var f = chainDto.SingleOrDefault(x => x.Id == o.Id);
                        if (f != null)
                            Chainforsaving.Add(f);
                    }

                    game.Chain = _mapper.Map<ICollection<ChainEntity>>(Chainforsaving);

                    await _context.SaveChangesAsync();
                    return Chainforsaving;
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

        private CheckEventChain SetChainProperties(ICollection<Chain> ChainCollection, Chain Chain)
        {
            var eventChain = new CheckEventChain()
            {
                CurrentEvent = GetEventEntity(Chain.Id.Value),
                PassEvent = GetEventEntity(Chain.SuccessEvent),
                FailEvent = GetEventEntity(Chain.FailEvent)
            };

            eventChain.PassEventNotFound = Chain.SuccessEvent.HasValue && eventChain.PassEvent == null;
            eventChain.FailEventNotFound = Chain.FailEvent.HasValue && eventChain.FailEvent == null;

            eventChain.CurrentEventIsStart = Chain.IsStart;

            var references = ChainCollection.Where(x => x.SuccessEvent == Chain.Id.Value
            || x.FailEvent == Chain.Id.Value).Select(x => x.Id.Value);

            eventChain.CurrentEventReferences = new List<int>();
            eventChain.CurrentEventReferences.AddRange(references);
            return eventChain;
        }

        public async Task<bool> ValidateChain(ICollection<Chain> ChainCollection)
        {
            // set all tasks off asyncronosly..
            var tasks = new List<Task<bool>>();

            var hasOneStartEvent = false;

            foreach (var chain in ChainCollection)
            {
                if (chain.IsStart)
                    hasOneStartEvent = true;
            }

            if (!hasOneStartEvent)
            {
                AddErrorToCollection(new Error
                {
                    Key = "Chain",
                    Message = "Chain must have at least one start event."
                });

                return false;
            }

            // for every event in the Chain validate each one..
            foreach (var Chain in ChainCollection)
            {
                var checkEventChain = SetChainProperties(ChainCollection, Chain);

                tasks.Add(Validate(checkEventChain));
            }

            await Task.WhenAll(tasks);

            var eventsFailedCount = tasks.Where(x => x.Result == false).Count();

            if (eventsFailedCount > 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Validate a Chain object depending on its type. It will also check its success/fail events if any.
        /// </summary>
        /// <param name="chainEvent">This object contains the current event and its success/fail events if any plus any
        ///  references to the current object.</param>
        /// <returns></returns>
        private async Task<bool> Validate(CheckEventChain chainEvent)
        {
            var eventDto = _mapper.Map<EventEntity>(chainEvent.CurrentEvent);

            if (eventDto == null || chainEvent.PassEventNotFound || chainEvent.FailEventNotFound)
            {
                AddErrorToCollection(new Error
                {
                    Key = "Bad Event Reference",
                    Message = "An event referenced in your Chain does not exist for this game."
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
                var hasPassEvent = chainEvent.PassEvent != null ? true : false;
                var hasFailEvent = chainEvent.FailEvent != null ? true : false;
                var passTimeCheck = false;
                var failTimeCheck = false;

                if (hasPassEvent)
                    // check start dates of pass or fail events are after the current eventdto.
                    passTimeCheck = chainEvent.PassEvent.StartDate > eventDto.EndDate ? true : false;

                if (hasFailEvent)
                    failTimeCheck = chainEvent.FailEvent.StartDate > eventDto.EndDate ? true : false;

                if (hasPassEvent && (!passTimeCheck)) // if it has a pass event but has failed the time check.
                {
                    valid = false;
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + chainEvent.PassEvent.Id + ". Please check times don't overlap."
                    });
                }

                if (hasFailEvent && (!failTimeCheck))
                {
                    valid = false;
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + chainEvent.FailEvent.Id + ". Please check times don't overlap."
                    });
                }


                if (chainEvent.CurrentEventReferences.Count > 0)
                {
                    // iterate over references to this event to make sure dates dont collide.
                    foreach (var id in chainEvent.CurrentEventReferences)
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

                chainEvent.IsValid = valid;
            }
            else if (type == EventType.Moderate)
            {
                // it MUST sit inbetween two other events.
                if (chainEvent.CurrentEventReferences.Count > 0)
                    valid = true;
                else
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '" + eventDto.Type + "' must follow another event in the Chain."
                    });

                    return false;
                }

                if (chainEvent.CurrentEventIsStart)
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '" + eventDto.Type + "' cannot start a Chain."
                    });

                    return false;
                }


                // so check other events that this event id is used in either a pass or fail event
                // MIGHT NOT NEED TO DO THIS AS I CHECK ALL OTHER PASS FAIL EVENTS SO IT SHOULD BE PICKED UP
                //foreach (var eventRef in ChainEvent.CurrentEventReferences)
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
                //            Message = "Chain for event Id " + eventDto.Id + " type of "
                //                + eventDto.Type + ". Dates clashing with another event."
                //        });

                //        break;
                //    }

                //}

                // it must have at least a pass round
                var hasPassEvent = chainEvent.PassEvent != null ? true : false;
                var hasFailEvent = chainEvent.FailEvent != null ? true : false;
                var passTimeCheck = false;
                var failTimeCheck = false;

                if (hasPassEvent)
                    // check start dates of pass or fail events are after the current eventdto.
                    passTimeCheck = chainEvent.PassEvent.StartDate > eventDto.EndDate ? true : false;

                if (hasFailEvent)
                    failTimeCheck = chainEvent.FailEvent.StartDate > eventDto.EndDate ? true : false;

                if (hasPassEvent && passTimeCheck)
                    valid = true;
                else
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Chain for event Id " + eventDto.Id + " type of '"
                        + eventDto.Type + "'. Must have a success event set and times must not clash."
                    });

                    valid = false;
                }

                if (hasFailEvent && (!failTimeCheck))
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + chainEvent.FailEvent.Id + ". Please check times don't overlap."
                    });

                    valid = false;
                }


                chainEvent.IsValid = valid;
            }
            else if (type == EventType.Custom)
            {
                // cannot start a chain
                if (chainEvent.CurrentEventIsStart)
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '" + eventDto.Type + "' cannot start a Chain."
                    });

                    return false;
                }

                return true;
            }
            else if (type == EventType.RandomDraw)
            {

                // it MUST have a previous event.
                if (chainEvent.CurrentEventReferences.Count > 0)
                    valid = true;
                else
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                                + eventDto.Type + "' must follow another event in the Chain."
                    });
                }


                if (chainEvent.CurrentEventIsStart)
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '" + eventDto.Type + "' cannot start a Chain."
                    });

                    return false;
                }
                // so check other events that this event id is used in either a pass or fail event
                // MIGHT NOT NEED TO DO THIS AS I CHECK ALL OTHER PASS FAIL EVENTS SO IT SHOULD BE PICKED UP
                //foreach (var eventRef in ChainEvent.CurrentEventReferences)
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
                //            Message = "Chain for event Id " + eventDto.Id + " type of "
                //                + eventDto.Type + ". Dates clashing with another event."
                //        });

                //        break;
                //    }
                //}

                // check any pass or fail events that times dont collide.
                var hasPassEvent = chainEvent.PassEvent != null ? true : false;
                var hasFailEvent = chainEvent.FailEvent != null ? true : false;
                var passTimeCheck = false;
                var failTimeCheck = false;

                if (hasPassEvent)
                    // check start dates of pass or fail events are after the current eventdto.
                    passTimeCheck = chainEvent.PassEvent.StartDate > eventDto.EndDate ? true : false;
                else
                {
                    // IT MUST HAVE A PASS EVENT TO PASS THE WINNERS ONTO.
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. It must have a success event to pass eventual winners onto."
                    });

                    valid = false;
                }

                if (hasFailEvent)
                    failTimeCheck = chainEvent.FailEvent.StartDate > eventDto.EndDate ? true : false;

                // FAIL THE TIME CHECKS IF THEY COLLIDE WITH THIS CURRENT EVENT.
                if (hasPassEvent && (!passTimeCheck))
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + chainEvent.PassEvent.Id + ". Please check times don't overlap."
                    });

                    valid = false;
                }

                if (hasFailEvent && (!failTimeCheck))
                {
                    AddErrorToCollection(new Error
                    {
                        Key = "Event Id " + eventDto.Id,
                        Message = "Event of type '"
                        + eventDto.Type + "'. Clashes with event id " + chainEvent.FailEvent.Id + ". Please check times don't overlap."
                    });

                    valid = false;
                }


                chainEvent.IsValid = valid;
            }

            return valid;
        }

        public async Task<bool> DeleteChainAsync(int gameId)
        {
            var success = true;

            var gameEntity = _context.Find(typeof(GameEntity), gameId) as GameEntity;

            if (gameEntity != null)
            {
                var randomDrawEvents = from Chain in gameEntity.Chain
                                       join evnt in gameEntity.Events
                                            on Chain.Id equals evnt.Id
                                       where evnt.Type == EventType.RandomDraw
                                       select evnt;

                // clear any random draw jobs
                foreach (var randomDraw in randomDrawEvents)
                {
                    await _randomDrawEventService.ClearDrawExecutionJobs(randomDraw.Id);
                }

                gameEntity.Chain = null;

                foreach (var id in _context.Entries.Where(x => x.GameId == gameId).Select(e => e.Id))
                {
                    var entity = new EntryEntity { Id = id };
                    _context.Entries.Attach(entity);
                    _context.Entries.Remove(entity);
                }
                foreach (var id in _context.Players.Where(x => x.GameId == gameId).Select(e => e.Id))
                {
                    var entity = new PlayerEntity { Id = id };
                    _context.Players.Attach(entity);
                    _context.Players.Remove(entity);
                }
                //gameEntity.Entries.Clear(); // clear all entries for this game.
                //gameEntity.Players.Clear(); // clear all players for this game.
            }
                
            else
                success = false;

            await _context.SaveChangesAsync();

            return success;
        }

        public async Task<ICollection<Chain>> GetChainAsync(int gameId)
        {
            var gameEntity = await _context.FindAsync(typeof(GameEntity), gameId) as GameEntity;

            var ChainDto = _mapper.Map<ICollection<Chain>>(gameEntity.Chain);

            return ChainDto;
        }
    }
}
