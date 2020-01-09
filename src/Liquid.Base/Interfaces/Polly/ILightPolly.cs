// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;

namespace Liquid.Base.Interfaces.Polly
{
    public interface ILightPolly : IWorkbenchService
    {
        Task<HttpResponseMessage> ResilientRequest(string url, HttpClient httpClient, WorkBenchServiceHttp http, dynamic pollyconfiguration , object data = null);
    }
}
