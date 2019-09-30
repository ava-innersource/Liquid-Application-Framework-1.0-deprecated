using System;
using System.Collections.Generic;
using System.Text;
using Liquid.Runtime.Configuration.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Liquid.Runtime
{
    public static class Secrets
    {
        /// <summary>
        /// Use CrossOrigin for Liquid Microservice.
        /// </summary>
        /// <param name="configuration"></param>
        public static IConfiguration UseSecrets(this IConfiguration configuration)
        {
            SecretsConfiguration config = LightConfigurator.Config<SecretsConfiguration>("Secrets");
            
            if (config.Enable)
            {
                if (config.Module.ToUpperInvariant() == "K8S_SECRETS")
                {
                    var builder = new ConfigurationBuilder()
                    .AddConfiguration(configuration)
                    .SetBasePath(configuration.GetValue<string>(WebHostDefaults.ContentRootKey))
                    .AddJsonFile($"secrets/appsettings.{config.Name.ToLower()}.k8ssecrets.json", optional: true, reloadOnChange: true);
                    
                    configuration = (IConfiguration)builder.Build();
                }

            }
            return configuration;
           
        }

    }
}
