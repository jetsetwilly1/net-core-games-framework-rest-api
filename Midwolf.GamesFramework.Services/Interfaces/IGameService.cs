using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IGameService
    {
        Task<bool> GameExists(int gameId);

        Task<Game> GetGameAsync(int id);

        Task<ICollection<Game>> GetAllGamesAsync(string userId);

        Task<Game> AddGameAsync(Game game);

        Task<Game> UpdateGameAsync(Game dto);

        Task<bool> DeleteGameAsync(int id);
    }
}
