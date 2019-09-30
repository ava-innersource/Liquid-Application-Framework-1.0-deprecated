using Liquid.Base;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Liquid.Activation
{
    /// <summary>
    /// Global Context for Microservice
    /// </summary>
    public class LightContext : ILightContext
    {
        public IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor
        /// </summary>
        public LightContext() { }
        /// <summary>
        /// Constructor with HttpContext Accessor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public LightContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// User with Claims
        /// </summary>
        public ClaimsPrincipal User { get; set; }
    }
}
