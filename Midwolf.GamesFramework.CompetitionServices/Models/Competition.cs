using Midwolf.GamesFramework.Services.Attributes;
using Midwolf.GamesFramework.Services.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Midwolf.GamesFramework.CompetitionServices.Models
{
    public enum CompetitionState
    {
        Running,
        SoldOut,
        Drawn,
        Expired
    }

    public class Competition : IGame
    {
        public Competition() { }

        public Competition(double createdTimestamp, double lastupdatedTimestamp, int entriesCount, int playersCount)
        {
            // used to set private properties primarily for swagger examples.
            Created = createdTimestamp;
            LastUpdated = lastupdatedTimestamp;
            EntriesCount = entriesCount;
            PlayersCount = playersCount;
        }

        public int Id { get; private set; }

        public string Title { get; set; }

        [HasNestedValidation]
        public CompetitionMetadata Metadata { get; set; }

        public double Created { get; private set; }

        public double LastUpdated { get; private set; }

        public int EntriesCount { get; private set; }

        public int PlayersCount { get; private set; }

        [JsonIgnore]
        public int UserId { get; set; }
    }

    public class CompetitionMetadata
    {
        [ReadOnly(true)]
        public TicketsState TicketsState { get; private set; }

        [Required]
        public CompetitionState? State { get; set; } // running, expired
        [Required]
        public int? TotalNumbers { get; set; }
        [Required]
        public int? TotalWinners { get; set; }
        [Required]
        public int? EntryExpiryInSeconds { get; set; } // this is used to set entries expiry times.

        [Required]
        [HasNestedValidation]
        public CompetitionDetails Competition { get; set; }

        public void SetTicketState(TicketsState ticketState)
        {
            TicketsState = ticketState;
        }
    }

    public class CompetitionDetails
    {
        [Required(ErrorMessage = "Add the title for this Competition.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "In no more than 50 characters add a short description.")]
        [StringLength(50, ErrorMessage = "You have exceeded 50 characters.")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Add a description using a maximum of 1000 characters.")]
        [StringLength(1000, ErrorMessage = "You have exceeded 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please include the player id for this entry.")]
        public string MainImageUrl { get; set; } // this is the 1000w by 750h image.

        [Required(ErrorMessage = "Please include the player id for this entry.")]
        public Dictionary<string, ICollection<string>> PrizeSpecifications { get; set; } // this is product information

        public ICollection<CompetitionImage> ImageUrls { get; set; }
    }

    public class CompetitionImage
    {
        public string LargeImageUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
    }
    
    public class TicketsState
    {
        public int TotalSold { get; set; }

        public int TotalReserved { get; set; }

        public ICollection<int> Sold { get; set; }

        public ICollection<int> Reserved { get; set; }
    }
}
