using Liquid.Base;
using Liquid.Base.Domain;
using Liquid.Base.Interfaces.Polly;
using Liquid.Runtime;
using Liquid.Runtime.Configuration;
using Liquid.Runtime.Configuration.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Liquid.Domain.API
{
    /// <summary>
    /// This class is a wrapper around HttpWebRequest in APIWrapper
    /// It simplifies the process by abstracting the details of the HttpWebRequest 
    /// </summary>
    public abstract class AbstractApiWrapper
    {
        protected string _endpoint = string.Empty;
        protected string _suffix = string.Empty;
        protected string _token = string.Empty;
        protected string _apiname = string.Empty;
        protected Boolean _stub = false;
        protected LightDomain _lightDomain;

        private ILightPolly Polly
        {
            get
            {
                return WorkBench.Polly;
            }

        }

        /// <summary>
        /// This method authenticates and authorizes each requests by hostname, port and token
        /// </summary>
        /// <param name="hostName">host that serves the API</param>
        /// <param name="port">endpoint of connection</param>
        /// <param name="token">token authentication</param>
        protected AbstractApiWrapper(string hostName, int port, string suffix, string token, LightDomain lightDomain, Boolean stub = false)
        {
            _token = token;
            _endpoint = $"{hostName}{((port > 0) ? ":" + port.ToString() + "/" : "/")}{((!string.IsNullOrEmpty(suffix)) ? suffix + "/" : "")}";
            // Defines apiname for use of stubs according to hostname
            _apiname = hostName.Replace("http://", "");
            _lightDomain = lightDomain;
            _suffix = suffix;
            _stub = stub;
        }

        /// <summary>
        ///  This method authenticates and authorizes each requests by apiName and Token
        /// </summary>
        /// <param name="apiName">name of the API</param>
        /// <param name="token">token authentication</param>
        protected AbstractApiWrapper(string apiName, string token)
        {
            _apiname = apiName;
            if (token != null)
                _token = token;

            /// verify the section configuration through API configuration by api name
            /// to build the ENDPOINT
            var config = LightConfigurator.Config<ApiConfiguration>(apiName);
            if (config != null)
            {
                string host = config.Host;
                int port = config.Port ?? -1;
                string suffix = config.Suffix ?? string.Empty;
                _suffix = suffix;
                _endpoint = $"{host}{((port > 0) ? ":" + port.ToString() + "/" : "/")}{((!string.IsNullOrEmpty(suffix)) ? suffix + "/" : "")}";
                _stub = config.Stub;
            }
        }

        /// <summary>
        ///   Method that asynchronously gets the object that processes requests for the route.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="header"></param>
        /// <returns>Returns JSON data to the APIWrapping </returns>
        public async Task<JToken> GetAsync(string serviceRoute, [Optional] Dictionary<string, string> header)
        {
            return await Task.Run(() => this.Get(serviceRoute, header));
        }

        /// <summary>
        ///   Method that asynchronously GETs the object that processes requests for the route.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="header"></param>
        /// <returns>Returns JSON data to the APIWrapping </returns>
        public async Task<JToken> PutAsync(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> headers)
        {
            return await Task.Run(() => this.Put(serviceRoute, body, headers));
        }

        public async Task<JToken> PostAsync(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> headers)
        {
            return await Task.Run(() => this.Post(serviceRoute, body, headers));
        }

        public async Task<JToken> PutAsync(string serviceRoute, MultipartFormDataContent body, [Optional]Dictionary<string, string> headers)
        {
            return await SendAsync("PUT", serviceRoute, body, headers);
        }

        public async Task<JToken> PostAsync(string serviceRoute, MultipartFormDataContent body, [Optional]Dictionary<string, string> headers)
        {
            return await SendAsync("POST", serviceRoute, body, headers);
        }

        public async Task<DomainResponse> DeleteAsync(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> headers)
        {
            return await Task.Run(() => this.Delete(serviceRoute, headers));
        }

        protected abstract Task<JToken> SendAsync(string operation, string serviceRoute, MultipartFormDataContent body, Dictionary<string, string> headers);

        public async Task<T> GetAsync<T>(string serviceRoute, [Optional] Dictionary<string, string> header)
        {
            return await Task.Run(() => this.Get<T>(serviceRoute, header));
        }

        public async Task<T> PutAsync<T>(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> headers)
        {
            return await Task.Run(() => this.Put<T>(serviceRoute, body, headers));
        }

        public async Task<T> PostAsync<T>(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> headers)
        {
            return await Task.Run(() => this.Post<T>(serviceRoute, body, headers));
        }

        public async Task<T> PutAsync<T>(string serviceRoute, MultipartFormDataContent body, [Optional]Dictionary<string, string> headers)
        {
            return await SendAsync<T>("PUT", serviceRoute, body, headers);
        }

        public async Task<T> PostAsync<T>(string serviceRoute, MultipartFormDataContent body, [Optional]Dictionary<string, string> headers)
        {
            return await SendAsync<T>("POST", serviceRoute, body, headers);
        }
        protected abstract Task<T> SendAsync<T>(string operation, string serviceRoute, MultipartFormDataContent body, Dictionary<string, string> headers);

        public async Task<T> DeleteAsync<T>(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> headers)
        {
            return await Task.Run(() => this.Delete<T>(serviceRoute, headers));
        }

        public async Task<T> PutAsync<T>(string serviceRoute, FormUrlEncodedContent body, [Optional]Dictionary<string, string> headers)
        {
            return await SendAsync<T>("PUT", serviceRoute, body, headers);
        }
        public async Task<T> PostAsync<T>(string serviceRoute, FormUrlEncodedContent body, [Optional]Dictionary<string, string> headers)
        {
            return await SendAsync<T>("POST", serviceRoute, body, headers);
        }
        protected abstract Task<T> SendAsync<T>(string operation, string serviceRoute, FormUrlEncodedContent body, Dictionary<string, string> headers);



        /// <summary>
        ///  Methods Sync that GETs the object that processes requests for the route.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="header"></param>
        /// <returns>Returns object data to the APIWrapping</returns>
        public abstract T Get<T>(string serviceRoute, [Optional] Dictionary<string, string> header);

        /// <summary>
        /// Method Sync that GETs the object that processes requests for the route.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <returns>Returns JSON data to the APIWrapping</returns>
        public abstract JToken Get(string serviceRoute, [Optional] Dictionary<string, string> header);

        /// <summary>
        /// Method Sync that POST the object that processes requests for the route and returns a Typed response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="header"></param>
        /// <returns></returns>
        public abstract T Post<T>(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> header);

        /// <summary>
        /// Method Sync that POST the object that processes requests for the route and returns the service response as a JToken.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="header"></param>
        /// <returns></returns>
        public abstract JToken Post(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> header);

        /// <summary>
        /// Method Sync that PUT the object that processes requests for the route and returns a Typed response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="header"></param>
        /// <returns></returns>
        public abstract T Put<T>(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> header);

        /// <summary>
        /// Method Sync that PUT the object that processes requests for the route and returns the service response as a JToken.
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <param name="body">all the information enclosed in the message body in the JSON format</param>
        /// <param name="header"></param>
        /// <returns></returns>
        public abstract JToken Put(string serviceRoute, [Optional] JToken body, [Optional]Dictionary<string, string> header);

        public abstract T Delete<T>(string serviceRoute, [Optional]Dictionary<string, string> headers);

        public abstract DomainResponse Delete(string serviceRoute, [Optional]Dictionary<string, string> headers);



        /// <summary>
        /// Constructs the serviceRoute to the APIWrapper
        /// </summary>
        /// <param name="serviceRoute">specified route prefix to connect to the service</param>
        /// <returns>String ENDPOINT</returns>
        protected string MakeUri(string serviceRoute)
        {
            ///Removes any duplicate slashes
            if (serviceRoute.IndexOf("//") >= 0)
                serviceRoute = serviceRoute.Replace("//", "/");

            ///Removes the first bar, if it exists
            if (serviceRoute.StartsWith("/"))
                serviceRoute = serviceRoute.Substring(1);

            return _endpoint + serviceRoute;
        }

        /// <summary>
        /// Resilient request using Polly
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="httpClient"></param>
        /// <param name="http"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal async Task<HttpResponseMessage> ResilientRequest(string URL, HttpClient httpClient, WorkBenchServiceHttp http, ByteArrayContent data = null)
        {
            if (_stub)
            {
                var config = new MockData().GetMockData<StubAPIConfiguration>("stubAPIData");
                if (config != null)
                {
                    HostStubAPIConfiguration host = config.Hosts.Where(x => x.Name == _apiname).FirstOrDefault();
                    if (host != null)
                    {
                        MethodStubAPIConfiguration method = host.Methods.Where(x => x.WorkBenchServiceHttp == http
                                                                              && Regex.Matches(URL, x.Route, RegexOptions.IgnoreCase).FirstOrDefault() != null
                        ).FirstOrDefault();

                        string jsonString = (new { PayLoad = method.Response, critics = new List<Critic>() }).ToStringCamelCase();
                        return await Task.FromResult(new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(jsonString, System.Text.Encoding.UTF8),

                        });
                    }
                    else
                    {
                        throw new LightException($"Workbench cannot made the request for: {URL} by STUB, there isn't host for request.");
                    }
                }
                else
                {
                    throw new LightException($"Workbench cannot made the request for: {URL} by STUB, check the configuration file.");
                }
            }
            else
            {
                if (this.Polly != null)
                {
                    return await this.Polly.ResilientRequest(URL, httpClient, http, LightConfigurator.Config<ApiConfiguration>(this._apiname).Polly, data);
                }
                else
                {
                    switch (http)
                    {
                        case WorkBenchServiceHttp.GET:
                            return await httpClient.GetAsync(URL);
                        case WorkBenchServiceHttp.POST:
                            return await httpClient.PostAsync(URL, data);
                        case WorkBenchServiceHttp.PUT:
                            return await httpClient.PutAsync(URL, data);
                        case WorkBenchServiceHttp.DELETE:
                            return await httpClient.DeleteAsync(URL);
                        default:
                            throw new LightException($"Workbench cannot made the request for: {URL}");
                    }
                }
            }
        }
    }
}
