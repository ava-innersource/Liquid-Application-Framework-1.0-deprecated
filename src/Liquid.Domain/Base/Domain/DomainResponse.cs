using Liquid.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Liquid.Base.Domain
{
    /// <summary>
    /// Base type for returning business data and critic from domain classes
    /// </summary>
    public class DomainResponse
    {
        /// <summary>
        /// Viewmodel data
        /// </summary>
        [JsonIgnore]
        public object ViewModelData { get; set; }

        /// <summary>
        /// Modeol Data
        /// </summary>
        [JsonIgnore]
        public object ModelData { get; set; }

        /// <summary>
        /// Response data serialized support (bytes, JSON, XML and etc.)
        /// </summary>
        public JToken PayLoad { get; set; }

        /// <summary>
        /// Business critics produced by domain business logic
        /// </summary>
        public JToken Critics { get; set; }

        /// <summary>
        /// When true indicate that some critics have a not found message
        /// </summary>
        [JsonIgnore]
        public bool NotFoundMessage { get; set; }
    }
}