using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Runtime.Polly
{
    public class Polly : LightPolly
    {
        public override async Task<HttpResponseMessage> ResilientRequest(string url, HttpClient httpClient, WorkBenchServiceHttp http, dynamic pollyconfiguration, object data = null)
        {
            PollyConfiguration pollyConfig = (PollyConfiguration)pollyconfiguration;
            AsyncRetryPolicy<HttpResponseMessage> retryPolicy = null;

            // TODO: Please let's remove this ternary
            retryPolicy = pollyConfig.IsBackOff == true ? Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode.Equals(HttpStatusCode.InternalServerError))
                .Or<WebException>().Or<HttpRequestException>()
                    .WaitAndRetryAsync(pollyConfig.Retry, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (result, timeSpan, retryCount, context) =>
                    {
                        CallbackError<HttpResponseMessage>(result, timeSpan, retryCount);
                    }) :

                    Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode.Equals(HttpStatusCode.InternalServerError)).Or<WebException>().Or<HttpRequestException>()
                    .WaitAndRetryAsync(pollyConfig.Retry, retryAttempt => TimeSpan.FromSeconds(pollyConfig.Wait), (result, timeSpan, retryCount, context) =>
                    {
                        CallbackError<HttpResponseMessage>(result, timeSpan, retryCount);
                    });

           return await retryPolicy.ExecuteAsync(() =>
            {
                switch (http)
                {
                    case WorkBenchServiceHttp.GET:
                        return httpClient.GetAsync(url);
                    case WorkBenchServiceHttp.POST:
                        return httpClient.PostAsync(url, (HttpContent)data);
                    case WorkBenchServiceHttp.PUT:
                        return httpClient.PutAsync(url, (HttpContent)data);
                    case WorkBenchServiceHttp.DELETE:
                        return httpClient.DeleteAsync(url);
                    default:
                        return null;
                }
            });
        }

        private void CallbackError<T>(DelegateResult<T> result, TimeSpan timeSpan, int retryCount)
        {
            Debug.WriteLine($"Request failed with {result.Result}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
        }
    }
}
