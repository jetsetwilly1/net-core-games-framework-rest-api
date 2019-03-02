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
        public int Id { get; set; }

        [JsonProperty("issuccess")]
        public bool IsSuccess { get; set; }
    }

    public class ModerateResult
    {
        public int Id { get; set; }

        public string State { get; set; }
    }
}
