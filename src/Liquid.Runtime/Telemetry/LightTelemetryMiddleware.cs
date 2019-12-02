using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Liquid.Runtime.Telemetry
{
    /// <summary>
    /// Add some methods to configure Telemetry providers.
    /// </summary>
    public static class TelemetryExtensions
    {
        /// <summary>
        /// Enable Application Insights telemetry
        /// </summary>
        /// <param name="services">Collection of services that the telemetry will be added to</param>
        /// <remarks>
        /// Only works if the configuration file contains a section named 'ApplicationInsights'.
        /// Will also add the KubernetesEnricher if the section contains a key EnableKubernetes with 
        /// value 'true'.
        /// </remarks>
        public static void AddTelemetry(IServiceCollection services)
        {
            // TODO: use typed configuration
            var applicationInsightsSection = WorkBench.Configuration.GetSection("ApplicationInsights");

            if (applicationInsightsSection == null) return;

            services.AddApplicationInsightsTelemetry();

            var configs = applicationInsightsSection.GetChildren().ToList();

            var k8sEnabled = configs.FirstOrDefault(p => RelaxedEqual(p.Key.Trim(), "EnableKubernetes"));
            if (k8sEnabled == null) return;

            if (RelaxedEqual(k8sEnabled.Value, "true"))
            {
                services.AddApplicationInsightsKubernetesEnricher();
            }
        }

        /// <summary>
        /// Ativa o middleware customizado AppInsightsTelemetry para capturar todos os eventos das API.
        /// e registrar logg no AppInsights com os detalhes adequados
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseTelemetry(this IApplicationBuilder builder)
        {
            return builder
                .UseMiddleware<TelemetryMiddleware>();
        }

        /// <summary>
        /// Checks if the value of both strings are the same, in a relaxed way
        /// </summary>
        /// <param name="value">The value of the original string</param>
        /// <param name="expected">The value that the string will be compared to</param>
        /// <returns>True if both strings are equal</returns>
        /// <remarks>
        /// This method ignores casing and whitespace to enable a more relaxed comparison.
        /// </remarks>
        private static bool RelaxedEqual(string value, string expected)
        {
            return string.Compare(value.Trim(), expected, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

    /// <summary>
    /// This telemetry don't will use the TelemetryClient because we need centrilaze all messages to AppInsights
    /// </summary>
    public class TelemetryMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Middleware Telemetry
        /// Exception 500
        /// </summary>
        /// <param name="next"></param>
        public TelemetryMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the logic of the middleware.
        /// Intercepet the swagger call
        /// </summary>
        /// <param name="context">given request path</param>
        /// <returns>A Task that completes when the middleware has completed processing.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                ((LightTelemetry)WorkBench.Telemetry).TrackException(e);
                throw;
            }
        }
    }
}
