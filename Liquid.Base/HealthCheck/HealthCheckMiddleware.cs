using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Liquid.Base.HealthCheck;

namespace Liquid.Runtime
{
    public static class HealthCheckExtension
    {
        /// <summary>
        /// Enables a mock middleware for a non Production environments
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HealthCheckMiddleware>();
        }
    }

    /// <summary>
    /// Mock Middleware
    /// </summary>
    public class HealthCheckMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        public HealthCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Middleware invoke process
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.ToLower().Contains("/health"))
            {
                LightHealthResult healthResult = new LightHealthResult();
                healthResult.Status = LightHealth.HealthCheck.Healthy.ToString();
                LightHealth.CheckHealth(healthResult);
                foreach (var key in healthResult.CartridgesStatus)
                {
                    if (key.Status == LightHealth.HealthCheck.Unhealthy.ToString())
                    {
                        healthResult.Status = LightHealth.HealthCheck.Unhealthy.ToString();
                        context.Response.StatusCode = 503; // ServiceUnavailable
                    }
                    else
                    {
                        context.Response.StatusCode = 200; // Success
                    }
                }
                var jsonFile = JsonConvert.SerializeObject(healthResult);
                await context.Response.WriteAsync(jsonFile);
                return;
            }

            await _next.Invoke(context);
        }
    }
}
