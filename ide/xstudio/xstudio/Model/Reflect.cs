using System.Collections.Generic;
using Newtonsoft.Json;

namespace xstudio.Model
{
    public class Reflect
    {
        [JsonProperty(PropertyName = "name")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "property")]
        public List<Property> Propertys { get; set; } 
    }

    public class Property
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public string Desc { get; set; }
    }
}