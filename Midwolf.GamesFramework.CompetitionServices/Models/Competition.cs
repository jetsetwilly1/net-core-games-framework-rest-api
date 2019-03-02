using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Midwolf.GamesFramework.Services.Models.CompetitionModels
{
    public class Competition
    {
        public string Title { get; set; }

        public CompetitionData MetaData { get; set; }
    }

    public class CompetitionData
    {
        public string State { get; set; } // running, expired
        public int TotalNumbers { get; set; }
        public int TotalWinners { get; set; }

        public int EntryExpiryInMinutes { get; set; } // this is used to set entries expiry times.

        public CompetitionDetails Competition { get; set; }
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
        public string PrizeDetails { get; set; } // this is product information

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
