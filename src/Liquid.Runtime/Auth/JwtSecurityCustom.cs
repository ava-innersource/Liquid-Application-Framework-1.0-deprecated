using Liquid.Runtime.Configuration.Base;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using System.Security.Cryptography;

namespace Liquid.Runtime
{
    public static class JwtSecurityCustom
    {

        /// <summary>
        /// Verify if token was received and return a claims or if token is empty, return a new mock claims
        /// </summary>
        /// <param name="protectedText">token</param>
        /// <returns>ClaimsPrincipal</returns>
        public static ClaimsPrincipal VerifyTokenReceived(string protectedText)
        {
            ClaimsIdentity claims = null;
            ClaimsIdentity mockClaims = null;
            JwtSecurityToken jwt;
			// TODO: This is way too much specialized and must be reworked
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production" &&
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Quality" &&
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Integration")
            {
                mockClaims = new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Email, "amaw@amaw.com.br"),
                        new Claim("emails", "amaw@amaw.com.br"),
                        new Claim(ClaimTypes.Name, "ByPass user"),
                        new  Claim(ClaimTypes.Role, "admin"),
                        new Claim(ClaimTypes.NameIdentifier, "ByPassAuthMiddleware"),
                        new Claim("nonce", Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Surname,"User"),
                        new Claim(ClaimTypes.GivenName,"ByPass")}
               , "ByPassAuth");
            }

            try
            {
                if (!string.IsNullOrEmpty(protectedText))
                {
                    jwt = new JwtSecurityToken(jwtEncodedString: protectedText);
                    claims = new ClaimsIdentity(jwt.Claims, "ByPassAuth");
                }
                else
                {
                    claims = mockClaims;
                }
            }
            catch
            {
                claims = mockClaims;
            }

            if (claims == null)
            {
                return null;
            }
            else
            {
                //Validation PASSED
                return new ClaimsPrincipal(claims);
            }
        }

        /// <summary>
        /// Create a Mocked Token for Test on non Production environments.
        /// </summary>
        /// <param name="email">email</param>
        /// <param name="name">name</param>
        /// <returns>JWT</returns>
        public static string CreateMockedToken(string email, string name)
        {
            ClaimsIdentity mockClaims;
            string tokenString;
            try
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production" &&
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Quality")
                {
                    mockClaims = new ClaimsIdentity(new[] {
                            new Claim(ClaimTypes.Email, email),
                            new Claim("emails", email),
                            new Claim(ClaimTypes.Name, name),
                            new  Claim(ClaimTypes.Role, "admin"),
                            new Claim(ClaimTypes.NameIdentifier, "ByPassAuthMiddleware"),
                            new Claim("nonce", Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.Surname,name),
                            new Claim(ClaimTypes.GivenName,name) }
                    , "ByPassAuth");

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = mockClaims
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    tokenString = tokenHandler.WriteToken(token);
                }
                else
                    throw new SecurityTokenValidationException();
            }
            catch (SecurityTokenValidationException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }

            return tokenString;
        }
        /// <summary>
        /// Create a JWT of Amaw
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="mockClaims"></param>
        /// <returns></returns>
        public static string CreateToken(string email, string name, ClaimsIdentity mockClaims)
        {
            var config = LightConfigurator.Config<AuthConfiguration>("Auth");
            string tokenString;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var audiences = config.Audiencies.Split(',');

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

                var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials
                  (key, SecurityAlgorithms.RsaSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    SigningCredentials = signingCredentials,
                    Issuer = config.Authority,
                    Audience = (audiences != null && audiences.Length > 0) ? audiences[0] : null,
                    Subject = mockClaims,
                    Expires = DateTime.Now.AddMinutes(60),
                    NotBefore = DateTime.Now
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                tokenString = tokenHandler.WriteToken(token);
            }
            catch (SecurityTokenValidationException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }

            return tokenString;
        }

    }
}
