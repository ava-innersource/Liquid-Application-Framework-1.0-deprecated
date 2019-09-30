using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Liquid.Base
{
    /// <summary>
    /// Global Context interface for Microservice
    /// </summary>
    public interface ILightContext
    {
        ClaimsPrincipal User { get; set; }
    }
}
