using AutoMapper;
using Microsoft.Extensions.Logging;
using Midwolf.GamesFramework.Services.Models;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.CompetitionServices.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace Midwolf.GamesFramework.CompetitionServices
{
    public interface ICompetitionGameService
    {
        Task<Competition> GetGameAsync(int competitionId);
        Task<ICollection<Competition>> GetAllGamesAsync(string userId);
        Task<Competition> AddGameAsync(Competition game);
        Task<Competition> UpdateGameAsync(Game dto);
        CompetitionMetadata GetCompetitionMetaData(JObject metadata);
    }

    public class GameService : ICompetitionGameService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IGameService _defaultGameService;
        private readonly IEntryService _defaultEntryService;
        private readonly IEventService _defaultEventService;
        private readonly IChainService _defaultChainService;

        public GameService(ILoggerFactory loggerFactory, IMapper mapper, 
            IGameService defaultGameService, IEntryService defaultEntryService,
            IEventService defaultEventService, IChainService defaultChainService)
        {
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<GameService>();
            _defaultGameService = defaultGameService;
            _defaultEntryService = defaultEntryService;
            _defaultEventService = defaultEventService;
            _defaultChainService = defaultChainService;
        }

        public CompetitionMetadata GetCompetitionMetaData(JObject metadata)
        {
            return metadata.ToObject<CompetitionMetadata>();
        }

        public async Task<Competition> AddGameAsync(Competition competition)
        {
            // convert to game
            var gameDto = _mapper.Map<Game>(competition);

            var result = await _defaultGameService.AddGameAsync(gameDto);

            // now we add the Chain for a competition as its always the same.
            // submission, moderation, randomdraw

            var submissionEvent = new Event
            {
                Name = "Submission Event",
                
            };

            var competitionDto = _mapper.Map<Competition>(result);

            return competitionDto;
        }

        public async Task<Competition> UpdateGameAsync(Game dto)
        {
            var gameDto = await _defaultGameService.UpdateGameAsync(dto);

            var competitionDto = _mapper.Map<Competition>(gameDto);

            return competitionDto;
        }

        public async Task<Competition> GetGameAsync(int competitionId)
        {
            var gameDto = await _defaultGameService.GetGameAsync(competitionId);

            var competitionDto = _mapper.Map<Competition>(gameDto);
            
            competitionDto.Metadata.SetTicketState(await SetTicketsStateForCompetition(competitionId));

            return competitionDto;
        }

        public async Task<ICollection<Competition>> GetAllGamesAsync(string userId)
        {
            var gamesDto = await _defaultGameService.GetAllGamesAsync(userId);

            if (gamesDto != null)
            {
                var competitionsDto = _mapper.Map<ICollection<Competition>>(gamesDto);

                foreach (var competition in competitionsDto)
                {
                    competition.Metadata.SetTicketState(await SetTicketsStateForCompetition(competition.Id));
                }

                return competitionsDto;
            }

            return null;
        }

        private async Task<TicketsState> SetTicketsStateForCompetition(int competitionId)
        {
            var entries = await _defaultEntryService.GetAllEntriesAsync(competitionId);

            var competitionEntries = _mapper.Map<ICollection<CompetitionEntry>>(entries);

            if (competitionEntries == null || competitionEntries.Count == 0)
                return null;

            var reservedNumbers = new List<int>();

            foreach (var entry in competitionEntries.Where(x => x.Metadata.Status == EntryStatus.Reserved))
            {
                reservedNumbers.AddRange(entry.Metadata.Tickets);
            }

            var soldNumbers = new List<int>();

            foreach (var entry in competitionEntries.Where(x => x.Metadata.Status == EntryStatus.Complete))
            {
                soldNumbers.AddRange(entry.Metadata.Tickets);
            }

            return new TicketsState
            {
                Reserved = reservedNumbers,
                Sold = soldNumbers,
                TotalReserved = reservedNumbers.Count,
                TotalSold = soldNumbers.Count
            };
        }
    }
}
