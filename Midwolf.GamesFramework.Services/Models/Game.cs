using Midwolf.GamesFramework.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models
{
    public sealed class Game
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public JObject Metadata { get; set; }

        public double Created { get; set; }

        public double LastUpdated { get; set; }

        public int EntriesCount { get; set; }

        public int PlayersCount { get; set; }

        [JsonIgnore]
        public int UserId { get; set; }
    }
}
