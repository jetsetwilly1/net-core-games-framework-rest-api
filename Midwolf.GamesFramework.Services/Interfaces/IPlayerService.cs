using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IPlayerService : IErrorService<Error>
    {
        Task<bool> PlayerExists(int gameId, int playerId);

        Task<Player> AddPlayerAsync(int gameId, Player eventDto);

        Task<bool> DeletePlayerAsync(int playerId);

        Task<Player> GetPlayerByIdAsync(int gameId, int playerId);

        Task<ICollection<Player>> GetAllPlayersAsync(int gameId);

        Task<Player> UpdatePlayerAsync(Player dto);
    }
}
