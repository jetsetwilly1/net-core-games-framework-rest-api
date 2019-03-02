using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IEntryService : IErrorService<Error>
    {
        Task<bool> EntryExists(int gameId, int entryId);

        Task<Entry> AddEntryAsync(int gameId, Entry eventDto);

        Task<bool> DeleteEntryAsync(int entryId);

        Task<Entry> GetEntryAsync(int gameId, int entryId);

        Task<ICollection<Entry>> GetAllEntriesAsync(int gameId);

        Task<Entry> UpdateEntryAsync(Entry dto);
    }
}
