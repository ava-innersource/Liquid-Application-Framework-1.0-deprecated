using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Liquid.Runtime.Telemetry
{
    /// <summary>
    /// Cria uma extensão para o middleware
    /// </summary>
    public static class TelemetryExtensions
    {
        /// <summary>
        /// Enable Application Insight Telemetry
        /// </summary>
        /// <param name="services"></param>
        public static void AddTelemetry(IServiceCollection services)
        {
            if (WorkBench.Configuration.GetSection("ApplicationInsights") != null)
            {
                services
                .AddApplicationInsightsTelemetry();
                var configs = WorkBench.Configuration.GetSection("ApplicationInsights").GetChildren().ToList();
                var k8senabled = configs.FirstOrDefault(p => p.Key == "EnableKubernetes");
                if (k8senabled.Value.ToLower() == "true")
                {
                    services.AddApplicationInsightsKubernetesEnricher();
                }
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
