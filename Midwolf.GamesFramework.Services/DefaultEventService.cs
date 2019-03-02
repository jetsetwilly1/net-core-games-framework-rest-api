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
    public class DefaultEventService : IEventService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        //private readonly IDefaultService _defaultServices;

        public DefaultEventService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper)//, IDefaultService defaultService)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultEventService>();
            //_defaultServices = defaultService;
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
            var entityToUpdate = _context.Find(typeof(EventEntity), dto.Id);

            // patch the entity with dto
            entityToUpdate = _mapper.Map(dto, entityToUpdate);

            // update should only update the changed values.
            _context.Update(entityToUpdate);
            
            await _context.SaveChangesAsync();

            var eventDto = _mapper.Map<Event>(entityToUpdate);

            return eventDto;
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            var success = true;

            var entityToUpdate = _context.Find(typeof(EventEntity), eventId) as EventEntity;

            if (entityToUpdate != null)
                _context.Events.Remove(entityToUpdate);
            else
                success = false;

            await _context.SaveChangesAsync();

            return success;
        }
    }
}
