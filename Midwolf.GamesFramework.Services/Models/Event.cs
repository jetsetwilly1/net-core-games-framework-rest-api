using Midwolf.GamesFramework.Services.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Midwolf.GamesFramework.Services.Models
{
    public static class EventType
    {
        public static string Submission = "submission"; // begin

        // entries in this state HAVE to be pushed to the next event manually. 
        // its so custom actions can be dealt with by client. when the end date is reached 
        // entries in this state stay here. 
        // if manual advance is true and used they will be pushed to pass event if ok 
        // if pass is not valid then they will try to push to fail event.
        public static string Custom = "custom";

        public static string Moderate = "moderate"; // process
        public static string RandomDraw = "randomdraw"; // randomdraw
    }

    public enum Interval
    {
        Minute = 60,
        Hour = 3600,
        Day = 86400,
        Week = 604800,
        Month = 2629746,
        Game = 0
    }

    public enum TransitionType
    {
        Timed,
        Action,
        Holding
    }

    public interface IEventRules
    {

    }       

    public class Submission : IEventRules
    {
        /// <summary>
        /// Set the interval enum to determine how many entries are allowed.
        /// </summary>
        [Required(ErrorMessage ="Please add an interval.")]
        //[RegularExpression("Minute|Hour|Day|Week|Month", ErrorMessage = "Type must be 'Minute', 'Hour', 'Day', 'Week', 'Month'")]
        public Interval Interval { get; set; }

        /// <summary>
        /// The amount of entries allowed per interval.
        /// </summary>
        [Range(0, Int32.MaxValue)]
        public int NumberEntries { get; set; }

        /// <summary>
        /// NOT USED.
        /// </summary>
        [Range(0, Int32.MaxValue)]
        public int NumberRefferals { get; set; }
    }

    public class RandomDraw : IEventRules
    {
        /// <summary>
        /// Add the total number of winners that will be randomly drawn from the entries.
        /// </summary>
        [Required(ErrorMessage = "Please enter how many winners are expected.")]
        [Range(1, int.MaxValue,ErrorMessage = "Please enter how many 'winners' are expected.")]
        public int? Winners { get; set; }
    }

    public class Moderate : IEventRules
    {
    }

    public class Custom : IEventRules
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

        /// <summary>
        /// Type can be either submission|randomdraw|moderate|custom
        /// </summary>
        [Required]
        [RegularExpression("submission|randomdraw|moderate|custom", ErrorMessage = "Type must be 'submission', 'randomdraw', 'moderate', 'custom'")]
        public string Type { get; set; }

        /// <summary>
        /// Name of the event, for example 'Moderate entries'
        /// </summary>
        [Required]
        [StringLength(200, ErrorMessage = "Please ensure the title is no longer than 200 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Unix timestamp
        /// </summary>
        [Required(ErrorMessage = "Please include the start date as a unix timestamp when this event should start.")]
        [DateLessThan("EndDate", ErrorMessage = "The endDate timestamp must be greater than the startDate.")]
        public double? StartDate { get; set; }

        /// <summary>
        /// Unix timestamp
        /// </summary>
        [Required(ErrorMessage = "Please include the end date as a unix timestamp when this event should end.")]
        public double? EndDate { get; set; }

        /// <summary>
        /// Some events require a ruleset definition.
        /// </summary>
        [HasNestedValidation]
        public IEventRules RuleSet { get; set; }

        /// <summary>
        /// Set to true if you want to move entries manually.
        /// </summary>
        [JsonProperty("manually_advance")]
        public bool ManualAdvance { get; set; }

        [JsonIgnore]
        public TransitionType TransitionType { get; private set; }
    }
}
