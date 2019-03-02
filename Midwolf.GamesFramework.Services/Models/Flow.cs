using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Midwolf.GamesFramework.Services.Models
{
    public class Flow 
    {
        [Required(ErrorMessage = "Include the Event Id for this flow object.")]
        public int? Id { get; set; }

        public int? SuccessEvent { get; set; }

        public int? FailEvent { get; set; }

        public bool IsStart { get; set; }
    }
    
}
