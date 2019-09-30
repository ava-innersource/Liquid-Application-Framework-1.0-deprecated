using Liquid.Runtime.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Runtime
{
    public static class LiquidApiExtension
    {
        /// <summary>
        /// Adds APIs Conventions to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseOpenApiConventions(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OpenApiMiddleware>();
        }
    }

    /// <summary>
    /// Builds a middleware pipeline after receiving the pipeline from a swagger pipeline
    /// </summary>
    public class OpenApiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISwaggerProvider _swaggerProvider;
        private readonly JsonSerializer _swaggerSerializer;
        private readonly TemplateMatcher _requestMatcher;
        private List<Action<SwaggerDocument, HttpRequest>> PreSerializeFilters { get; set; }

        /// <summary>
        /// Middleware of Open Api
        /// </summary>
        /// <param name="next"></param>
        public OpenApiMiddleware(RequestDelegate next,
            ISwaggerProvider swaggerProvider,
            IOptions<MvcJsonOptions> mvcJsonOptions)
        {
            _next = next;
            _swaggerProvider = swaggerProvider;
            _swaggerSerializer = SwaggerSerializerFactory.Create(mvcJsonOptions);
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse("swagger/{documentName}/swagger.json"), new RouteValueDictionary());
            PreSerializeFilters = new List<Action<SwaggerDocument, HttpRequest>>();
        }

        /// <summary>
        /// Invokes the logic of the middleware.
        /// (Intercept the swagger call)
        /// </summary>
        /// <param name="context">given request path</param>
        /// <returns>A Task that completes when the middleware has completed processing.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (!RequestingSwaggerDocument(context.Request, out string documentName))
            {
                await _next(context);
                return;
            }

            var basePath = context.Request == null || string.IsNullOrEmpty(context.Request.PathBase)
                ? null
                : context.Request.PathBase.ToString();
            SwaggerDocument swagger = null;
            if (_swaggerProvider != null)
                swagger = _swaggerProvider.GetSwagger(documentName, null, basePath);

            // One last opportunity to modify the Swagger Document - this time with request context
            foreach (var filter in PreSerializeFilters)
            {
                filter(swagger, context.Request);
            }

            await RespondWithSwaggerJson(context.Response, swagger);
        }

        /// <summary>
        /// Get a swagger document on http request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="documentName"></param>
        /// <returns></returns>
        private bool RequestingSwaggerDocument(HttpRequest request, out string documentName)
        {
            documentName = null;
            if (request.Method != "GET") return false;

            var routeValues = new RouteValueDictionary();
            if (!_requestMatcher.TryMatch(request.Path, routeValues) || !routeValues.ContainsKey("documentName")) return false;

            documentName = routeValues["documentName"].ToString();
            return true;
        }

        /// <summary>
        /// Send swagger modified
        /// </summary>
        /// <param name="response"></param>
        /// <param name="swagger"></param>
        /// <returns></returns>
        private async Task RespondWithSwaggerJson(HttpResponse response, SwaggerDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                _swaggerSerializer.Serialize(writer, swagger);
                //Get Swagger Document and apply all AMAW conventions for microservice
                string ret = SwaggerConventions.ApllyConventions(jsonBuilder.ToString());
                await response.WriteAsync(ret, new UTF8Encoding(false));
            }
        }
    }
}
