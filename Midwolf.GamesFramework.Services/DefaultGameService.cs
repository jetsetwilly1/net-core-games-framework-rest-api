using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Midwolf.GamesFramework.Services;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Midwolf.GamesFramework.Services.Storage;

namespace Midwolf.GamesFramework.Services
{
    public class DefaultGameService : IGameService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DefaultGameService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper)
        {  
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultGameService>();
        }

        public async Task<Game> AddGameAsync(Game game)
        {
            // convert to gameentity
            var gameEntity = _mapper.Map<GameEntity>(game);

            var date = DateTime.UtcNow;

            gameEntity.Created = date;
            gameEntity.LastUpdated = date;
            
            // save to db
            await _context.Games.AddAsync(gameEntity);

            await _context.SaveChangesAsync();

            // set the new entity and return.
            game = _mapper.Map<Game>(gameEntity);

            return game;
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            var success = true;

            var entityToDelete = _context.Find(typeof(GameEntity), id) as GameEntity;

            if (entityToDelete != null)
                _context.Games.Remove(entityToDelete);
            else
                success = false;

            await _context.SaveChangesAsync();

            return success;
        }

        public async Task<bool> GameExists(int gameId)
        {
            var c = await _context.Games.CountAsync(x => x.Id == gameId);

            return c > 0 ? true : false;
        }

        public async Task<ICollection<Game>> GetAllGamesAsync(string userId)
        {
            var games = _context.Games.Where(x => x.UserId == userId);

            if (games.Count() > 0)
            {
                var gamesDto = _mapper.Map<ICollection<Game>>(games);

                return gamesDto;
            }
            else
                return null;
        }

        public async Task<Game> GetGameAsync(int id)
        {
            var g = await _context.Games.SingleOrDefaultAsync(x => x.Id == id);
            var gameDto = _mapper.Map<Game>(g);
            return gameDto;
        }

        public async Task<Game> UpdateGameAsync(Game dto)
        {
            // find the entity
            var entityToUpdate = _context.Find(typeof(GameEntity), dto.Id) as GameEntity;

            // patch the entity with dto
            entityToUpdate = _mapper.Map(dto, entityToUpdate);

            // update last updated timestamp
            entityToUpdate.LastUpdated = DateTime.UtcNow;

            // update should only update the changed values.
            _context.Update(entityToUpdate);

            await _context.SaveChangesAsync();

            var gameDto = _mapper.Map<Game>(entityToUpdate);

            return gameDto;
        }
    }
}
