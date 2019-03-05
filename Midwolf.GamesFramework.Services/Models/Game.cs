using Midwolf.GamesFramework.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models
{
    public sealed class Game
    {
        public Game() { }

        public Game(double createdTimestamp, double lastupdatedTimestamp, int entriesCount, int playersCount)
        {
            // used to set private properties primarily for swagger examples.
            Created = createdTimestamp;
            LastUpdated = lastupdatedTimestamp;
            EntriesCount = entriesCount;
            PlayersCount = playersCount;
        }

        /// <summary>
        /// The Id of the Game.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Title of the game.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Metadata must be sent as a Json object.
        /// </summary>
        public JObject Metadata { get; set; }

        /// <summary>
        /// Unix timestamp when the game was created.
        /// </summary>
        public double Created { get; private set; }

        /// <summary>
        /// Unix timestamp when the game was last updated.
        /// </summary>
        public double LastUpdated { get; private set; }

        /// <summary>
        /// Total entries for this game.
        /// </summary>
        public int EntriesCount { get; private set; }

        /// <summary>
        /// Total players added to this game.
        /// </summary>
        public int PlayersCount { get; private set; }

        [JsonIgnore]
        public int UserId { get; set; }
    }
}
