using Liquid.Base;
using Liquid.Domain;
using Liquid.Interfaces;
using Liquid.Runtime;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Liquid.Activation
{
    /// <summary>
    /// Class created to apply a message inheritance to use a liquid framework
    /// </summary> 
    public abstract class LightMessage<T> : LightViewModel<T>, ILightMessage where T : LightMessage<T>, ILightMessage, new()
    {
        private ILightContext _context;

        [JsonIgnore]
        public ILightContext Context
        {
            get
            { 
                if (_context == null)
                {
                    CheckContext(TokenJwt);
                }
                return _context;
            }
            set
            {
                _context = value;
            }
        }

        public string TokenJwt
        {
            get
            {
                CheckContext(null);
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor();
                tokenDescriptor.Subject = (ClaimsIdentity)Context?.User?.Identity;
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                return tokenString;
            }
            set
            {
                CheckContext(value);
            }
        }

        /// <summary>
        /// Verify if context was received otherwise create the context with mock
        /// </summary>
        /// <param name="token">Token</param> 
        private void CheckContext(string token)
        {
            if (_context == null)
            {
                _context = new LightContext();
            }
            if (_context.User == null)
            {
                _context.User = JwtSecurityCustom.VerifyTokenReceived(token);
            }
        }
         
    }
}