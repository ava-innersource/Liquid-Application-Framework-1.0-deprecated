using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Base.Interfaces
{
    public interface IWorkBenchHealthCheck : IWorkBenchService
    {
        /// <summary>
        /// Interface created to force cartridges to implement the method for it's HealthCheck
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        LightHealth.HealthCheck HealthCheck(string serviceKey, string value);
    }
}
