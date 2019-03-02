using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models
{
    // THIS IS ESSENTIALLY A BASKET UNTIL A PAYMENTID IS GIVEN THEN ITS AN ORDER
    public class EntryData
    {
        public int Status { get; set; } // ie reserved or complete ie bought. It must be set to reserved first.
        public string PaymentId { get; set; }
        public string InvoiceId { get; set; }
        public ICollection<int> Tickets { get; set; } // numbers wanting to buy
        public double Expires { get; set; } // this is set internally using the competition(game) entry expiry

        public string Qualifier { get; set; } // this is a json string of questions answers
    }

    public class Entry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please include the player id for this entry.")]
        public int? PlayerId { get; set; }

        public Dictionary<object, object> Metadata { get; set; }

        public DateTime CreatedAt { get; set; }

        public int State { get; set; } // this is the event id it currently sits in.
    }
}
