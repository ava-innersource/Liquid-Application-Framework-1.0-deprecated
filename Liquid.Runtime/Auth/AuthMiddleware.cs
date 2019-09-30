using Liquid.Runtime.Configuration.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Liquid.Runtime
{
    /// <summary>
    /// Authentication Extension for startup
    /// </summary>
    public static class AuthenticationExtension
    {
        private static AuthConfiguration config;

        /// <summary>
        /// Enables a mock middleware for a non Production environments
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseByPassAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }

        /// <summary>
        /// Add JWT support on authentication
        /// </summary>
        /// <param name="services"></param>
        public static void AddAuth(this IServiceCollection services, bool hasIdentityServer)
        {
            config = LightConfigurator.Config<AuthConfiguration>("Auth");
            var rsaConfig = LightConfigurator.Config<SigningCredentialsConfig>("SigningCredentials");

            RSAParameters rsaParameters = new RSAParameters
            {
                D = Convert.FromBase64String(rsaConfig.D),
                DP = Convert.FromBase64String(rsaConfig.DP),
                DQ = Convert.FromBase64String(rsaConfig.DQ),
                Exponent = Convert.FromBase64String(rsaConfig.Exponent),
                InverseQ = Convert.FromBase64String(rsaConfig.InverseQ),
                Modulus = Convert.FromBase64String(rsaConfig.Modulus),
                P = Convert.FromBase64String(rsaConfig.P),
                Q = Convert.FromBase64String(rsaConfig.Q)
            };
            SecurityKey key = new RsaSecurityKey(rsaParameters);
            key.KeyId = rsaConfig.KeyId;

            if (!hasIdentityServer)
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwtOpt =>
                {
                    jwtOpt.Authority = config.Authority;
                    jwtOpt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = config.Authority,
                        ValidateAudience = true,
                        ValidAudiences = config.Audiencies.Split(','),
                        IssuerSigningKey = key
                    };
                    jwtOpt.Events = new JwtBearerEvents();
                    jwtOpt.RequireHttpsMetadata = config.RequireHttpsMetadata;
                });
            }

        }
    }

    /// <summary>
    /// Mock Middleware
    /// </summary>
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        public AuthMiddleware(RequestDelegate next)
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
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production" &&
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Quality")
            {
                string token = string.Empty;
                bool tokenMustValidated = false;
                var authHeader = context.Request.Headers["Authorization"];
                if (authHeader != StringValues.Empty)
                {
                    var header = authHeader.FirstOrDefault();
                    if (!string.IsNullOrEmpty(header) && header.Contains("Bearer"))
                    {
                        tokenMustValidated = true;
                    }
                    if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ") && header.Length > "Bearer ".Length)
                    {
                        token = header.Substring("Bearer ".Length);
                        tokenMustValidated = true;
                    }
                }
                 
                if (context.Request.Path.Value.ToLower().Contains("/reseed"))
                {
                    WorkBench.Repository.ResetData(context.Request.Query["files"].ToString());

                    context.Response.StatusCode = 200; // Succes
                    await context.Response.WriteAsync(string.Empty);
                    return;
                }
                if (tokenMustValidated)
                    context.User = JwtSecurityCustom.VerifyTokenReceived(token);
            }

            await _next.Invoke(context);
        }
    }
}
