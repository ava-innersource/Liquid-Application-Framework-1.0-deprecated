using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Liquid.Domain
{    /// <summary>
     /// Implement Extensions of all objects
     /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Convert object content before send to server
        /// </summary>
        /// <param name="obj">Object content</param>
        /// <returns></returns>
        public static ByteArrayContent ConvertToByteArrayContent(this object value)
        {
            //return
            //    new StringContent(JsonConvert.SerializeObject(value), System.Text.Encoding.UTF8, "application/json");

            var myContent = JsonConvert.SerializeObject(value);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return byteContent;
        }

        /// <summary>Serializes the object to a JSON string.</summary>
        /// <returns>A JToken representation of the object.</returns>
        public static JToken ToJson(this object value)
        {
            return JToken.FromObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(value, Formatting.Indented)));
        }

        /// <summary>Serializes the object to a JSON string with CamelCase.</summary>
        /// <returns>A JToken representation of the object.</returns>
        public static JToken ToJsonCamelCase(this object value)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            return JToken.FromObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(value, settings)));
        }

        /// <summary>Serializes the object to a JSON string with CamelCase.</summary>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToStringCamelCase(this object value)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            return JsonConvert.SerializeObject(value, settings);
        }
    }
}
