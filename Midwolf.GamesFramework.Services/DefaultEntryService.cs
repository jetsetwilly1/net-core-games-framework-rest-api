using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Midwolf.GamesFramework.Services.Storage;
using Midwolf.GamesFramework.Services.Interfaces;

namespace Midwolf.GamesFramework.Services
{
    public class DefaultEntryService : ErrorService, IEntryService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRandomDrawEventService _randomDrawEventService;

        public DefaultEntryService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper,
            IRandomDrawEventService randomDrawEventService)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultEventService>();
            _randomDrawEventService = randomDrawEventService;
        }

        public async Task<Entry> AddEntryAsync(int gameId, Entry entryDto)
        {
            
            // get game record and update it
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            var playerExistsCount = game.Players.Count(x => x.Id == entryDto.PlayerId);

            if (playerExistsCount == 0)
            {
                AddErrorToCollection(new Error { Key = "EntryFailed", Message = "Player doesnt exist for this game.  No entry was added." });
                return null;
            }

            if (game != null)
            {
                // get game Chain and enter this entry to the isstart event.
                // if the entry fails these rules then it will be passed to a fail event. 
                // if the entry passes these rules it will be passed to a success event.
                // make sure the entry is within the start and end dates.

                var Chain = game.Chain.SingleOrDefault(x => x.IsStart == true);

                var gameEvent = game.Events.SingleOrDefault(x => x.Id == Chain.Id);

                var eventMap = _mapper.Map<Event>(gameEvent);
                
                // add event to it.
                var entryEntity = _mapper.Map<EntryEntity>(entryDto);

                entryEntity.CreatedAt = DateTime.UtcNow;
                entryEntity.State = Chain.Id;

                var result = Validate(entryEntity, gameEvent);

                if (result == "hardfail") // no entry was added to the game
                    return null;
                else if (result == "ok") // entry was added to game
                    entryEntity.State = Chain.SuccessEvent.Value;
                else if(result == "softfail") // entry was added to game but were errors.
                {
                    // set the entry state...
                    var failstate = Chain.FailEvent ?? Chain.Id;
                    entryEntity.State = failstate;
                }

                if (game.Entries == null)
                    game.Entries = new List<EntryEntity>();

                game.Entries.Add(entryEntity);

                await _context.SaveChangesAsync();

                entryDto = _mapper.Map<Entry>(entryEntity);

                return entryDto;
            }
            else
                return null; // game wasn't found.
        }
        
        public async Task<bool> DeleteEntryAsync(int entryId)
        {
            var success = true;

            var entityToUpdate = _context.Find(typeof(EntryEntity), entryId) as EntryEntity;

            if (entityToUpdate != null)
                _context.Entries.Remove(entityToUpdate);
            else
                success = false;

            await _context.SaveChangesAsync();

            return success;
        }

        public async Task<bool> EntryExists(int gameId, int entryId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            var c = game.Entries.Count(x => x.Id == entryId && x.State != -1);

            return c > 0 ? true : false;
        }

        public async Task<ICollection<Entry>> GetAllEntriesAsync(int gameId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game.Entries.Count(x => x.State != -1) > 0)
            {
                var entries = _mapper.Map<ICollection<Entry>>(game.Entries.Where(x => x.State != -1));

                return entries;
            }
            else
                return null;
        }

        public async Task<Entry> GetEntryAsync(int gameId, int entryId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game.Entries.Count > 0)
            {
                var entryEntity = game.Entries.SingleOrDefault(x => x.Id == entryId);

                var entryDto = _mapper.Map<Entry>(entryEntity);

                return entryDto;
            }
            else
                return null;
        }

        public async Task<Entry> UpdateEntryAsync(Entry dto)
        {
            // find the entity
            var entityToUpdate = _context.Find(typeof(EntryEntity), dto.Id) as EntryEntity;

            var currentEventId = entityToUpdate.State;

            // patch the entity with dto
            entityToUpdate = _mapper.Map(dto, entityToUpdate);

            if (currentEventId != dto.State)
            {
                // then validate state move..
                entityToUpdate.State = currentEventId;
                var targetEventId = dto.State;

                if (!ValidateStateMove(entityToUpdate, targetEventId))
                    return null;
                else
                {
                    entityToUpdate.State = dto.State;
                }
            }
            
            // update should only update the changed values.
            _context.Update(entityToUpdate);

            await _context.SaveChangesAsync();

            var entryDto = _mapper.Map<Entry>(entityToUpdate);

            return entryDto;
        }

        /// <summary>
        /// This takes a current entry and its target event and validates that the state change is valid.
        /// </summary>
        /// <param name="entry">The entry being validated.</param>
        /// <param name="targetEvent">The event the entry is being moved to.</param>
        /// <returns></returns>
        private string Validate(EntryEntity entry, EventEntity targetEvent)
        {
            var eventDto = _mapper.Map<Event>(targetEvent);

            IEventRules ruleset = eventDto.RuleSet;

            if (targetEvent.Type == EventType.Submission)
            {
                // is entry inside start and end dates
                if (entry.CreatedAt >= targetEvent.StartDate && entry.CreatedAt <= targetEvent.EndDate)
                {
                    var rules = (Submission)ruleset;

                    // get current total entries for this player for the given interval
                    var startDate = targetEvent.Game.Created;
                    
                    switch (rules.Interval)
                    {
                        case Interval.Minute:
                            startDate = entry.CreatedAt.Subtract(new TimeSpan(0, 0, (int)Interval.Minute));
                            break;
                        case Interval.Hour:
                            startDate = entry.CreatedAt.Subtract(new TimeSpan(0, 0, (int)Interval.Hour));
                            break;
                        case Interval.Day:
                            startDate = entry.CreatedAt.Subtract(new TimeSpan(0, 0, (int)Interval.Day));
                            break;
                        case Interval.Week:
                            startDate = entry.CreatedAt.Subtract(new TimeSpan(0, 0, (int)Interval.Week));
                            break;
                        case Interval.Month:
                            startDate = entry.CreatedAt.Subtract(new TimeSpan(0, 0, (int)Interval.Month));
                            break;
                        default:
                            startDate = targetEvent.Game.Created;
                            break;
                    }
                    

                    // get the amount of entries for the interval and player id
                    var entriesCount = targetEvent.Game.Entries.Where(x => x.CreatedAt >= startDate && x.PlayerId == entry.PlayerId).Count();

                    if (entriesCount >= rules.NumberEntries)
                    {
                        AddErrorToCollection(new Error { Key = "EntryFailed", Message = "Player has exceeded the number of entries for the ruleset interval. No entry was added." });
                        return "hardfail";
                    }
                }
                else
                {
                    AddErrorToCollection(new Error { Key = "EntryFailed", Message = "Event of type '" + targetEvent.Type + "' is closed. No entry was added." });

                    return "hardfail";
                }
            }
            else if (targetEvent.Type == EventType.RandomDraw)
                // TIMED ROUND WILL EXECUTE AT THE END DATE, IF MANUAL ADVANCE SET TO FALSE WILL MOVE ALL OF THEM IF TRUE 
                // THEY WILL ONLY MOVE IF ADVANCE CALL IS USED.
            {
                
                // there are no rules not allowing entries to be moved here.
                if (targetEvent.EndDate < DateTime.UtcNow)
                {
                    // then this event is closed. The end date has passed 
                    AddErrorToCollection(new Error { Key = "EntryFailed", Message = "Event of type '" + targetEvent.Type + "' enddate has passed. No entry was added." });

                    return "hardfail";
                }
            }
            else if (targetEvent.Type == EventType.Moderate) // ACTIONED MANUALLY OR
            {
                // AT THE END DATE
                // IF MANUAL ADVANCE FALSE THEY WILL ALL BE MOVED TO FALSE EVENT IF AVAILABLE
                // IF MANUAL ADVANCE TRUE THEY WILL REMAIN IN THE ROUND UNLESS MOVED BY MODERATE ENDPOINT

                // there are no rules not allowing entries to be moved here.
                
                if (targetEvent.EndDate < DateTime.UtcNow)
                {
                    // then this event is closed. The end date has passed 
                    AddErrorToCollection(new Error { Key = "EntryFailed", Message = "Event of type '" + targetEvent.Type + "' enddate has passed. No entry was added." });

                    return "hardfail";
                }
            }
            else if (targetEvent.Type == EventType.Custom) // ACTIONED MANUALLY OR
            {
                // AT THE END DATE
                // IF MANUAL ADVANCE FALSE THEY WILL ALL BE MOVED TO FALSE EVENT IF AVAILABLE
                // IF MANUAL ADVANCE TRUE THEY WILL REMAIN IN THE ROUND UNLESS MOVED BY MANUAL ENTRY PATCH

                // there are no rules not allowing entries to be moved here.

                if (targetEvent.EndDate < DateTime.UtcNow)
                {
                    // then this event is closed. The end date has passed 
                    AddErrorToCollection(new Error { Key = "EntryFailed", Message = "Event of type '" + targetEvent.Type + "' enddate has passed. No entry was added." });

                    return "hardfail";
                }
            }

            return "ok";
        }

        /// <summary>
        /// This will iterate over all entries and update there state depending on the entry chain flow for the associated game.
        /// It will move any entry to the next event if the end date has been reached and rules are valid.
        /// This will not return any errors as its use is for automatic updates only.
        /// </summary>
        /// <param name="gameId">The game associated with the entries to iterate over.</param>        
        /// <returns>A boolean when completed.</returns>
        public async Task<bool> ProcessAllEntriesStateForGame(int gameId)
        {
            var game = await _context.Games.FirstAsync(x => x.Id == gameId);

            foreach (var entry in game.Entries)
            {
                var chainEntity = game.Chain.FirstOrDefault(x => x.Id == entry.State);

                if (chainEntity != null) await ProcessEntryStateAsync(entry, chainEntity, false);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Given an entryId it will check to see if the entry needs to be automatically moved onto the next state.
        /// Check the HasErrors property on this service for any errors.
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns>ok - if the update is successfull, hardfail - errors are added to this error collection service.</returns>
        public async Task<bool> ProcessEntryStateAsync(int entryId)
        {
            var entryEntity = _context.Find(typeof(EntryEntity), entryId) as EntryEntity;

            var chainEntity = entryEntity.Game.Chain.First(x => x.Id == entryEntity.State);

            return await ProcessEntryStateAsync(entryEntity, chainEntity, true);
        }

        /// <summary>
        /// Given an entry it will check to see if the entry needs to be automatically moved onto the next state.
        /// </summary>
        /// <param name="entry">The entry to check</param>
        /// <param name="chain">The chain instance the entry belongs too.</param>
        /// <param name="saveChanges">If true any entry entity changes to state will be saved to the database. 
        /// Set this to false if you are going to call SaveChanges on the db context yourself.</param>
        /// <returns>Returns true if the entryEntity state has changed.</returns>
        private async Task<bool> ProcessEntryStateAsync(EntryEntity entry, ChainEntity chain, bool saveChanges)
        {            
            // get entry
            // get the event its in
            // is the event a timed event? if so does the entry need to move on?
            var currentEvent = await _context.Events.FirstAsync(x => x.Id == entry.State);

            if (currentEvent.TransitionType == TransitionType.Timed &&
                currentEvent.ManualAdvance == false) // if manual advance is false then we can try and move it.
            {
                var datetimeNow = DateTime.UtcNow;

                if (currentEvent.EndDate < datetimeNow) // the event it sits in now has expired...
                {
                    // then if valid this entry should move on to the next event state.
                    // first check the pass round
                    var successeventId = chain.SuccessEvent ?? 0;
                    var faileventId = chain.FailEvent ?? 0;

                    // CHECK IF ITS A MODERATE EVENT TYPE THE ENTRY IS CURRENTLY IN THEN IT NEEDS TO EITHER STAY WHERE IT IS 
                    // OR MOVE TO THE FALSE EVENT.
                    // CUSTOM EVENTS CAN ONLY BE MOVED MANUALLY BY CALLING ADVANCE OR UPDATING ENTRIES MANUALLY.
                    // CURRENTLY ALL OTHER EVENTS ARE ACTION ONLY SO ARE NOT AFFECTED BY THIS.

                    if (currentEvent.Type == EventType.Moderate)
                    {
                        // then move to false event.. or keep where it is
                        var ok = await ValidateMove(entry, faileventId);
                        if (ok)
                        {
                            entry.State = faileventId;
                            if (saveChanges) await _context.SaveChangesAsync();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempt to manually advance an events entry.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>Will return true if move successfull.</returns>
        public async Task<bool> ManuallyAdvanceEntry(EntryEntity entry)
        {
            var currentEvent = await _context.Events.FirstAsync(x => x.Id == entry.State);

            var chain = entry.Game.Chain.SingleOrDefault(x => x.Id == entry.State);

            if (currentEvent.TransitionType == TransitionType.Timed &&
                currentEvent.ManualAdvance == true) // if manual advance should be true.
            {
                // then if valid this entry should move on to the next event state.
                // first check the pass round
                var successeventId = chain.SuccessEvent ?? 0;
                var faileventId = chain.FailEvent ?? 0;

                if (currentEvent.Type == EventType.RandomDraw)
                {
                    // check event state to see if its been drawn already
                    var state = currentEvent.EventState.ToObject<RandomEventState>();
                    if (!state.IsDrawn)
                    {
                        await _randomDrawEventService.ClearDrawExecutionJobs(currentEvent.Id);
                        if (await _randomDrawEventService.ExecuteDraw(currentEvent.Id))
                        {
                            state.HangfireJobId = "ManualAdvanceTrigger";
                            state.IsDrawn = true;
                            await _context.SaveChangesAsync();
                        }
                    }
                    return true;
                }

                if (currentEvent.Type == EventType.Moderate)
                {
                    // Then they entries must be moderated to move onto the next event.
                    return false;
                }

                // all other manual advances will try success event then fail.
                if (successeventId != 0)
                {
                    if (await ValidateMove(entry, successeventId))
                    {
                        entry.State = successeventId;
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }

                if (faileventId != 0)
                {
                    if (await ValidateMove(entry, faileventId))
                    {
                        entry.State = faileventId;
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
                
            }

            return false;
        }


        /// <summary>
        /// Given an entry it validate the destination event is ok to move to.
        /// </summary>
        /// <param name="entry">the entry to move</param>
        /// <param name="destEventId">the destination event id</param>
        /// <returns>true if the move is valid</returns>
        private async Task<bool> ValidateMove(EntryEntity entry, int destEventId)
        {
            var isSuccess = false;

            if (destEventId != 0)
            {
                var destEvent = await _context.Events.FirstAsync(x => x.Id == destEventId);

                if (Validate(entry, destEvent) == "ok")
                    isSuccess = true;
            }

            return isSuccess;
        }

        /// <summary>
        /// Validate if a state update requested by a user via a patch update is valid.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="targetEventId"></param>
        /// <returns></returns>
        private bool ValidateStateMove(EntryEntity entry, int targetEventId)
        {
            // check to ensure the state they want to move to is possible.
            // get the current event state they are in..
            var chainCheck = entry.Game.Chain.SingleOrDefault(x => x.Id == entry.State);

            var failEventId = chainCheck.FailEvent ?? 0;
            var successEventId = chainCheck.SuccessEvent ?? 0;

            // if the target event state equals one of the success or fail id's... then validate move and update.
            if (failEventId == targetEventId || successEventId == targetEventId)
            {
                var destEventEntity = entry.Game.Events.SingleOrDefault(x => x.Id == targetEventId);

                var destruleset = _mapper.Map<Event>(destEventEntity);

                var destState = Validate(entry, destEventEntity);

                if (destState == "ok")
                {
                    return true;
                }
            }
            else
            {
                AddErrorToCollection(new Error { Key = "EntryUpdate", Message = "Update to entry state failed, cannot move to proposed state from current state." });
            }

            return false;
        }
    }
}
