using Liquid.Runtime.Configuration.Base;
using Microsoft.AspNetCore.Builder;

namespace Liquid.Runtime
{
    /// <summary>
    /// Include support of Cors, that processing data included on Configuration file.
    /// </summary>
    public static class Cors
    {
        /// <summary>
        /// Use CrossOrigin for Liquid Microservice.
        /// </summary>
        /// <param name="builder"></param>
        public static IApplicationBuilder UseCrossOrigin(this IApplicationBuilder builder)
        {
            CorsConfiguration config = LightConfigurator.Config<CorsConfiguration>("Cors");

            string[] origins = { "" };
            string[] headers = { "" };
            string[] methods = { "" };
            // Always there is "," separating on the string of configuration 
            //unless used just a domain to configuration
            if (config.Origins.Contains(","))
            {
                origins = config.Origins.Split(',');
            }
            else
            {
                origins[0] = config.Origins;
            }
            // Always there is "," separating on the string of configuration 
            //unless used just a domain to configuration
            if (config.Headers.Contains(","))
            {
                headers = config.Headers.Split(',');
            }
            else
            {
                headers[0] = config.Headers;
            }
            // Always there is "," separating on the string of configuration 
            //unless used just a domain to configuration
            if (config.Methods.Contains(","))
            {
                methods = config.Methods.Split(',');
            }
            else
            {
                methods[0] = config.Methods;
            }

            builder.UseCors(b =>
            {
                b.WithOrigins(origins);
                b.WithHeaders(headers);
                b.WithMethods(methods);
            });

            return builder;
        }
    }
}
