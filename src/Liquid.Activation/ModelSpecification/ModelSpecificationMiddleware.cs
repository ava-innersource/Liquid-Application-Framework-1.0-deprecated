using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;

namespace Liquid.Activation
{
    public static class ModelSpecificationExtension
    {
		public static string PathSwagger = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, $"{PlatformServices.Default.Application.ApplicationName}.xml");
		public static JObject JObject;
        /// <summary>
        /// Enables a Model Specification middleware
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseModelSpecification(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ModelSpecificationMiddleware>();
        }
    }

    /// <summary>
    /// Mode Specification Middleware
    /// </summary>
    public class ModelSpecificationMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        public ModelSpecificationMiddleware(RequestDelegate next)
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
            if (context.Request.Path.Value.ToLower().Contains("/modelspec"))
            {		
				if (ModelSpecificationExtension.JObject == null)
				{
					ModelSpecificationExtension.JObject = new ModelSpecification().GetModelSpecification();
				}

                context.Response.StatusCode = 200; // Success
                var jsonFile = JsonConvert.SerializeObject(ModelSpecificationExtension.JObject);

                await context.Response.WriteAsync(jsonFile);
                return;
            }

            await _next.Invoke(context);
        }
    }
}
