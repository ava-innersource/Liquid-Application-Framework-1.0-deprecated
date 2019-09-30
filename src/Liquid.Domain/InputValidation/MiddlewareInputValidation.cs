using Liquid.Domain.Base;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Liquid.Domain
{   /// <summary>
    /// Class responsible for invoking the next component in the pipeline
    /// </summary>
    public class MiddlewareInputValidation
    {
        private readonly RequestDelegate _next;

        public MiddlewareInputValidation(RequestDelegate next)
        {
            _next = next;
        }
        /// <summary>
        /// Invokes the logic of the middleware.
        /// (Intercepet the swagger call)
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>A Task that completes when the middleware has completed processing.</returns>
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                /// Calls the next delegate/middleware in the pipeline
                await _next(httpContext);
            }
            /// If the Invoke method catchs any exception
            /// (it catches any exceptions that occur in later calls)
            /// it will throw a status code 400 Bad Request in Json
            catch (InvalidInputException ex)
            {
                //httpContext.Response.Clear();                 
                httpContext.Response.StatusCode = 400; //Bad Request  
                httpContext.Response.ContentType = "application/json";
                string jsonString = (new { critics = ex.InputErrors }).ToStringCamelCase();
                await httpContext.Response.WriteAsync(jsonString);
            }
        }
    }

}
