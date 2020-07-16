using Midwolf.GamesFramework.Services.Models.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models
{
    public class Entry : IEntry
    {
        public int Id { get; private set; }

        /// <summary>
        /// The player id this entry is associated too.
        /// </summary>
        [Required(ErrorMessage = "Please include the player id for this entry.")]
        public int? PlayerId { get; set; }

        /// <summary>
        /// A Json object of any metadata you may want to store for this entry.
        /// </summary>
        public JObject Metadata { get; set; }

        /// <summary>
        /// When the entry was created.
        /// </summary>
        public double CreatedAt { get; private set; }

        /// <summary>
        /// The current state the entry is in.
        /// </summary>
        public int State { get; set; } // this is the event id it currently sits in.
    }
}
