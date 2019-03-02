using Midwolf.GamesFramework.Services.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Midwolf.GamesFramework.Services.Models
{
    public static class EventType
    {
        public static string Submission = "submission";
        public static string Moderate = "moderate";
        public static string RandomDraw = "randomdraw";
    }

    public enum Interval
    {
        // TODO PUT THE INTERVAL AMOUNT IN SECONDS AGAINST EACH ONE.
        Minute = 60,
        Hour = 3600,
        Day = 86400,
        Week = 604800,
        Month = 2629746,
        Game = 0
    }

    public interface IEventRules
    {

    }
    
    public class Submission : IEventRules
    {
        [Required]
        public string Interval { get; set; }

        
        public int NumberEntries { get; set; }

        public int NumberRefferals { get; set; }
    }

    public class RandomDraw : IEventRules
    {
        [Required]
        public int? Winners { get; set; }
    }

    public class Moderate : IEventRules
    {
    }

    public interface IBaseDto
    {
        int Id { get; set; }
    }

    [JsonConverter(typeof(JsonRulesConverter))]
    public class Event : IBaseDto
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression("submission|randomdraw|moderate", ErrorMessage = "Type must be 'submission', 'randomdraw', 'moderate'")]
        public string Type { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Please ensure the title is no longer than 200 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please include the start date as a unix timestamp when this event should start.")]
        [DateLessThan("EndDate", ErrorMessage = "The endDate timestamp must be greater than the startDate.")]
        public double? StartDate { get; set; }

        [Required(ErrorMessage = "Please include the end date as a unix timestamp when this event should end.")]
        public double? EndDate { get; set; }

        //[Required]
        //public Dictionary<object, object> Rules { get; set; }

        [HasNestedValidation]
        public IEventRules RuleSet { get; set; }

        [JsonProperty("manually_advance")]
        public bool ManualAdvance { get; set; }
    }

    //public class EventPatch : IBaseDto
    //{
    //    public int Id { get; set; }
        
    //    public string Name { get; set; }

    //    public double? StartDate { get; set; }

    //    public double? EndDate { get; set; }

    //    public Dictionary<object, object> Rules { get; set; }
    //}
}
