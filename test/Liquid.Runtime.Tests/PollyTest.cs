using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Liquid.Runtime.Configuration;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Polly;
using Xunit;



namespace Liquid.Runtime.Tests
{
    public class PollyTest
    {
        private readonly LightPolly _sut;
        private string _url;
        private HttpClient _httpClient;
        private WorkBenchServiceHttp _http;
        private PollyConfiguration _pollyConfiguration;



        public PollyTest()
        {
            _sut = new Polly.Polly();
            _httpClient = new HttpClient();
            _http = WorkBenchServiceHttp.GET;
        }


        [Fact]
        public async Task ShouldThrowHttpRequestExceptionOnInvalidUrlAfterRetriedConfiguredAttempts()
        {
            //Arrange
            _pollyConfiguration = new PollyConfiguration { IsBackOff = true, Retry = 3, Wait = 5 };
            _url = "https://www.aipdhapsidh.com";
            HttpResponseMessage result;

            //Action/Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => result = await _sut.ResilientRequest(_url, _httpClient, _http, _pollyConfiguration).ConfigureAwait(true));
        }


        [Fact]
        public async Task ShouldUseDefaultPollyConfigAsRetryAttemptWhenIsBackOff()
        {
            //Arrange
            _pollyConfiguration = new PollyConfiguration { IsBackOff = true, Retry = 3, Wait = 5 };
            _url = "https://www.google.com";

            //Action
            using (HttpResponseMessage result = await _sut.ResilientRequest(_url, _httpClient, _http, _pollyConfiguration).ConfigureAwait(true))
            {
                //Assert
                Assert.True(result.IsSuccessStatusCode);
            }
        }


        [Fact]
        public async Task ShouldUseDefaultPollyConfigAsRetryAttemptWhenIsBackOffFalse()
        {
            //Arrange
            _pollyConfiguration = new PollyConfiguration { IsBackOff = false, Retry = 3, Wait = 5 };
            _url = "https://www.google.com";
            //Action
            using (HttpResponseMessage result = await _sut.ResilientRequest(_url, _httpClient, _http, _pollyConfiguration).ConfigureAwait(true))
            {
                //Assert
                Assert.True(result.IsSuccessStatusCode);
            }
        }


    }
}