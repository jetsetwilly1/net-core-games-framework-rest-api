using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IModerateService
    {
        Task<ICollection<ModerateResult>> ModerateAsync(int gameId, ICollection<ModerateEntry> moderateDto);

        Task<ICollection<Entry>> GetEntriesForModerationIdAsync(int gameId, int moderationEventId);

        Task<ICollection<Entry>> GetAllEntriesInModerationAsync(int gameId);
    }
}
