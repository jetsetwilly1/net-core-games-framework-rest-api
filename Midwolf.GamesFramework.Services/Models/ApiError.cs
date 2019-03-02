using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models
{
    public sealed class ApiError
    {
        public const string ModelBindingErrorMessage = "Invalid parameters.";

        public ApiError()
        {
        }

        public ApiError(ICollection<Error> errors)
        {
            Message = "Request failed validation";

            var e = errors.FirstOrDefault();

            if (e != null)
                Detail = e.Key + ": " + e.Message;
        }

        public ApiError(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Creates a new <see cref="ApiError"/> from the result of a model binding attempt.
        /// The first model binding error (if any) is placed in the <see cref="Detail"/> property.
        /// </summary>
        /// <param name="modelState"></param>
        public ApiError(ModelStateDictionary modelState)
        {
            Message = ModelBindingErrorMessage;

            Detail = modelState
                .FirstOrDefault(x => x.Value.Errors.Any())
                .Value?.Errors?.FirstOrDefault()?.ErrorMessage;
        }

        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue("")]
        public string StackTrace { get; set; }

    }
}
