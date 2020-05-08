using System.Net.Http;
using System.Threading.Tasks;
using Liquid.Runtime.Polly;
using Xunit;

namespace Liquid.Runtime.Tests
{
    public class PollyTests
    {
        private readonly LightPolly _sut;
        private HttpClient _httpClient;
        private WorkBenchServiceHttp _http;

        public PollyTests()
        {
            _sut = new Polly.Polly();
            _httpClient = new HttpClient();
            _http = WorkBenchServiceHttp.GET;
        }

        [Fact]
        public async Task ResilientRequestWhenInvalidUrlAfterRetriedConfiguredAttempsThrowHttpRequest()
        {
            //Arrange
            PollyConfiguration pollyConfiguration = new PollyConfiguration { IsBackOff = true, Retry = 3, Wait = 5 };
            var url = "https://www.aipdhapsidh.com";
            //HttpResponseMessage result;

            //Action/Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await _sut.ResilientRequest(url, _httpClient, _http, pollyConfiguration).ConfigureAwait(true));
        }

        [Fact]
        public async Task ResilientRequestShouldUseMathPowAsRetryAttemptWhenIsBackOffTrue()
        {
            //Arrange
            PollyConfiguration pollyConfiguration = new PollyConfiguration { IsBackOff = true, Retry = 3, Wait = 5 };
            var url = "https://www.google.com";

            //Action
            using (HttpResponseMessage result = await _sut.ResilientRequest(url, _httpClient, _http, pollyConfiguration).ConfigureAwait(true))
            {
                //Assert
                Assert.True(result.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task ResilientRequestShouldUseDefaultPollyConfigAsRetryAttemptWhenIsBackOffFalse()
        {
            //Arrange
            PollyConfiguration pollyConfiguration = new PollyConfiguration { IsBackOff = false, Retry = 3, Wait = 5 };
            var url = "https://www.google.com";
            //Action
            using (HttpResponseMessage result = await _sut.ResilientRequest(url, _httpClient, _http, pollyConfiguration).ConfigureAwait(false))
            {
                //Assert
                Assert.True(result.IsSuccessStatusCode);
            }
        }
    }
}