using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Liquid.Runtime
{
    /// <summary>
    /// Apply Open Api Conventions
    /// An API specification needs to specify the responses for all API operations.
    /// </summary>
    public static class SwaggerConventions
    {
        /// <summary>
        /// Apply all conventions for AMAW microservice on http status code.
        /// HttpGet: 200, 401 and 404
        /// HttpPost: 201, 400 and 401
        /// HttpPut: 204, 400, 401 and 404
        /// HttpDelete: 400, 401 and 404
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        internal static string ApllyConventions(dynamic json)
        {
            JObject o = JObject.Parse(json);

            foreach (JObject apiPath in o["paths"].Values())
            {
                foreach (JObject httpVerbs in apiPath.Values())
                {
                    switch (((JProperty)httpVerbs.Parent).Name)
                    {
                        case "get":
                            CreateGetResponses((JObject)httpVerbs["responses"]);
                            break;
                        case "post":
                            CreatePostResponses((JObject)httpVerbs["responses"]);
                            break;
                        case "put":
                            CreatePutResponses((JObject)httpVerbs["responses"]);
                            break;
                        case "delete":
                            CreateDeleteResponses((JObject)httpVerbs["responses"]);
                            break;
                    }
                }
            }

            return o.ToString();
        }

        /// <summary>
        /// Gets convention object with the given type.
        /// </summary>
        /// <param name="conv">name of the convention</param>
        /// <returns>Returns convention objects</returns>
        private static JProperty GetConventionObject(string conv)
        {
            switch (conv)
            {
                case "200":
                    return new JProperty("200", new JObject(new JProperty("description", "Ok")));
                case "201":
                    return new JProperty("201", new JObject(new JProperty("description", "Created")));
                case "204":
                    return new JProperty("204", new JObject(new JProperty("description", "No Content")));
                case "400":
                    return new JProperty("400", new JObject(new JProperty("description", "Bad Request")));
                case "401":
                    return new JProperty("401", new JObject(new JProperty("description", "Unauthorized")));
                case "404":
                    return new JProperty("404", new JObject(new JProperty("description", "Not Found")));
                default:
                    return new JProperty("200", new JObject(new JProperty("description", "Ok")));
            }
        }

        private static void CreateDeleteResponses(JObject responses)
        {
            List<string> Deleteconventions = new List<string>() { "400", "401", "404" };
            bool isReadyConvention = false;

            foreach (var conv in Deleteconventions)
            {
                foreach (JProperty resp in responses.Properties())
                {
                    if (resp.Name == conv)
                        isReadyConvention = true;
                }
                if (!isReadyConvention)
                    responses.Add(GetConventionObject(conv));
            }
        }

        private static void CreatePutResponses(JObject responses)
        {
            List<string> Putconventions = new List<string>() { "204", "400", "401", "404" };
            bool isReadyConvention = false;

            foreach (var conv in Putconventions)
            {
                foreach (JProperty resp in responses.Properties())
                {
                    if (resp.Name == conv)
                        isReadyConvention = true;
                }
                if (!isReadyConvention)
                    responses.Add(GetConventionObject(conv));
            }
        }

        private static void CreatePostResponses(JObject responses)
        {
            List<string> Postconventions = new List<string>() { "201", "400", "401" };
            bool isReadyConvention = false;

            foreach (var conv in Postconventions)
            {
                foreach (JProperty resp in responses.Properties())
                {
                    if (resp.Name == conv)
                        isReadyConvention = true;
                }
                if (!isReadyConvention)
                    responses.Add(GetConventionObject(conv));
            }
        }

        private static void CreateGetResponses(JObject responses)
        {
            List<string> Getconventions = new List<string>() { "201", "401", "404" };
            bool isReadyConvention = false;

            foreach (var conv in Getconventions)
            {
                foreach (JProperty resp in responses.Properties())
                {
                    if (resp.Name == conv)
                        isReadyConvention = true;
                }
                if (!isReadyConvention)
                    responses.Add(GetConventionObject(conv));
            }
        }
    }
}
