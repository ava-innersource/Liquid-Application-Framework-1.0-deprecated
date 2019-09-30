using Liquid.Base;
using Liquid.Base.Domain;
using Liquid.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Domain.API
{
    /// <summary>
    /// This API provide a simple way to consume rest services
    /// </summary>
    public sealed class LightApi : AbstractApiWrapper
    {
        #region Class Definition
        private ICriticHandler CritictHandler { get; set; }
        private HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Initilizes API from fixed hostname, port and token
        /// </summary>
        /// <param name="hostName">host that serves the API</param>
        /// <param name="port">endpoint of connection</param>
        /// <param name="token">token authentication</param>        
        public LightApi(string hostName, int port, string suffix, string token, LightDomain lightDomain) : base(hostName, port, suffix, token, lightDomain) { }

        /// <summary>
        /// Initilizes API from fixed APIName
        /// </summary>
        /// <param name="apiName">name of the API</param>
        public LightApi(string apiName) : this(apiName, null) { }

        /// <summary>
        /// Initilizes API from fixed APIName and Token
        /// </summary>
        /// <param name="apiName">name of the API</param>
        /// <param name="token">token authentication</param>
        public LightApi(string apiName, string token) : base($"{nameof(LightApi)}:{apiName}", token) { }


        public void SetTestClient(HttpClient client)
        {
            httpClient = client;
            _endpoint = _suffix + "/";
        }

        #endregion

        #region Public Methods

        #region Sync Methods

        /// <summary>
        ///  Methods Sync that GETs the object that processes requests for the route.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="header"></param>
        /// <returns>Returns object data to the APIWrapping</returns>
        public override T Get<T>(string serviceRoute, [Optional] Dictionary<string, string> header)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            ///Create the Request
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                ///Sends the authorization token if exisists
                if (!string.IsNullOrEmpty(_token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                if (header != null)
                    foreach (var item in header)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }

                ///Get the Response from the API
                var httpResponseMessage = base.ResilientRequest(MakeUri(serviceRoute), httpClient, WorkBenchServiceHttp.GET).Result;

                if (typeof(T).Equals(typeof(HttpResponseMessage)))
                {
                    return (T)Convert.ChangeType(httpResponseMessage, typeof(T));
                }

                ///Now load the JSON Document
                return HandleResult<T>(JToken.Parse(httpResponseMessage.Content.ReadAsStringAsync().Result));
            }
            ///Catch the errors
            catch (Exception ex)
            {
                throw new LightException($"Error on API call Rest. SR: {MakeUri(serviceRoute)} || EX: {ex.Message} || ST:{ex.StackTrace}", ex);
            }
            ///If no Exception, returns the result
        }

        /// <summary>
        /// Methods that gets the object that processes requests for the route.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <returns>Returns the JSON object in text</returns>
        public override JToken Get(string serviceRoute, [Optional] Dictionary<string, string> header)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            /// Create the Request
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                ///Send the authentication token if exisists
                if (!string.IsNullOrEmpty(_token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                if (header != null)
                    foreach (var item in header)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }

                ///Get the Response
                var httpResponseMessage = base.ResilientRequest(MakeUri(serviceRoute), httpClient, WorkBenchServiceHttp.GET).Result;

                return HandleResult(JToken.Parse(httpResponseMessage.Content.ReadAsStringAsync().Result));
            }
            ///Catchs the exception
            catch (Exception ex)
            {
                throw new LightException($"Error on API call Rest. SR: {MakeUri(serviceRoute)} || EX: {ex.Message} || ST:{ex.StackTrace}", ex);
            }
            ///If no Exceptions, returns the result
        }


        /// <summary>
        /// Method Sync that POST the object that processes requests for the route and returns a Typed response.
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

            try
            {
                //request.ContentType = "application/json";

                ///Sends the authorization token if exisists
                if (!string.IsNullOrEmpty(_token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);


                if (headers != null)
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                ///Get the Response
                var response = base.ResilientRequest(MakeUri(serviceRoute), httpClient, WorkBenchServiceHttp.DELETE).Result;

                if (typeof(T).Equals(typeof(HttpResponseMessage)))
                {
                    return (T)Convert.ChangeType(response, typeof(T));
                }

                ///return DomainResponse
                return JToken.Parse(response.Content.ReadAsStringAsync().Result).ToObject<T>();

            }
            catch
            {
                ///return new DomainResponse
                return default(T);
            }
        }

        public override DomainResponse Delete(string serviceRoute, [Optional]Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            try
            {
                //request.ContentType = "application/json";

                ///Sends the authorization token if exisists
                if (!string.IsNullOrEmpty(_token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                if (headers != null)
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                ///Get the Response
                var response = base.ResilientRequest(MakeUri(serviceRoute), httpClient, WorkBenchServiceHttp.DELETE).Result;

                ///return new DomainResponse
                return new DomainResponse()
                {
                    PayLoad = response.Content.ReadAsStringAsync().Result
                };

            }
            catch
            {
                return new DomainResponse();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Send a POST or PUT operation expecting a result from a JSON type.
        /// </summary>
        protected override async Task<JToken> SendAsync(string operation, string serviceRoute, MultipartFormDataContent body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    ///Send the authentication token if exisists
                    if (!String.IsNullOrEmpty(_token))
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);


                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }

                    HttpResponseMessage response;

                    if (operation == "POST")
                        response = await base.ResilientRequest(MakeUri(serviceRoute), client, WorkBenchServiceHttp.POST, body.ConvertToByteArrayContent());
                    else
                        response = await base.ResilientRequest(MakeUri(serviceRoute), client, WorkBenchServiceHttp.PUT, body.ConvertToByteArrayContent());

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        JToken result = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                        return HandleResult(result);
                    }
                }
            }
            ///Catchs the Exception calling the API
            catch (Exception ex)
            {
                throw new LightException($"Error on API call Rest. OP: {operation} || SR: {MakeUri(serviceRoute)} || EX: {ex.Message} || ST:{ex.StackTrace}", ex);
            }
            ///return JToken
            return default(JToken);
        }


        /// <summary>
        /// Send a POST or PUT operation expecting a result from a JSON type.
        /// </summary>
        protected override async Task<T> SendAsync<T>(string operation, string serviceRoute, MultipartFormDataContent body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            try
            {

                ///Send the authentication token if exisists
                if (!String.IsNullOrEmpty(_token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);


                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                HttpResponseMessage response;

                if (operation == "POST")
                    response = await base.ResilientRequest(MakeUri(serviceRoute), httpClient, WorkBenchServiceHttp.POST, body.ConvertToByteArrayContent());
                else
                    response = await base.ResilientRequest(MakeUri(serviceRoute), httpClient, WorkBenchServiceHttp.PUT, body.ConvertToByteArrayContent());

                if (typeof(T).Equals(typeof(HttpResponseMessage)))
                {
                    return (T)Convert.ChangeType(response, typeof(T));
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JToken result = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                    return HandleResult<T>(result);
                }

            }
            ///Catchs the Exception calling the API
            catch (Exception ex)
            {
                throw new LightException($"Error on API call Rest. OP: {operation} || SR: {MakeUri(serviceRoute)} || EX: {ex.Message} || ST:{ex.StackTrace}", ex);
            }
            ///return JToken
            return default(T);
        }

        /// <summary>
        /// Send a POST or PUT operation expecting a result from a JSON type.
        /// </summary>
        protected override async Task<T> SendAsync<T>(string operation, string serviceRoute, FormUrlEncodedContent body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            try
            {

                ///Send the authentication token if exisists
                if (!String.IsNullOrEmpty(_token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);


                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                HttpResponseMessage response;

                if (operation == "POST")
                    response = await base.ResilientRequest(MakeUri(serviceRoute), httpClient, WorkBenchServiceHttp.POST, body.ConvertToByteArrayContent());
                else
                    response = await base.ResilientRequest(MakeUri(serviceRoute), httpClient, WorkBenchServiceHttp.PUT, body.ConvertToByteArrayContent());

                if (typeof(T).Equals(typeof(HttpResponseMessage)))
                {
                    return (T)Convert.ChangeType(response, typeof(T));
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JToken result = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                    return HandleResult<T>(result);
                }

            }
            ///Catchs the Exception calling the API
            catch (Exception ex)
            {
                throw new LightException($"Error on API call Rest. OP: {operation} || SR: {MakeUri(serviceRoute)} || EX: {ex.Message} || ST:{ex.StackTrace}", ex);
            }
            ///return JToken
            return default(T);
        }

        /// <summary>
        /// Handle a JToken response and performs validation on critics and content received
        /// </summary>
        /// <typeparam name="T">Object type expected by user</typeparam>
        /// <param name="response">A JToken object received from service</param>
        /// <returns>Object response as a type expected by user</returns>
        private T HandleResult<T>(JToken response)
        {
            var domainResponse = response.ToObject<DomainResponse>();
            var result = default(T);
            if (domainResponse != null)
            {
                if (_lightDomain != null)
                {
                    this.CritictHandler = _lightDomain.CritictHandler;
                }
                List<Critic> critics = domainResponse.Critics.ToObject<List<Critic>>();
                // If the response contains critics it should be add to critic handler
                if (critics != null && critics.Count > 0)
                {
                    foreach (ICritic critic in critics)
                    {
                        if (this.CritictHandler != null)
                            this.CritictHandler.Critics.Add(critic);
                    }
                }
                /// Parse the object returned from service
                if ((domainResponse.PayLoad != null) && (domainResponse.PayLoad?.Type != JTokenType.Null))
                    result = domainResponse.PayLoad.ToObject<T>();

            }

            return result;
        }

        /// <summary>
        /// Handle a JToken response and performs validation on critics and content received
        /// </summary>
        /// <param name="response">A JToken object received from service</param>
        /// <returns>A JToken extrated from response</returns>
        private JToken HandleResult(JToken response)
        {
            var domainResponse = response.ToObject<DomainResponse>();
            JToken result = null;
            if (domainResponse != null)
            {
                if (_lightDomain != null)
                    CritictHandler = _lightDomain.CritictHandler;

                var critics = domainResponse.Critics.ToObject<List<Critic>>();
                /// If the response contains critics it should be add to critic handler
                if (critics != null && critics.Count > 0)
                {
                    foreach (ICritic critic in critics)
                    {
                        if (this.CritictHandler != null)
                            this.CritictHandler.Critics.Add(critic);
                    }
                }

                /// Parse the object returned from service
                if ((domainResponse.PayLoad != null) && (domainResponse.PayLoad?.Type != JTokenType.Null))
                    result = domainResponse.PayLoad;
            }

            return result;
        }

        /// <summary>
        /// Performas a POST or PUT operation expecting a result from a defined type.
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

            try
            {
                if (body == null)
                {
                    body = new JObject();
                }

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                /// Sends a authorizarion token, if it exists
                if (!string.IsNullOrEmpty(_token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                WorkBenchServiceHttp httpMethod = WorkBenchServiceHttp.POST;
                if (operation == "PUT")
                    httpMethod = WorkBenchServiceHttp.PUT;

                var httpResponse = base.ResilientRequest(this.MakeUri(serviceRoute), httpClient, httpMethod, body.ConvertToByteArrayContent()).Result;
                if (typeof(T).Equals(typeof(HttpResponseMessage)))
                {
                    return (T)Convert.ChangeType(httpResponse, typeof(T));
                }
                JToken result = JToken.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                return HandleResult<T>(result);

            }
            catch (Exception ex)
            {
                string exBody = body != null ? body.ToString() : "null";
                StringBuilder sb = new StringBuilder();
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        sb.AppendLine($"{item.Key} = {item.Value}");
                    }
                }
                else
                {
                    sb.Append("header = vazio");
                }
                throw new LightException($"Error on API call Rest. OP: {operation} || SR: {MakeUri(serviceRoute)} || BD: {exBody} || HD: {sb.ToString()} || EX: {ex.Message} || ST:{ex.StackTrace}", ex);
            }
        }


        /// <summary>
        /// Send a POST or PUT operation expecting a result from a JSON type.
        /// </summary>
        /// <param name="operation"></param>Http
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="headers">pass additional information with the request or the response</param>
        /// <returns></returns>
        private JToken Send(string operation, string serviceRoute, [Optional] JToken body, Dictionary<string, string> headers)
        {
            serviceRoute = serviceRoute.Replace(Environment.NewLine, string.Empty).Replace("\"", "'");

            HttpClient httpClient = new HttpClient();

            try
            {
                if (body == null)
                {
                    body = new JObject();
                }

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                /// Sends a authorizarion token, if it exists
                if (!string.IsNullOrEmpty(_token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                WorkBenchServiceHttp httpMethod = WorkBenchServiceHttp.POST;
                if (operation == "PUT")
                    httpMethod = WorkBenchServiceHttp.PUT;

                var httpResponse = base.ResilientRequest(this.MakeUri(serviceRoute), httpClient, httpMethod, body.ConvertToByteArrayContent()).Result;

                JToken result = JToken.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                return HandleResult(result);

            }
            ///Catchs the Exception calling the API and throw
            catch (Exception ex)
            {
                string exBody = body != null ? body.ToString() : "null";
                StringBuilder sb = new StringBuilder();
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        sb.AppendLine($"{item.Key} = {item.Value}");
                    }
                }
                else
                {
                    sb.Append("header = vazio");
                }
                throw new LightException($"Error on API call Rest. OP: {operation} || SR: {MakeUri(serviceRoute)} || BD: {exBody} || HD: {sb.ToString()} || EX: {ex.Message} || ST:{ex.StackTrace}", ex);
            }
        }
        #endregion

    }
}