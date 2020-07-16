using AutoMapper;
using Microsoft.Extensions.Logging;
using Midwolf.GamesFramework.Services.Models;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.CompetitionServices.Models;
using System.Collections.Generic;
using Midwolf.GamesFramework.Services;
using Midwolf.GamesFramework.Services.Storage;
using Midwolf.GamesFramework.Services.Models.Db;
using Newtonsoft.Json;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Midwolf.GamesFramework.CompetitionServices
{
    public interface ICompetitionEntryService : IErrorService<Error>
    {
        Task<CompetitionEntry> AddEntryAsync(int competitionId, CompetitionEntry entry);

        Task<CompetitionEntry> GetEntryAsync(int gameId, int entryId);

        Task<ICollection<CompetitionEntry>> GetAllEntriesAsync(int competitionId);

        Task<CompetitionEntry> UpdateEntryAsync(Entry dto);

        Task<bool> CheckEntryExpired(int entityId);

        //Task<bool> UpdateEntryStateAsync(int entryId);

        Task<bool> UpdateAllEntriesStateForGame(int gameId);

        Task<bool> UpdateAllEntriesExpiryStatus(int competitionId);
    }

    public class EntryService : ErrorService, ICompetitionEntryService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IEntryService _defaultEntryService;
        private readonly ICompetitionGameService _gameService;

        public EntryService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper
            , IEntryService defaultEntryService, ICompetitionGameService gameService)
        {
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<GameService>();
            _defaultEntryService = defaultEntryService;
            _gameService = gameService;
            _context = context;
        }

        public async Task<CompetitionEntry> AddEntryAsync(int competitionId, CompetitionEntry competition)
        {
            // convert to entry
            var entryDto = _mapper.Map<Entry>(competition);
            var dtoMetadata = entryDto.Metadata.ToObject<EntryMetadata>();
            // set the expiry time.
            dtoMetadata = await SetEntryDataExpiryTime(competitionId, dtoMetadata);

            entryDto.Metadata = JObject.FromObject(dtoMetadata);

            var result = await _defaultEntryService.AddEntryAsync(competitionId, entryDto);

            if (_defaultEntryService.HasErrors)
            {
                Errors = _defaultEntryService.Errors; // just map any errors.
                HasErrors = true;
            }

            var competitionResult = _mapper.Map<CompetitionEntry>(result);

            return competitionResult;
        }

        /// <summary>
        /// Updates all competition entries status, checks to see if they are expired and marks accordingly.
        /// </summary>
        /// <param name="competitionId">The competition associated with the entries.</param>
        /// <returns></returns>
        public async Task<bool> UpdateAllEntriesExpiryStatus(int competitionId)
        {
            var entityDb = _context.Find(typeof(GameEntity), competitionId) as GameEntity;
            
            foreach (var entry in entityDb.Entries)
            {
                await CheckEntryExpired(entry.Id);
            }

            return true;
        }

        /// <summary>
        /// Check to see if an entry is expired. If it has expired then the entry metadata status is updated to 'Expired'
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns>A boolean indicating if the entry has expired.</returns>
        public async Task<bool> CheckEntryExpired(int entityId)
        {
            var entityDb = _context.Find(typeof(EntryEntity), entityId) as EntryEntity;

            var originalEntry = _mapper.Map<Entry>(entityDb);
            var entryMetadata = originalEntry.Metadata.ToObject<EntryMetadata>();

            if (entryMetadata.Status == EntryStatus.Expired)
                return true;

            var currentUnixTimeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();

            if (entryMetadata.Expires > currentUnixTimeStamp)
            {
                entryMetadata.Status = EntryStatus.Expired;

                entityDb.Metadata = JObject.FromObject(entryMetadata);

                await _context.SaveChangesAsync();

                return true;
            }
            else
                return false;
        }

        public async Task<ICollection<CompetitionEntry>> GetAllEntriesAsync(int competitionId)
        {
            var entries = await _defaultEntryService.GetAllEntriesAsync(competitionId);

            var competitionEntries = _mapper.Map<ICollection<CompetitionEntry>>(entries);

            return competitionEntries;
        }

        public async Task<CompetitionEntry> GetEntryAsync(int competitionId, int entryId)
        {
            var entry = await _defaultEntryService.GetEntryAsync(competitionId, entryId);

            var competitionEntry = _mapper.Map<CompetitionEntry>(entry);

            return competitionEntry;
        }

        public EntryMetadata GetEntryMetaData(JObject metadata)
        {
            return metadata.ToObject<EntryMetadata>();
        }

        public async Task<EntryMetadata> SetEntryDataExpiryTime(int competitionId, EntryMetadata metadata)
        {
            var competitionEntity = await _gameService.GetGameAsync(competitionId);

            var competitionMetadata = competitionEntity.Metadata;

            var newExpiry = DateTime.UtcNow.AddSeconds(competitionMetadata.EntryExpiryInSeconds.Value);

            metadata.SetExpiryTime(((DateTimeOffset)newExpiry).ToUnixTimeSeconds());

            return metadata;
        }

        /// <summary>
        /// Process the entries state, some entries might need to be moved on and this method will do that.
        /// </summary>
        /// <param name="competitionId"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAllEntriesStateForGame(int competitionId)
        {
            return await _defaultEntryService.ProcessAllEntriesStateForGame(competitionId);
        }

        /// <summary>
        /// This will check the entry can be updated before carrying it out.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<CompetitionEntry> UpdateEntryAsync(Entry dto)
        {
            var dtoMetadata = GetEntryMetaData(dto.Metadata);

            // they cant update a complete or expired entry.
            //if (dtoMetadata.Status == EntryStatus.Complete || dtoMetadata.Status == EntryStatus.Expired)
            //{
            //    // the model has validated that the necessary properties are populated.
            //    var entryDto = await _defaultEntryService.UpdateEntryAsync(dto);

            //    if (_defaultEntryService.HasErrors)
            //    {
            //        Errors = _defaultEntryService.Errors; // just map any errors from underying service.
            //        HasErrors = true;
            //    }

            //    var competitionEntry = _mapper.Map<CompetitionEntry>(entryDto);
            //    return competitionEntry;
            //}

            if (dtoMetadata.Status == EntryStatus.Reserved) // its ok to update the entry.
            {
                // at this point the expiry timestamp is ok so no need to check it.
                var entityToUpdate = _context.Find(typeof(EntryEntity), dto.Id) as EntryEntity;

                var originalEntry = _mapper.Map<Entry>(entityToUpdate);
                var originalEntryMetadata = GetEntryMetaData(originalEntry.Metadata);

                // before applying the patch i need to check to see if expiry time needs updating.
                // by seeing if there tickets being purchased has changed.
                var differences = dtoMetadata.Tickets.Except(originalEntryMetadata.Tickets);

                if (differences.Count() > 0)
                {
                    dtoMetadata = await SetEntryDataExpiryTime(entityToUpdate.GameId, dtoMetadata);

                    // update the dto with the amended metadata.
                    dto.Metadata = JObject.FromObject(dtoMetadata);
                }

                var entryDto = await _defaultEntryService.UpdateEntryAsync(dto);

                if (_defaultEntryService.HasErrors)
                {
                    Errors = _defaultEntryService.Errors; // just map any errors.
                    HasErrors = true;
                }

                var competitionEntry = _mapper.Map<CompetitionEntry>(entryDto);
                return competitionEntry;
            }
            else
            {
                AddErrorToCollection(new Error { Key = "entrynotreserved", Message = "Cannot update as entry is in '" + dtoMetadata.Status.ToString() + "' status.  An entry can only be updated if status is 'reserved'" });
            }

            return null;
        }

        /// <summary>
        /// Check numbers are available from the entries already added. Given numbers check they are available
        /// and return the numbers that are.
        /// </summary>
        /// <param name="tickets">Collection of numbers to check.</param>
        /// <returns>Collection of numbers that are available from the numbers passed in.</returns>
        //public ICollection<int> CheckTicketsAvailable(int gameId, ICollection<int> tickets)
        //{
        //    // todo get entries that in reserved or complete state.
        //    var entriesToCheck = _context.Games.First(x => x.Id == gameId).
        //        Entries.
        //        Where(x => x.Metadata.ToObject<EntryMetadata>().Status == EntryStatus.Complete ||
        //        x.Metadata.ToObject<EntryMetadata>().Status == EntryStatus.Reserved);

        //    foreach (var entry in entriesToCheck)
        //    {
        //        var mdata = entry.Metadata.ToObject<EntryMetadata>();

        //        mdata.Tickets.ToList().Exists(tickets)
        //    }
        //}

        public async Task<bool> UpdateEntryStateAsync(int entryId)
        {
            return await _defaultEntryService.ProcessEntryStateAsync(entryId);
        }
    }
}
