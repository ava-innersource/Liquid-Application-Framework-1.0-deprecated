using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;

namespace Liquid.Domain.API
{
    /// <summary>
    /// This class provide a simple way to consume rest services mocked.  
    /// </summary>
    public class LightApiMock<T> : IDisposable where T : class
    {
        private TestServer _server;
        private IWebHostBuilder _builder;
        public HttpClient Client;

        /// <summary>
        /// Initialize API from fixed hostname and port with TestServer and set suffix and token
        /// </summary> 
        public LightApiMock()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string configPath = $"appsettings.{environment}.json";
            if (!File.Exists(configPath))
            {
                configPath = $"appsettings.json";
            }
            _builder = new WebHostBuilder().UseConfiguration(
                new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configPath).Build()
            ).UseStartup<T>();
            _server = new TestServer(_builder);
            Client = _server.CreateClient();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiName"></param>
        /// <returns></returns>
        public LightApi GetLightApi(string apiName)
        {
            LightApi api = new LightApi(apiName);
            api.SetTestClient(Client);
            return api;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public LightApi GetLightApi(string apiName, string token)
        {
            LightApi api = new LightApi(apiName, token);
            api.SetTestClient(Client);
            return api;
        }

        /// <summary>
        /// Dispose server after use
        /// </summary>
        public void Dispose()
        {
            _server.Dispose();
        } 
    }
}
