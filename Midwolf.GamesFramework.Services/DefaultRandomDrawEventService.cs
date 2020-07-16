using AutoMapper;
using Hangfire;
using Microsoft.Extensions.Logging;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Midwolf.GamesFramework.Services.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services
{
    public class DefaultRandomDrawEventService : IRandomDrawEventService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DefaultRandomDrawEventService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultRandomDrawEventService>();
        }

        /// <summary>
        /// This will set the draw execution job to run on enddate of the event only if manual advance is set to false.
        /// </summary>
        /// <param name="randomEventId"></param>
        public async virtual Task<bool> UpdateDrawExecutionJobByEndDate(int randomEventId)
        {
            var isSuccess = false;

            var randomEvent = await _context.Events.FindAsync(randomEventId);

            var state = new RandomEventState();

            if (randomEvent.EventState != null)
                state = randomEvent.EventState.ToObject<RandomEventState>();

            if (!string.IsNullOrEmpty(state.HangfireJobId)) // remove existing job
                BackgroundJob.Delete(state.HangfireJobId);
           
            // set a job to run the draw at the enddate.
            var jobId = BackgroundJob.Schedule(() => ExecuteDraw(randomEventId), DateTimeOffset.Parse(randomEvent.EndDate.ToString("yyyy-MM-ddTHH:mm:ss")));
            
            state.HangfireJobId = jobId;
            state.IsDrawn = false;

            randomEvent.EventState = JObject.FromObject(state);

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(jobId)) isSuccess = true;

            return isSuccess;
        }

        public async Task<bool> ClearDrawExecutionJobs(int randomEventId)
        {
            var randomEvent = await _context.Events.FindAsync(randomEventId);

            var state = randomEvent.EventState.ToObject<RandomEventState>();
            
            if (!string.IsNullOrEmpty(state.HangfireJobId)) // remove existing job
                BackgroundJob.Delete(state.HangfireJobId);

            state.HangfireJobId = null;
            state.IsDrawn = false;

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// This method should execute the draw and move the winning entries to the success event.
        /// It will be executed primarily via a job.
        /// </summary>
        /// <param name="randomEventId">The random event id to process.</param>
        public async virtual Task<bool> ExecuteDraw(int randomEventId)
        {
            // given the entries currently in this event check the rules to see how many winners are expected and 
            // execute the draw.
            // the winning entries are put forward to the pass event.
            var randomEvent = await _context.Events.FindAsync(randomEventId);

            if (randomEvent != null)
            {
                var entries = _context.Entries.Where(x => x.State == randomEvent.Id);

                if (entries.Count() > 0)
                {
                    var rules = JsonConvert.DeserializeObject<RandomDraw>(randomEvent.RuleSet);
                    var entryIds = entries.Select(x => x.Id).ToList();

                    var winningEntries = PickWinners(entryIds, rules.Winners.Value);

                    var game = await _context.Games.FindAsync(randomEvent.GameId);

                    var chainInfo = game.Chain.FirstOrDefault(x => x.Id == randomEvent.Id);

                    // move the winning entries to the pass event.
                    foreach (var winningEntry in winningEntries)
                    {
                        var entry = entries.FirstOrDefault(x => x.Id == winningEntry);
                        entry.State = chainInfo.SuccessEvent.Value;
                    }

                    var state = randomEvent.EventState.ToObject<RandomEventState>();
                    state.IsDrawn = true;

                    await _context.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Very rough way of generating winners.
        /// </summary>
        /// <param name="entryIds"></param>
        /// <param name="totalWinners"></param>
        /// <returns></returns>
        private IEnumerable<int> PickWinners(ICollection<int> entryIds, int totalWinners)
        {
            var rnd = new Random();

            return entryIds.OrderBy(x => rnd.Next()).Take(totalWinners);
        }
    }
}
