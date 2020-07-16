using Midwolf.GamesFramework.Services.Attributes;
using Midwolf.GamesFramework.Services.Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Midwolf.GamesFramework.CompetitionServices.Models
{
    public class CompetitionEntry : IEntry
    {
        public int Id { get; private set; }

        public string Title { get; set; }

        [HasNestedValidation]
        public EntryMetadata Metadata { get; set; }

        [Required(ErrorMessage = "Please include the player id for this entry.")]
        public int? PlayerId { get; set; }

        [JsonIgnore]
        public int UserId { get; set; }

        public double CreatedAt { get; private set; }

        public int State { get; set; }
    }

    // THIS IS ESSENTIALLY A BASKET UNTIL A PAYMENTID IS GIVEN THEN ITS AN ORDER
    public class EntryMetadata : IValidatableObject
    {
        [Required]
        public EntryStatus Status { get; set; } // ie reserved, complete or expired ie bought. It must be set to reserved first.
        public string PaymentId { get; set; } // ie paypal id or crypto payment id
        public string InvoiceId { get; set; }
        public ICollection<int> Tickets { get; set; } // numbers wanting to buy

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public double Expires { get; private set; } // this is set internally using the competition(game) entry expiry

        public JObject Qualifier { get; set; } // this is a json string of questions answers

        public void SetExpiryTime(double expiryTime) => Expires = expiryTime;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // validate this object according to the Status set.
            if (Status == EntryStatus.Complete)
            {
                if(string.IsNullOrEmpty(PaymentId))
                    yield return new ValidationResult(
                "To set the status to 'complete' you must include a paymentId.", new[] { "PaymentId" });

                if(Tickets.Count == 0)
                    yield return new ValidationResult(
                "To set the status to 'complete' you must include tickets being purchased.", new[] { "Tickets" });

                if(Qualifier == null)
                    yield return new ValidationResult(
                "To set the status to 'complete' you must include the qualifier for this competition.", new[] { "Qualifier" });
            }            
        }
    }

    public enum EntryStatus
    {
        Reserved,
        Complete,
        Expired
    }
}
