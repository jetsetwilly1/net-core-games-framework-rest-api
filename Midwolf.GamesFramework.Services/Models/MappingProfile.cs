using AutoMapper;
using Midwolf.GamesFramework.Services.Models.Db;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Midwolf.GamesFramework.Services.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ignore the id on dto to entity and on reverse map ignore null values.
            //CreateMap<User, UserEntity>().ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => string.Join(",", src.Roles)))
            //    .ReverseMap();

            CreateMap<Entry, EntryEntity>().ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Player, PlayerEntity>().ForMember(dest => dest.Id, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Game, GameEntity>().ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds((long)src.Created).DateTime))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds((long)src.LastUpdated).DateTime))
                //.ForMember(dest => dest.EntriesCount, opt => opt.Ignore()) // ignore as this is readonly
                //.ForMember(dest => dest.Flow, opt => opt.Ignore()) // ignore as this is readonly
                //.ForMember(dest => dest.PlayersCount, opt => opt.Ignore()) // ignore as this is readonly
                .ReverseMap()
                .ForMember(dest => dest.EntriesCount, opt => opt.MapFrom(c => c.Entries.Count(x => x.State != -1))) // total entries count where state not equal to -1
                .ForMember(dest => dest.PlayersCount, opt => opt.MapFrom(c => c.Players.Count)) // total players count
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => ((DateTimeOffset)src.Created).ToUnixTimeSeconds()))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => ((DateTimeOffset)src.LastUpdated).ToUnixTimeSeconds()));

            CreateMap<Event, EventEntity>().ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds((long)src.StartDate.Value).DateTime))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds((long)src.EndDate.Value).DateTime))
                .ForMember(d => d.RuleSet, o => o.MapFrom(s => JsonConvert.SerializeObject(s.RuleSet)))
                .ReverseMap()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => ((DateTimeOffset)src.StartDate).ToUnixTimeSeconds()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => ((DateTimeOffset)src.EndDate).ToUnixTimeSeconds()))
                .ForMember(d => d.RuleSet, o => o.MapFrom(s => (IEventRules)JsonConvert.DeserializeObject(s.RuleSet, GetEventRulesetType(s.Type))));

            CreateMap<Flow, FlowEntity>().ReverseMap();
        }

        private Type GetEventRulesetType(string type)
        {
            switch (type)
            {
                case "submission":
                    return typeof(Submission);
                case "moderate":
                    return typeof(Moderate);
                case "randomdraw":
                    return typeof(RandomDraw);
                default:
                    throw new NotImplementedException(string.Format("Event ruleset type of {0} is not implemented.", type, type));
            }
        }
    }
}
