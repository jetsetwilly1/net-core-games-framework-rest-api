using AutoMapper;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.CompetitionModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

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
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => JObject.FromObject(src.MetaData)));

        }
    }
}
