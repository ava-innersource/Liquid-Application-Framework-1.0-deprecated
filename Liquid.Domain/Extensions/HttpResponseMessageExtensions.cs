using Liquid.Base.Domain;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Liquid.Domain
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Convert to LightDomain after response server
        /// </summary>
        /// <param name="response">Http response message</param>
        /// <returns>LightDomain</returns>
        public static async Task<DomainResponse> ConvertToDomainAsync(this HttpResponseMessage response)
        {
            var value = await response?.Content?.ReadAsStringAsync();
            DomainResponse result = default(DomainResponse);
            result = JsonConvert.DeserializeObject<DomainResponse>(value);
            return result;
        }
    }
}
