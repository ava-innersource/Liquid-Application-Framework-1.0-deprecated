using Liquid.Domain;
using Liquid.Runtime;
using Liquid.Runtime.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using System;
using Microsoft.AspNetCore.Hosting;
using Liquid.Activation;

namespace Liquid.Middleware
{
    public static class UseBase
    {
        /// <summary>
        /// Activates the WorkBench and the middleware of its services previously initiated.
        /// </summary>
        /// <param name="builder">The builder of the core application</param>
        /// <returns>The builder of the core application</returns>
        public static IServiceCollection AddWorkBench(this IServiceCollection service, IConfiguration Configuration, bool hasIdentityServer = false)
        {
            WorkBench.Configuration = Configuration;
            //Add Secrets options
            WorkBench.Configuration = Configuration.UseSecrets();

            service.AddHttpContextAccessor();

            //Inject Swagger (Open API specification) and API Versioning
            WebApiVersion.AddApiVersion(service);
            Swagger.AddSwagger(service);

            //Inject JWT pattern and security
            AuthenticationExtension.AddAuth(service, hasIdentityServer);

            TelemetryExtensions.AddTelemetry(service);

            return service;
        }

        /// <summary>
        /// Activates the WorkBench and the middleware of its services previously initiatedB.
        /// </summary>
        /// <param name="builder">The builder of the core application</param>
        /// <returns>The builder of the core application</returns>
        public static IApplicationBuilder UseWorkBench(this IApplicationBuilder builder)
        {
            //Inject telemetry middleware according to ILightTelemetry type informed in WorkBench.UseTelemetry<T>(), being it either AMAW's or custom. 
            Object telemetryService = WorkBench.GetRegisteredService(WorkBenchServiceType.Telemetry);
           
            //If a ILightTelemetry type was injected then injects its related middleware
            if (telemetryService != null)
            {
                builder.UseTelemetry();
            }

            // Inject Swagger for all microservices
            builder.UseOpenApiSwagger();
            builder.UseOpenApiConventions();

            //Inject ByPass Auth and MVC Authentication
            builder.UseByPassAuth();
            builder.UseAuthentication();

            //Inject to UseCors on the WorkBench
            builder.UseCrossOrigin();

            // Enables Json localization
            builder.AddLocalization();

            /////Adicionando o serviço de Health Check
            builder.UseHealthCheck();

            // Inject Model Specification middleware
            builder.UseModelSpecification(); 

            //Inject Polly framework, to Resilience.
            WorkBench.Polly = WorkBench.Polly ?? new Runtime.Polly.Polly();

            //Always injects the InputValidation middleware
            return builder.UseMiddleware<MiddlewareInputValidation>();
        }
    }
}
