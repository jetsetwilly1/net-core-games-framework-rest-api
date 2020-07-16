using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IEventService : IErrorService<Error>
    {
        Task<bool> AdvanceEntriesForEvent(int gameId, int eventId);

        Task<bool> EventExists(int gameId, int eventId);

        Task<Event> AddEventAsync(int gameId, Event eventDto);

        //Task<Event> UpdateEventAsync(int gameId, Event eventDto, int eventId);

        Task<bool> DeleteEventAsync(int eventId);

        Task<Event> GetEventAsync(int gameId, int eventId);

        Task<ICollection<Event>> GetAllEventsAsync(int gameId);

        Task<Event> UpdateEventAsync(Event dto);
    }
}
