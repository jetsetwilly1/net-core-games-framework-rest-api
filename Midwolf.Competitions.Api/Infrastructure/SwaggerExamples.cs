using Midwolf.GamesFramework.CompetitionServices.Models;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Midwolf.GamesFramework.Services.Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.Competitions.Api.Infrastructure
{
    public class EntryRequestExample : IExamplesProvider
    {
        private class EntryRequestExampleJson
        {
            public string Title { get; set; }
            public EntryDataExample MetaData { get; set; }

            public int? PlayerId { get; set; }
        }

        private class EntryDataExample
        {
            public EntryStatus Status { get; set; } // ie reserved, complete or expired ie bought. It must be set to reserved first.

            public ICollection<int> Tickets { get; set; } // numbers wanting to buy

            public JObject Qualifier { get; set; } // this is a json string of questions answers
        }

        public object GetExamples()
        {
            return new EntryRequestExampleJson
            {
                Title = "My First Entry.",
                PlayerId = 203,
                MetaData = new EntryDataExample
                {
                    Status = EntryStatus.Reserved,
                    Tickets = new List<int> { 1, 43, 13, 55 },
                    Qualifier = JsonConvert.DeserializeObject<JObject>("{ Question: 'What is the capital on England?', Answer : 'C: London' }")
                }
            };
        }
    }

    


    public class CompetitionRequestExample : IExamplesProvider
    {
        private class CompetitionRequestJson
        {
            public string Title { get; set; }
            public CompetitionMetadataExample Metadata { get; set; }
        }

        private class CompetitionMetadataExample
        {
            public CompetitionState State { get; set; }

            public int TotalNumbers { get; set; }

            public int TotalWinners { get; set; }

            public int EntryExpiryInSeconds { get; set; }

            public CompetitionDetails Competition { get; set; }
        }

        public object GetExamples()
        {
            return new CompetitionRequestJson
            {
                Title = "Win an Iphone X competition",                
                Metadata = new CompetitionMetadataExample
                {
                    TotalNumbers = 300,
                    TotalWinners = 1,
                    State = CompetitionState.Running,
                    EntryExpiryInSeconds = 300, // 5 minutes
                    Competition = new CompetitionDetails
                    {
                        Title = "Win a new Iphone X",
                        Description = "iPhone X features an all-screen design with a 5.8-inch Super Retina HD display with HDR and True Tone. Designed with the most durable glass ever in an iPhone and a surgical-grade stainless steel band. Charges wirelessly. Resists water and dust. 12MP dual cameras with dual optical image stabilisation for great low-light photos.",
                        PrizeSpecifications = new Dictionary<string, ICollection<string>>
                        {
                            { "Product Details", new List<string> { "White Iphone X" } },
                            { "Dimensions", new List<string> { "Dimensions : 143.6 x 70.9 x 7.7 mm", "Weight : 174 grams" } },
                            { "Key Features", new List<string> { "RAM: 3GB", "Storage Capacity: 256GB", "Network: Unlocked", "Screen Size: 5.8 in", "Connectivity: Bluetooth, Lightning connector, NFC, Wi-Fi", "Processor: Hexa Core", "Operating System: iOS", "Camera Resolution 12.0MP" } }
                        },
                        ShortDescription = "Win an brand new amazing Iphone X unlocked",
                        ImageUrls = new List<CompetitionImage>
                        {
                            new CompetitionImage {
                                LargeImageUrl = "https://cdn.yourdomain.com/images/iphonex1large.jpg",
                                ThumbnailImageUrl = "https://cdn.yourdomain.com/images/iphonex1small.jpg"
                            },
                            new CompetitionImage {
                                LargeImageUrl = "https://cdn.yourdomain.com/images/iphonex2large.jpg",
                                ThumbnailImageUrl = "https://cdn.yourdomain.com/images/iphonex2small.jpg"
                            },
                            new CompetitionImage {
                                LargeImageUrl = "https://cdn.yourdomain.com/images/iphonex3large.jpg",
                                ThumbnailImageUrl = "https://cdn.yourdomain.com/images/iphonex3small.jpg"
                            },
                            new CompetitionImage {
                                LargeImageUrl = "https://cdn.yourdomain.com/images/iphonex4large.jpg",
                                ThumbnailImageUrl = "https://cdn.yourdomain.com/images/iphonex4small.jpg"
                            }
                        },
                        MainImageUrl = "https://cdn.yourdomain.com/images/iphonex4main.jpg"
                    }
                }
            };
        }
    }

    public class CompetitionResponseExample : IExamplesProvider<Competition>
    {
        public Competition GetExamples()
        {
            return new Competition(1551453264, 1551971664, 311, 138)
            {
                Title = "Win an Iphone X competition",
                Metadata = new CompetitionMetadata
                {
                    TotalNumbers = 300,
                    TotalWinners = 1,
                    State = CompetitionState.Running,
                    EntryExpiryInSeconds = 300, // 5 minutes
                    Competition = new CompetitionDetails
                    {
                        Title = "Win a new Iphone X",
                        Description = "iPhone X features an all-screen design with a 5.8-inch Super Retina HD display with HDR and True Tone. Designed with the most durable glass ever in an iPhone and a surgical-grade stainless steel band. Charges wirelessly. Resists water and dust. 12MP dual cameras with dual optical image stabilisation for great low-light photos.",
                        PrizeSpecifications = new Dictionary<string, ICollection<string>>
                        {
                            { "Product Details", new List<string> { "White Iphone X" } },
                            { "Dimensions", new List<string> { "Dimensions : 143.6 x 70.9 x 7.7 mm", "Weight : 174 grams" } },
                            { "Key Features", new List<string> { "RAM: 3GB", "Storage Capacity: 256GB", "Network: Unlocked", "Screen Size: 5.8 in", "Connectivity: Bluetooth, Lightning connector, NFC, Wi-Fi", "Processor: Hexa Core", "Operating System: iOS", "Camera Resolution 12.0MP" } }
                        },
                        ShortDescription = "Win an brand new amazing Iphone X unlocked",
                        ImageUrls = new List<CompetitionImage>
                        {
                            new CompetitionImage {
                                LargeImageUrl = "https://cdn.yourdomain.com/images/iphonex1large.jpg",
                                ThumbnailImageUrl = "https://cdn.yourdomain.com/images/iphonex1small.jpg"
                            },
                            new CompetitionImage {
                                LargeImageUrl = "https://cdn.yourdomain.com/images/iphonex2large.jpg",
                                ThumbnailImageUrl = "https://cdn.yourdomain.com/images/iphonex2small.jpg"
                            },
                            new CompetitionImage {
                                LargeImageUrl = "https://cdn.yourdomain.com/images/iphonex3large.jpg",
                                ThumbnailImageUrl = "https://cdn.yourdomain.com/images/iphonex3small.jpg"
                            },
                            new CompetitionImage {
                                LargeImageUrl = "https://cdn.yourdomain.com/images/iphonex4large.jpg",
                                ThumbnailImageUrl = "https://cdn.yourdomain.com/images/iphonex4small.jpg"
                            }
                        },
                        MainImageUrl = "https://cdn.yourdomain.com/images/iphonex4main.jpg"
                    }
                }
            };
        }
    }
}
