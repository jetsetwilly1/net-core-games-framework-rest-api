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

        public DefaultEntryService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultEventService>();
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
                // get game flow and enter this entry to the isstart event.
                // if the entry fails these rules then it will be passed to a fail event. 
                // if the entry passes these rules it will be passed to a success event.
                // make sure the entry is within the start and end dates.

                var flow = game.Flow.SingleOrDefault(x => x.IsStart == true);

                var gameEvent = game.Events.SingleOrDefault(x => x.Id == flow.Id);

                var eventMap = _mapper.Map<Event>(gameEvent);
                
                // add event to it.
                var entryEntity = _mapper.Map<EntryEntity>(entryDto);

                entryEntity.CreatedAt = DateTime.UtcNow;
                entryEntity.State = flow.Id;

                var result = Validate(entryEntity, gameEvent, eventMap.RuleSet);

                if (result == "hardfail") // no entry was added to the game
                    return null;
                else if (result == "ok") // entry was added to game
                    entryEntity.State = flow.SuccessEvent.Value;
                else if(result == "softfail") // entry was added to game but were errors.
                {
                    // set the entry state...
                    var failstate = flow.FailEvent ?? flow.Id;
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
                var game = _context.Games.SingleOrDefault(x => x.Id == entityToUpdate.GameId);

                if (!ValidateStateMove(entityToUpdate, currentEventId, game))
                    return null;
            }
            
            // update should only update the changed values.
            _context.Update(entityToUpdate);

            await _context.SaveChangesAsync();

            var entryDto = _mapper.Map<Entry>(entityToUpdate);

            return entryDto;
        }

        private string Validate(EntryEntity entry, EventEntity gameEvent, IEventRules ruleset)
        {
            if (gameEvent.Type == EventType.Submission)
            {
                // is entry inside start and end dates
                if (entry.CreatedAt >= gameEvent.StartDate && entry.CreatedAt <= gameEvent.EndDate)
                {
                    var rules = (Submission)ruleset;

                    // get current total entries for this player for the given interval
                    var startDate = gameEvent.Game.Created;
                    Interval interval;
                    if (Enum.TryParse(rules.Interval.First().ToString().ToUpper() + rules.Interval.Substring(1), out interval))
                    {
                        switch (interval)
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
                                startDate = gameEvent.Game.Created;
                                break;
                        }
                    }

                    // get the amount of entries for the interval and player id
                    var entriesCount = gameEvent.Game.Entries.Where(x => x.CreatedAt >= startDate && x.PlayerId == entry.PlayerId).Count();

                    if (entriesCount >= rules.NumberEntries)
                    {
                        AddErrorToCollection(new Error { Key = "EntryFailed", Message = "Player has exceeded the number of entries for the ruleset interval. No entry was added." });
                        return "hardfail";
                    }
                }
                else
                {
                    AddErrorToCollection(new Error { Key = "EntryFailed", Message = "Event of type '" + gameEvent.Type + "' is closed. No entry was added." });

                    return "hardfail";
                }
            }
            else if (gameEvent.Type == EventType.RandomDraw)
            {
                // there are no rules not allowing entries to be moved here.
            }
            else if (gameEvent.Type == EventType.Moderate)
            {
                // there are no rules not allowing entries to be moved here.
            }

            return "ok";
        }

        private bool ValidateStateMove(EntryEntity entry, int currentEventId, GameEntity game)
        {
            var sourceEventEntity = game.Events.SingleOrDefault(x => x.Id == currentEventId);

            var currentruleset = _mapper.Map<Event>(sourceEventEntity);
            var currentState = Validate(entry, sourceEventEntity, currentruleset.RuleSet);

            if (currentState == "ok")
            {
                // check to ensure the state they want to move to is possible.
                var flowCheck = game.Flow.SingleOrDefault(x => x.Id == currentEventId);

                if (flowCheck.FailEvent == entry.State || flowCheck.SuccessEvent == entry.State)
                {
                    var destEventEntity = game.Events.SingleOrDefault(x => x.Id == entry.State);

                    var destruleset = _mapper.Map<Event>(destEventEntity);

                    var destState = Validate(entry, destEventEntity, destruleset.RuleSet);

                    if (destState == "ok")
                    {
                        return true;
                    }
                }
                else
                {
                    AddErrorToCollection(new Error { Key = "EntryUpdate", Message = "Update to entry state failed, cannot move to proposed state from current state." });
                }
            }

            return false;
        }
    }
}
