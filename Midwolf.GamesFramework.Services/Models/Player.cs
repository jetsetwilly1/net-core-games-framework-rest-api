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

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public Dictionary<object, object> Metadata { get; set; }
    }
}
