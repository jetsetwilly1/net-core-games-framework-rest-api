using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models.Db
{
    public class EntryEntity
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }

        public Dictionary<object,object> Metadata { get; set; }

        public DateTime CreatedAt { get; set; }

        public int State { get; set; }

        public int GameId { get; set; }
    }
}
