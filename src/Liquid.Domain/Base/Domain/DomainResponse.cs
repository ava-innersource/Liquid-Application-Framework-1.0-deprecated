using Liquid.Domain;
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
        public virtual JToken PayLoad { get; set; }

        /// <summary>
        /// Business critics produced by domain business logic
        /// </summary>
        public JToken Critics { get; set; } = new List<Critic>().ToJson();

        /// <summary>
        /// Identifies the current operation
        /// </summary>
        public string OperationId { get; set; }
		
        /// <summary>
        /// When true indicate that some critics have a not found message
        /// </summary>
        [JsonIgnore]
        public bool NotFoundMessage { get; set; }

        // TODO: All properties below were added by a specific project, and I must check wether they make sense
        
        /// <summary>
        /// When true indicate that some critics have a bad request message
        /// </summary>
        [JsonIgnore]
        public bool BadRequestMessage { get; set; }

        /// <summary>
        /// When true indicate that have some generic return message
        /// </summary>
        [JsonIgnore]
        public bool GenericReturnMessage { get; set; }
        
        /// <summary>
        /// Response status code for generic return
        /// </summary>
        [JsonIgnore]        
        public int? StatusCode { get; set; }
    }
}
