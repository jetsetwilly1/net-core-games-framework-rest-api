using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Api.Infrastructure
{
    public class GameRequest
    {
        public string Title { get; set; }
        public JObject Metadata { get; set; }
    }

    public class GameRequestExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new GameRequest
            {
                Title = "My First Game.",
                Metadata = JsonConvert.DeserializeObject<JObject>("{ description : 'This is my first game with metadata', totalWinners : 8 }")
            };
        }
    }

    public class GameResponseExample : IExamplesProvider<Game>
    {
        public Game GetExamples()
        {
            return new Game(1551453264, 1551971664, 311, 138)
            {
                Title = "My First Game",
                Metadata = JsonConvert.DeserializeObject<JObject>("{ description : 'This is my first game with metadata', totalWinners : 8 }"),
            };
        }
    }

    public class EventRequest
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public double? StartDate { get; set; }
        public double? EndDate { get; set; }
        public IEventRules RuleSet { get; set; }
    }

    public class EventRequestExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new EventRequest
            {
                Type = "submission",
                Name = "Submission event for game.",
                StartDate = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                EndDate = ((DateTimeOffset)DateTime.UtcNow.AddDays(1)).ToUnixTimeSeconds(),
                RuleSet = new Submission
                {
                    Interval = Interval.Hour,
                    NumberEntries = 5,
                    NumberRefferals = 0
                }
            };
        }
    }

    public class EventResponseExample : IExamplesProvider<Event>
    {
        public Event GetExamples()
        {
            return new Event
            {
                Name = "Moderate the entries.",
                Type = "moderate",
                StartDate = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                EndDate = ((DateTimeOffset)DateTime.UtcNow.AddDays(1)).ToUnixTimeSeconds()
            };
        }
    }
}
