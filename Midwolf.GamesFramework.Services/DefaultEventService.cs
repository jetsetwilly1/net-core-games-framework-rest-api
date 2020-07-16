using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Midwolf.GamesFramework.Services.Storage;
using Midwolf.GamesFramework.Services.Interfaces;

namespace Midwolf.GamesFramework.Services
{
    public class DefaultEventService : ErrorService, IEventService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IEntryService _defaultEntryService;
        private readonly IRandomDrawEventService _randomDrawEventService;

        public DefaultEventService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper,
            IEntryService defaultEntryService, IRandomDrawEventService randomDrawEventService)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultEventService>();
            _defaultEntryService = defaultEntryService;
            _randomDrawEventService = randomDrawEventService;
        }

        public async Task<bool> AdvanceEntriesForEvent(int gameId, int eventId)
        {
            // get all entries for an event and 
            var game = _context.Games.Find(gameId);
            
            foreach (var entry in game.Entries.Where(x => x.State == eventId))
            {
                await _defaultEntryService.ManuallyAdvanceEntry(entry);
            }

            return true;
        }

        public async Task<Event> AddEventAsync(int gameId, Event eventDto)
        {
            // get game record and update it
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game != null)
            {
                // add event to it.
                var eventEntity = _mapper.Map<EventEntity>(eventDto);
                
                if (game.Events == null)
                    game.Events = new List<EventEntity>();

                game.Events.Add(eventEntity);                

                await _context.SaveChangesAsync();

                eventDto.Id = eventEntity.Id;

                return eventDto;
            }
            else
                return null; // game wasn't found.
        }

        public async Task<Event> GetEventAsync(int gameId, int eventId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game.Events.Count > 0)
            {
                var eventEntity = game.Events.SingleOrDefault(x => x.Id == eventId);

                var eventDto = _mapper.Map<Event>(eventEntity);

                return eventDto;
            }
            else
                return null;
            
        }

        public async Task<ICollection<Event>> GetAllEventsAsync(int gameId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game.Events.Count > 0)
            {
                var events = _mapper.Map<ICollection<Event>>(game.Events);

                return events;
            }
            else
                return null;
        }

        public async Task<bool> EventExists(int gameId, int eventId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            var c = game.Events.Count(x => x.Id == eventId);
            
            return c > 0 ? true : false;
        }


        //public async Task<Event> UpdateEventAsync(Event dto)
        //{
        //    // the 
        //    var entityToUpdate = _context.Find(typeof(EventEntity), dto.Id);

        //    // patch the entity with dto
        //    var originalEntity = _mapper.Map(dto, entityToUpdate);




        //    _context.Attach(originalEntity);

        //    //var properties = patchDto.GetFilledProperties();

        //    //foreach (var property in properties)
        //    //{
        //    //    _context.Entry(entityToUpdate).Property(property).IsModified = true;
        //    //}

        //    await _context.SaveChangesAsync();

        //    var eventDto = _mapper.Map<Event>(originalEntity);

        //    return eventDto;
        //}

        public async Task<Event> UpdateEventAsync(Event dto)
        {
            // find the entity
            var entityToUpdate = _context.Find(typeof(EventEntity), dto.Id) as EventEntity;
            // patch the entity with dto
            var entityUpdated = _mapper.Map(dto, entityToUpdate);

            var chain = 0;
            if(entityToUpdate.Game.Chain != null) // if an event is in the chain they cannot update start and end dates.
                chain = entityToUpdate.Game.Chain.Count(x => x.Id == entityToUpdate.Id);

            if (entityToUpdate.Type != entityUpdated.Type)
            {
                AddErrorToCollection(new Error { Key = "UpdatesNotDone", Message = "You cannot update the 'type' of an event." });
                HasErrors = true;
            }

            if (chain > 0)
            {
                // then its in the chain so dont update start and end dates.
                if ((entityToUpdate.StartDate != entityUpdated.StartDate) ||(entityToUpdate.EndDate != entityUpdated.EndDate))
                {
                    AddErrorToCollection(new Error { Key = "UpdatesNotDone", Message = "This event is being used in the chain, you cannot amend the start or end dates." });
                    HasErrors = true;
                }
            }

            if(!HasErrors)
                // update should only update the changed values.
                _context.Update(entityToUpdate);
            
            await _context.SaveChangesAsync();

            var eventDto = _mapper.Map<Event>(entityUpdated);

            return eventDto;
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            var success = true;

            var entityToUpdate = _context.Find(typeof(EventEntity), eventId) as EventEntity;

            if (entityToUpdate != null)
            {
                var game = _context.Find(typeof(GameEntity), entityToUpdate.GameId) as GameEntity;

                var chainCount = 0;

                if(entityToUpdate.Game.Chain != null)
                    chainCount = entityToUpdate.Game.Chain.Count(x => x.Id == entityToUpdate.Id);

                if (chainCount > 0)
                {
                    var randomDrawEvents = from Chain in game.Chain
                                           join evnt in game.Events
                                                on Chain.Id equals evnt.Id
                                           where evnt.Type == EventType.RandomDraw
                                           select evnt;

                    // clear any random draw jobs
                    foreach (var randomDraw in randomDrawEvents)
                    {
                        await _randomDrawEventService.ClearDrawExecutionJobs(randomDraw.Id);
                    }
                    
                    game.Chain = null; // remove chain for this game too as the event is being used in the chain.

                    foreach (var id in _context.Entries.Where(x => x.GameId == game.Id).Select(e => e.Id))
                    {
                        var entity = new EntryEntity { Id = id };
                        _context.Entries.Attach(entity);
                        _context.Entries.Remove(entity);
                    }
                    foreach (var id in _context.Players.Where(x => x.GameId == game.Id).Select(e => e.Id))
                    {
                        var entity = new PlayerEntity { Id = id };
                        _context.Players.Attach(entity);
                        _context.Players.Remove(entity);
                    }

                    //game.Entries.Clear(); // clear all entries for this game.
                    //game.Players.Clear(); // clear all players for this game.
                }

                _context.Events.Remove(entityToUpdate);
            } 
            else
                success = false;

            await _context.SaveChangesAsync();

            return success;
        }
    }
}
