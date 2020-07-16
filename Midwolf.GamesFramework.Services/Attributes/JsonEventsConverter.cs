using Midwolf.GamesFramework.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Midwolf.GamesFramework.Services.Attributes
{
    public class JsonRulesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IEventRules));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            var eventJson = new Event
            {
                Name = (string)obj["name"],
                Type = (string)obj["type"],
                StartDate = (double?)obj["startDate"],
                EndDate = (double?)obj["endDate"]
            };

            // get ruleset if available...
            var prop = obj.Properties().Where(p => p.Name == "ruleSet").FirstOrDefault();

            if ((string)obj["type"] == EventType.Submission && prop != null)
            {
                var rules = (JObject)prop.Value;

                var s = rules.ToObject<Submission>(serializer);
                eventJson.RuleSet = s;
            }
            else if((string)obj["type"] == EventType.Submission && prop == null)
                eventJson.RuleSet = new Submission();


            if ((string)obj["type"] == EventType.RandomDraw && prop != null)
            {
                var rules = (JObject)prop.Value;

                var s = rules.ToObject<RandomDraw>(serializer);
                eventJson.RuleSet = s;
            }
            else if ((string)obj["type"] == EventType.RandomDraw && prop == null)
                eventJson.RuleSet = new RandomDraw();


            return eventJson;
        }
        
        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
