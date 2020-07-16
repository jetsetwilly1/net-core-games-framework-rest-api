using AutoMapper;
using Midwolf.GamesFramework.CompetitionServices.Models;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Midwolf.GamesFramework.Services.CompetitionServices
{
    /// <summary>
    /// This mapping profile will map competition models to default models for saving to database.
    /// </summary>
    public class CompetitionMappingProfile : Profile
    {
        public CompetitionMappingProfile()
        {
            CreateMap<Competition, Game>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => JObject.FromObject(src.Metadata)))
                .ReverseMap()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata.ToObject<CompetitionMetadata>()));

            CreateMap<CompetitionEntry, Entry>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => JObject.FromObject(src.Metadata)))
                .ReverseMap()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata.ToObject<EntryMetadata>()));
        }
    }
}
