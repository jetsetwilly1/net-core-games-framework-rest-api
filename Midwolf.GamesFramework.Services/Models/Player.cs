using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models
{
    public class Player
    {
        public int Id { get; set; }

        /// <summary>
        /// A players email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// A Json object of any metadata you may want to store for this player.
        /// </summary>
        public JObject Metadata { get; set; }
    }
}
