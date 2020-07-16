using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IChainService : IErrorService<Error>
    {
        Task<ICollection<Chain>> AddChainAsync(int gameId, ICollection<Chain> eventDto);

        Task<bool> DeleteChainAsync(int gameId);

        Task<ICollection<Chain>> GetChainAsync(int gameId);
    }
}
