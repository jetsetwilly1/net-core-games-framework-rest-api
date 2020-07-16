using AutoMapper;
using Hangfire;
using Microsoft.Extensions.Logging;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Midwolf.GamesFramework.Services.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services
{
    public class RandomDrawEventService : DefaultRandomDrawEventService, IRandomDrawEventService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public RandomDrawEventService(ApiDbContext context, ILoggerFactory loggerFactory, IMapper mapper) 
            : base (context, loggerFactory, mapper)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<RandomDrawEventService>();
        }

        // I COULD OVERRIDE THIS METHOD FROM THE BASE CLASS BUT I DONT NEED TO IN THIS CASE I DONT THINK.
        ///// <summary>
        ///// This will set the draw execution job to run on enddate of the event only if manual advance is set to false.
        ///// </summary>
        ///// <param name="randomEventId"></param>
        //public async override Task<bool> SetDrawExecutionJob(int randomEventId)
        //{
        //    var isSuccess = false;

        //    var randomEvent = await _context.Events.FindAsync(randomEventId);

        //    if (!randomEvent.ManualAdvance)
        //    {
        //        // set a job to run the draw at the enddate.
        //        var jobId = BackgroundJob.Schedule(() => ExecuteDraw(randomEventId), DateTimeOffset.Parse(randomEvent.EndDate.ToString("yyyy-MM-ddTHH:mm:ss")));

        //        if (!string.IsNullOrEmpty(jobId)) isSuccess = true;
        //    }

        //    return isSuccess;
        //}

        /// <summary>
        /// This method should execute the draw and move the winning entries to the success event.
        /// It will be executed primarily via a job. its overridden and will use the random.org to run the draws.
        /// </summary>
        /// <param name="randomEventId">The random event id to process.</param>
        public override async Task<bool> ExecuteDraw(int randomEventId)
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
