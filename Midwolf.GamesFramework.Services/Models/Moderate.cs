using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models
{
    public class ModerateEntry
    {
        /// <summary>
        /// The entry id being moderated.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// If true then the entry will be moved onto the Success event.  
        /// If false it will move to the False event or remain where it is.
        /// </summary>
        [JsonProperty("issuccess")]
        public bool IsSuccess { get; set; }
    }

    public class ModerateResult
    {
        public int Id { get; set; }

        public string State { get; set; }
    }
}
