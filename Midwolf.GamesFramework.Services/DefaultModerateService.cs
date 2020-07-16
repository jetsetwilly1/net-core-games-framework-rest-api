using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Midwolf.GamesFramework.Services.Storage;
using Midwolf.GamesFramework.Services.Interfaces;

namespace Midwolf.GamesFramework.Services
{
    public class DefaultModerateService : ErrorService, IModerateService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DefaultModerateService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultModerateService>();
        }

        public async Task<ICollection<Entry>> GetAllEntriesInModerationAsync(int gameId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game.Events.Count(x => x.Type == EventType.Moderate) > 0)
            {
                var modEvents = game.Events.Where(x => x.Type == EventType.Moderate).Select(x => x.Id);

                var entriesEntities = game.Entries.Where(x => modEvents.Contains(x.State));

                var entries = _mapper.Map<ICollection<Entry>>(entriesEntities);

                return entries;
            }
            else
                return null;
        }

        public async Task<ICollection<Entry>> GetEntriesForModerationIdAsync(int gameId, int moderationEventId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);

            if (game.Events.Count(x => x.Type == EventType.Moderate && x.Id == moderationEventId) > 0)
            {
                var entriesEntities = game.Entries.Where(x => x.State == moderationEventId);

                var entries = _mapper.Map<ICollection<Entry>>(entriesEntities);

                return entries;
            }
            else
                return null;
        }

        public async Task<ICollection<ModerateResult>> ModerateAsync(int gameId, int moderationEventId, ICollection<ModerateEntry> moderateDto)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);
            
            var ChainWithEventType = from Chain in game.Chain
                                     join evnt in game.Events
                                     on Chain.Id equals evnt.Id
                                     where evnt.Id == moderationEventId
                                     select new
                                     {
                                         Chain.Id,
                                         Chain.SuccessEvent,
                                         Chain.FailEvent,
                                         evnt.Type
                                     };

            var entriesList = new List<EntryEntity>();
            var results = new List<ModerateResult>();

            // iterate over the moderate list and move entries on etc LIMITED TO 30
            foreach (var modState in moderateDto.Take(30))
            {
                var entry = game.Entries.Where(x => x.Id == modState.Id).FirstOrDefault();

                if (entry != null)
                {
                    var ChainEvent = ChainWithEventType.Where(x => x.Id == entry.State).FirstOrDefault();

                    if (ChainEvent != null && ChainEvent.Type == EventType.Moderate)
                    {
                        if (entry != null)
                        {
                            if (modState.IsSuccess)
                                entry.State = ChainEvent.SuccessEvent.Value;
                            else
                                entry.State = ChainEvent.FailEvent ?? -1;

                            entriesList.Add(entry);



                            results.Add(new ModerateResult { Id = entry.Id, State = (entry.State == -1) ? "Entry has been deactivated." : entry.State.ToString() });
                        }
                        else
                        {
                            results.Add(new ModerateResult { Id = entry.Id, State = "Entry not in moderation event." });
                        }
                    }
                    else
                    {
                        results.Add(new ModerateResult { Id = entry.Id, State = "Entry not in moderation event." });
                    }
                }
                else
                {
                    results.Add(new ModerateResult { Id = modState.Id, State = "Entry not found." });
                }
            }

            if (entriesList.Count > 0)
                await _context.SaveChangesAsync();

            return results;
        }
    }
}
