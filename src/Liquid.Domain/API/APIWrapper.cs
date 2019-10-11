using Liquid.Base;
using Liquid.Base.Domain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Domain.API
{
    /// <summary>
    /// This class provide a simple way to consume rest services in a generic way. 
    /// There is no abstraction to business protocols.
    /// </summary>
    public class ApiWrapper : AbstractApiWrapper
    {
        /// <summary>
        /// Initilizes API from fixed hostname and port
        /// </summary>
        /// <param name="hostName">host that serves the API</param>
        /// <param name="port">endpoint of connection</param>
        public ApiWrapper(string hostName, int port, string suffix) : this(hostName, port, suffix,null) { }

        /// <summary>
        /// Initilizes API from fixed hostname, port and token
        /// </summary>
        /// <param name="hostName">host that serves the API</param>
        /// <param name="port">endpoint of connection</param>
        /// <param name="token">token authentication</param>
        public ApiWrapper(string hostName, int port, string suffix, string token) : base(hostName, port, suffix, token, null) { }

        /// <summary>
        /// Initilizes API from fixed APIName
        /// </summary>
        /// <param name="apiName">name of the API</param>
        public ApiWrapper(string apiName) : this(apiName, null) { }

        /// <summary>
        /// Initilizes API from fixed APIName and Token
        /// </summary>
        /// <param name="apiName">name of the API</param>
        /// <param name="token">token authentication</param>
        public ApiWrapper(string apiName, string token) : base($"{nameof(ApiWrapper)}:{apiName}", token) { } 


        /// <summary>
        /// Methods that gets the object that processes requests for the route.
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="serviceRoute">specified route prefix to connect to the service </param>
        /// <param name="header">pass additional information with the request or the response</param>
        /// <returns></returns>
        public override T Get<T>(string serviceRoute, [Optional] Dictionary<string, string> header)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            ///Calls the service following the route 
            HttpWebRequest request = null;
            dynamic response = null;

            ///Building the Request
            request = (HttpWebRequest)WebRequest.Create(MakeUri(serviceRoute));
            request.Method = "GET";
            request.ContentType = "application/json; encoding='utf-8'";

            ///Sends the authentication token if exisists, 
            if (!String.IsNullOrEmpty(_token))
                request.Headers.Add("Authorization", _token);

            if (header != null)
                foreach (var item in header)
                {
                    request.Headers.Add(item.Key, item.Value);
                }

            ///Gets the Response from API
            response = (HttpWebResponse)request.GetResponse();
            if (typeof(T) == typeof(HttpWebResponse))
            {
                return response;
            }
            else
            {
                ///Converts te Response in text to return
                string responseText;
                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseText = reader.ReadToEnd();
                }

                ///Now load the JSON Document
                return JToken.Parse(responseText).ToObject<T>();
            }
        }

        /// <summary>
        /// Methods Sync that gets the object that processes requests for the route.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <returns>Returns the object as a JToken</returns>
        public override JToken Get(string serviceRoute, [Optional] Dictionary<string, string> header)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            ///Calls the service following the route  
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            request = (HttpWebRequest)WebRequest.Create(MakeUri(serviceRoute));
            request.Method = "GET";
            request.ContentType = "application/json; encoding='utf-8'";

            ///Sends the authorization token if exisists
            if (!String.IsNullOrEmpty(_token))
                request.Headers.Add("Authorization", _token);

            if (header != null)
                foreach (var item in header)
                {
                    request.Headers.Add(item.Key, item.Value);
                }

            ///Get the Response from the API
            response = (HttpWebResponse)request.GetResponse();

            ///Converts te Response in text to return
            string responseText;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                responseText = reader.ReadToEnd();
            }

            ///Now load the JSON Document
            return JToken.Parse(responseText);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="header"></param>
        /// <returns></returns>
        public override T Post<T>(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> header)
        {
            return Send<T>("POST", serviceRoute, body, header);
        }

        /// <summary>
        /// Method Sync that POST the object that processes requests for the route and returns the service response as a JToken.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="header"></param>
        /// <returns></returns>
        public override JToken Post(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> header)
        {
            return Send("POST", serviceRoute, body, header);
        }

        /// <summary>
        /// Method Sync that POST the object that processes requests for the route and returns a Typed response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="header"></param>
        /// <returns></returns>
        public override T Put<T>(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> header)
        {
            return Send<T>("PUT", serviceRoute, body, header);
        }

        /// <summary>
        /// Method Sync that POST the object that processes requests for the route and returns the service response as a JToken.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="header"></param>
        /// <returns></returns>
        public override JToken Put(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> header)
        {
            return Send("PUT", serviceRoute, body, header);
        }
        public override T Delete<T>(string serviceRoute, [Optional]Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            //Calls the service following the route 
            HttpWebRequest request = null;
            dynamic response = null;

            request = (HttpWebRequest)WebRequest.Create(MakeUri(serviceRoute));
            request.Method = "DELETE";
            request.ContentType = "application/json";

            ///Send the authentication token if exisists
            if (!String.IsNullOrEmpty(_token))
                request.Headers.Add("Authorization", _token);
             
            if (headers != null)
                foreach (var item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            ///Get the Response
            response = (HttpWebResponse)request.GetResponse();

            if (typeof(T) == typeof(HttpWebResponse))
            {
                return response;
            }
            else
            {
                string responseText;
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseText = reader.ReadToEnd();
                }

                ///return new DomainResponse;
                return JToken.Parse(responseText).ToObject<T>();
            }
        }

        public override DomainResponse Delete(string serviceRoute, [Optional]Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty)
                                        .Replace("\"", "'");
            ///Calls the service following the route  
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            request = (HttpWebRequest)WebRequest.Create(MakeUri(serviceRoute));
            request.Method = "DELETE";
            request.ContentType = "application/json";

            ///Send the authentication token if exisists
            if (!String.IsNullOrEmpty(_token))
                request.Headers.Add("Authorization", _token);

            ///Get the Response
            response = (HttpWebResponse)request.GetResponse();
            string responseText;
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                responseText = reader.ReadToEnd();
            }

            ///return new DomainResponse(true, responseText);
            return new DomainResponse()
            {
                PayLoad = responseText
            };
        }



        /// <summary>
        /// Send a POST or PUT operation expecting a result from a JSON type.
        /// </summary>
        /// <param name="operation">possible operations per HTTP specification</param>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="headers">pass additional information with the request or the response</param>
        /// <returns></returns>
        protected override async Task<JToken> SendAsync(string operation, string serviceRoute, MultipartFormDataContent body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            using (HttpClient client = new HttpClient())
            {
                ///Sends the authentication token if exisists
                if (!String.IsNullOrEmpty(_token))
                    client.DefaultRequestHeaders.Add("Authorization", _token);

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                HttpResponseMessage response;

                if (operation == "POST")
                    response = await client.PostAsync(MakeUri(serviceRoute), body);
                else
                    response = await client.PutAsync(MakeUri(serviceRoute), body);

                if (response.StatusCode == HttpStatusCode.OK)
                    return JToken.Parse(response.Content.ReadAsStringAsync().Result);
            }

            ///return JSON
            return default(JToken);
        }

        /// <summary>
        /// Send a POST or PUT operation expecting a result from a JSON type.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="serviceRoute"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected override async Task<T> SendAsync<T>(string operation, string serviceRoute, MultipartFormDataContent body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            using (HttpClient client = new HttpClient())
            {
                ///Sends the authentication token if exisists
                if (!String.IsNullOrEmpty(_token))
                    client.DefaultRequestHeaders.Add("Authorization", _token);

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                dynamic response;

                if (operation == "POST")
                    response = await client.PostAsync(MakeUri(serviceRoute), body);
                else
                    response = await client.PutAsync(MakeUri(serviceRoute), body);

                if (typeof(T) == typeof(HttpResponseMessage))
                {
                    return response;
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                        return result.ToObject<T>();
                    }
                }
            }

            ///return JSON
            return default(T);
        }

        /// <summary>
        /// Send a POST or PUT operation expecting a result from a JSON type.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="serviceRoute"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected override async Task<T> SendAsync<T>(string operation, string serviceRoute, FormUrlEncodedContent body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            using (HttpClient client = new HttpClient())
            {
                ///Sends the authentication token if exisists
                if (!String.IsNullOrEmpty(_token))
                    client.DefaultRequestHeaders.Add("Authorization", _token);

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                dynamic response;

                if (operation == "POST")
                    response = await client.PostAsync(MakeUri(serviceRoute), body);
                else
                    response = await client.PutAsync(MakeUri(serviceRoute), body);

                if (typeof(T) == typeof(HttpResponseMessage))
                {
                    return response;
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                        return result.ToObject<T>();
                    }
                }
            }

            ///return JSON
            return default(T);
        }

        /// <summary>
        /// Method that send the object that processes requests for the route.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="headers">pass additional information with the request or the response</param>
        /// <returns></returns>
        private T Send<T>(string operation, string serviceRoute, [Optional] JToken body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            ///WebClient provides common methods for sending data to 
            ///and receiving data from a resource.
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                if (body == null)
                    body = new JObject();

                client.Headers["Content-Type"] = "application/json;charset=UTF-8";

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        client.Headers.Add(item.Key, item.Value);
                    }
                }

                ///Sends the authentication token if exisists
                if (!String.IsNullOrEmpty(_token))
                    client.Headers.Add("Authorization", _token);

                byte[] bodyByte = Encoding.UTF8.GetBytes(body.ToString());

                byte[] responsebytes = client.UploadData(MakeUri(serviceRoute), operation, bodyByte);

                ///Get the Response 
                JToken result = JToken.Parse(Encoding.UTF8.GetString(responsebytes));

                ///returns a generic object
                return result.ToObject<T>();
            }
        }

        /// <summary>
        ///Send a POST or PUT operation expecting a result from a JSON type.
        /// </summary>
        private JToken Send(string operation, string serviceRoute, [Optional] JToken body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            ///WebClient provides common methods for sending data to and
            ///receiving data from a resource.
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                if (body == null)
                    body = new JObject();

                client.Headers["Content-Type"] = "application/json;charset=UTF-8";

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        client.Headers.Add(item.Key, item.Value);
                    }
                }

                //Sends the authentication token if exisits
                if (!String.IsNullOrEmpty(_token))
                    client.Headers.Add("Authorization", _token);

                byte[] bodyByte = Encoding.UTF8.GetBytes(body.ToString());
                byte[] responsebytes = client.UploadData(MakeUri(serviceRoute), operation, bodyByte);

                //Get the Response
                JToken result = JToken.Parse(Encoding.UTF8.GetString(responsebytes));

                return result;
            }
        }

    }
}
