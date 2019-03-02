using AutoMapper;
using Microsoft.Extensions.Logging;
using Midwolf.GamesFramework.Services.Models;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models.CompetitionModels;
using Midwolf.GamesFramework.Services.Interfaces;

namespace Midwolf.GamesFramework.CompetitionServices
{
    public interface ICompetitionGameService
    {
        Task<Game> AddGameAsync(Competition game);
    }

    public class GameService : ICompetitionGameService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IGameService _defaultGameService;

        public GameService(ILoggerFactory loggerFactory, IMapper mapper, IGameService defaultGameService)
        {
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<GameService>();
            _defaultGameService = defaultGameService;
        }

        public async Task<Game> AddGameAsync(Competition game)
        {
            // convert to game
            var gameDto = _mapper.Map<Game>(game);

            var result = await _defaultGameService.AddGameAsync(gameDto);
            
            return result;
        }
    }
}
