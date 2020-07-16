using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models.Db
{
    public class RandomEventState
    {
        public string HangfireJobId { get; set; }
        public bool IsDrawn { get; set; }
    }

    public class EventEntity
    {
        public int Id { get; set; }
        public string Type { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        //public Dictionary<object,object> Rules { get; set; }

        public string RuleSet { get; set; }

        public int GameId { get; set; }

        [ForeignKey("GameId")]
        public virtual GameEntity Game { get; set; }

        public bool ManualAdvance { get; set; }

        public JObject EventState { get; set; }

        public TransitionType TransitionType { get; private set; }
    }
}
