using Liquid.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Base.Interfaces.Polly
{
    public interface ILightPolly : IWorkBenchService
    {
        Task<HttpResponseMessage> ResilientRequest(string url, HttpClient httpClient, WorkBenchServiceHttp http, dynamic pollyconfiguration , object data = null);
    }
}
