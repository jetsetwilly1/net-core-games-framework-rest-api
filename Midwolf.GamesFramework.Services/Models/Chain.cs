using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Midwolf.GamesFramework.Services.Models
{
    public class Chain 
    {
        /// <summary>
        /// The Id of the event this chain is for.
        /// </summary>
        [Required(ErrorMessage = "Include the Event Id for this Chain step.")]
        public int? Id { get; set; }

        /// <summary>
        /// The id of the event if the entry is successfull.
        /// </summary>
        public int? SuccessEvent { get; set; }

        /// <summary>
        /// The id of the event if the entry fails.
        /// </summary>
        public int? FailEvent { get; set; }

        /// <summary>
        /// Set to true for a start event like 'submission'
        /// </summary>
        public bool IsStart { get; set; }
    }
    
}
