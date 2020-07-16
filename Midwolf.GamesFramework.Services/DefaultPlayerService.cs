using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Midwolf.GamesFramework.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Midwolf.GamesFramework.Services.Models.Db;
using Midwolf.GamesFramework.Services.Storage;
using Midwolf.GamesFramework.Services.Interfaces;

namespace Midwolf.GamesFramework.Services
{
    public class DefaultPlayerService : ErrorService, IPlayerService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DefaultPlayerService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultPlayerService>();
        }

        public async Task<bool> PlayerExists(int gameId, int playerId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            var c = game.Players.Count(x => x.Id == playerId);

            return c > 0 ? true : false;
        }

        public async Task<Player> AddPlayerAsync(int gameId, Player playerDto)
        {
            if (await Validate(gameId, playerDto))
            {
                var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

                if (game != null)
                {
                    //add player to it.
                    var playerEntity = _mapper.Map<PlayerEntity>(playerDto);

                    if (game.Players == null)
                        game.Players = new List<PlayerEntity>();

                    game.Players.Add(playerEntity);

                    await _context.SaveChangesAsync();

                    playerDto.Id = playerEntity.Id;

                    return playerDto;

                }
                else
                    return null; // game wasn't found.
            }

            return null;
        }

        public async Task<bool> Validate(int gameId, Player playerDto)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game != null)
            {
                if (game.Players.Where(x => x.Email.ToLower().Trim() == playerDto.Email.ToLower().Trim()).Count() > 0)
                {
                    // email already exists
                    AddErrorToCollection(new Error { Key = "Player", Message = "Player with this email already exists for this game." });
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> DeletePlayerAsync(int playerId)
        {
            var success = true;

            var entityToUpdate = _context.Find(typeof(PlayerEntity), playerId) as PlayerEntity;

            if (entityToUpdate != null)
                _context.Players.Remove(entityToUpdate);
            else
                success = false;

            await _context.SaveChangesAsync();

            return success;
        }

        public async Task<ICollection<Player>> GetAllPlayersAsync(int gameId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game.Events.Count > 0)
            {
                var players = _mapper.Map<ICollection<Player>>(game.Players);

                return players;
            }
            else
                return null;
        }

        public async Task<Player> GetPlayerByIdAsync(int gameId, int playerId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game.Players.Count > 0)
            {
                var playerEntity = game.Players.SingleOrDefault(x => x.Id == playerId);

                var playerDto = _mapper.Map<Player>(playerEntity);

                return playerDto;
            }
            else
                return null;
        }

        public async Task<Player> UpdatePlayerAsync(Player dto)
        {
            // find the entity
            var entityToUpdate = _context.Find(typeof(PlayerEntity), dto.Id) as PlayerEntity;

            if (await Validate(entityToUpdate.GameId, dto))
            {
                // patch the entity with dto
                entityToUpdate = _mapper.Map(dto, entityToUpdate);

                // update should only update the changed values.
                _context.Update(entityToUpdate);

                await _context.SaveChangesAsync();

                var playerDto = _mapper.Map<Player>(entityToUpdate);

                return playerDto;
            }

            return null;
        }
    }
}
