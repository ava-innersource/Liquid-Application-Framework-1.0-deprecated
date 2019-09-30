using Liquid.Runtime.Configuration.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Liquid.Runtime
{
    /// <summary>
    /// Include support of Open API based on swagger, that processing data included on Configuration file.
    /// </summary>
    public static class Swagger
    {
        ///Model of Swagger Configuration details
        private static SwaggerConfiguration config;

        public class RemoveVerbsFilter : IDocumentFilter
        {
            public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
            {
                swaggerDoc.BasePath = config.BasePath;
                swaggerDoc.Host = config.Host;
                swaggerDoc.Schemes = config.Schemes;

                string[] list = config.ExcludingSwaggerList;
                if (list != null && list.Length != 0)
                {
                    foreach (string x in list)
                    {
                        if (!string.IsNullOrEmpty(x))
                        {
                            swaggerDoc.Definitions.Remove(x);
                            foreach (var group in context.ApiDescriptions)
                            {
                                var descriptions =
                                    group.ParameterDescriptions.Where(item =>
                                        item.Name.Equals(x) || item.Name.EndsWith($".{x}"))
                                        .ToList();

                                foreach (var param in descriptions)
                                {
                                    group.ParameterDescriptions.Remove(param);
                                }

                                var pathItem = default(PathItem);
                                if (swaggerDoc.Paths.TryGetValue($"/{group.RelativePath}", out pathItem))
                                {
                                    var pathParameter = default(IList<IParameter>);
                                    switch (group.HttpMethod)
                                    {

                                        case "GET":
                                            pathParameter = pathItem.Get.Parameters;
                                            break;
                                        case "POST":
                                            pathParameter = pathItem.Post.Parameters;
                                            break;
                                        case "PUT":
                                            pathParameter = pathItem.Put.Parameters;
                                            break;
                                        case "DELETE":
                                            pathParameter = pathItem.Delete.Parameters;
                                            break;
                                        default:
                                            break;

                                    }

                                    if (pathParameter != null)
                                    {
                                        var parameters =
                                              pathParameter
                                                  .Where(i => i.Name.Equals(x) || i.Name.EndsWith($".{x}"))
                                                  .ToList();

                                        foreach (var param in parameters)
                                        {
                                            pathParameter.Remove(param);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add Swagger support for Microservice.
        /// </summary>
        /// <param name="service"></param>
        public static void AddSwagger(this IServiceCollection services)
        {
            config = LightConfigurator.Config<SwaggerConfiguration>("Swagger");

            ///Fill all versions declareted
            services.AddSwaggerGen(c =>
            {
                foreach (var version in config.Versions)
                {
                    c.SwaggerDoc(version.Name, new Info
                    {
                        Version = version.Name,
                        Title = version.Info.Title,
                        Description = version.Info.Description,
                        TermsOfService = version.Info.TermsOfService,
                        Contact = new Contact { Name = version.Info.Contact.Name, Email = version.Info.Contact.Email, Url = version.Info.Contact.Url },
                        License = new License { Name = version.Info.License.Name, Url = version.Info.License.Url }
                    });
                }
                c.IgnoreObsoleteActions();

                ///Set the comments path for the swagger json and ui.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var appName = PlatformServices.Default.Application.ApplicationName;
                var xmlPath = Path.Combine(basePath, $"{appName}.xml");
                c.IncludeXmlComments(xmlPath);

                c.SchemaFilter<SwaggerIgnoreFilter>();

                c.DocumentFilter<RemoveVerbsFilter>();

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer",new string[]{}}
                });
            });
        }

        /// <summary>
        /// Use Swagger for Liquid Microservice.
        /// </summary>
        /// <param name="service"></param>
        public static IApplicationBuilder UseOpenApiSwagger(this IApplicationBuilder builder)
        {
            config = LightConfigurator.Config<SwaggerConfiguration>("Swagger");

            builder.UseSwaggerUI(c =>
            {
                ///Fill all versions declareted
                foreach (var version in config.Versions)
                {
                    c.SwaggerEndpoint($"/swagger/{version.Name}/swagger.json", $"{version.Name} Docs");
                }
            });

            return builder;
        }
    }
}
