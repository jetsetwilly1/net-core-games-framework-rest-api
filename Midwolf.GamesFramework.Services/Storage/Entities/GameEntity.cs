using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models.Db
{
    public class GameEntity
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

        //public int EntriesCount { get; set; }

        //public int PlayersCount { get; set; }

        // stored as json string
        public JObject Metadata { get; set; }

        [InverseProperty("Game")]
        public virtual ICollection<EventEntity> Events { get; set; }

        // json flow of the events
        public virtual ICollection<FlowEntity> Flow { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApiUser User { get; set; }

        public virtual ICollection<PlayerEntity> Players { get; set; }

        public virtual ICollection<EntryEntity> Entries { get; set; }
    }
}
