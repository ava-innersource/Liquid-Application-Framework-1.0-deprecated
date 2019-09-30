using System.IO;
using Liquid.Interfaces;
using Newtonsoft.Json;

namespace Liquid.Repository
{
    public  class LightAttachment : ILightAttachment
    {
        [JsonProperty(PropertyName = "_id")]
        public virtual string Id { get; set; }
        [JsonProperty(PropertyName = "_name")]
        public virtual string Name { get; set; }
        [JsonProperty(PropertyName = "_rid")]
        public virtual string ResourceId { get; set; } 
        [JsonProperty(PropertyName = "contentType")]
        public virtual string ContentType { get; set; }
        [JsonIgnore]
        public virtual Stream MediaStream { get; set; }
        [JsonIgnore]
        public string MediaLink { get; set; }
    }
} 
