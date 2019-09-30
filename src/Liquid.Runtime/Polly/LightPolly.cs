using Liquid.Base.Interfaces.Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Runtime.Polly
{
    public abstract class LightPolly : ILightPolly
    {

        /// <summary>
        /// 
        /// </summary>
        public void Initialize() { }

        public abstract Task<HttpResponseMessage> ResilientRequest(string url, HttpClient httpClient, WorkBenchServiceHttp http, dynamic pollyconfiguration, object data = null);
    }
}
