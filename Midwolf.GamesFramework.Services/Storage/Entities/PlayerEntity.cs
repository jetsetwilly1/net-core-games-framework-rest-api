using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models.Db
{
    public class PlayerEntity
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public Dictionary<object, object> Metadata { get; set; }

        public int GameId { get; set; }

        public virtual ICollection<EntryEntity> Entries { get; set; }
    }
}
