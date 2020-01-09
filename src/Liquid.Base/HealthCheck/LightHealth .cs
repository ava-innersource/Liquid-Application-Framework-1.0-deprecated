using System;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using System.Net.Http.Headers;
using System.Web;
using Liquid.Interfaces;
using System.Collections.Generic;
using Liquid.Base.Interfaces;
using Liquid.Base.HealthCheck;

namespace Liquid
{
   
    /// <summary>
    /// LightCheck is reponsable to run health check methods into AMAW cartridges
    /// </summary>
    public class LightHealth
    {
        public static Dictionary<string, HealthCheck> CartridgesStatus { get; private set; }

        /// <summary>
        /// Enum used as return status for cartridges health check
        /// </summary>
        public enum HealthCheck { Healthy, Unhealthy };        
        
        /// <summary>
        /// Start getting active cartridges at workbench
        /// </summary>
        /// <returns></returns>
        public static void CheckHealth(LightHealthResult lightHealthResult)
        {
            CheckActiveServices(lightHealthResult);
        }
        
        /// <summary>
        /// Method that calls the Cartridge Health Check method.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static HealthCheck CheckUp(WorkbenchServiceType serviceType, string value)
        {
            IWorkbenchHealthCheck workBenchHealCheck = GetService<IWorkbenchHealthCheck>(serviceType);
            string serviceKey = serviceType.ToString();
            var checkup = workBenchHealCheck.HealthCheck(serviceKey, value);
            return checkup;
        }

        /// <summary>
        /// Check active services, calls the HealthCheck for each active cartridges and return the Dictionary for response
        /// </summary>
        /// <returns></returns>
        private static void CheckActiveServices(LightHealthResult lightHealthResult)
        {
            foreach (var keys in Workbench.Instance._singletonCache.Keys)
            {
                LightHealthCartridgeResult cartridgeResult = new LightHealthCartridgeResult();
                cartridgeResult.Name = keys.ToString();
                cartridgeResult.Status = CheckUp(keys, Workbench.Instance._singletonCache[keys].ToString()).ToString();
                lightHealthResult.CartridgesStatus.Add(cartridgeResult);
            }
        }
        
        /// <summary>
        /// Get active cartridges from exposed _singletonCache property from Workbench
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="singletonType"></param>
        /// <param name="mandatoryParam"></param>
        /// <returns></returns>
        internal static T GetService<T>(WorkbenchServiceType singletonType, Boolean mandatoryParam = true)
        {
            IWorkbenchService service;
            if (!Workbench.Instance._singletonCache.TryGetValue(singletonType, out service))
            {
                if (mandatoryParam)
                    throw new ArgumentException($"No Workbench service of type '{singletonType.ToString()}' was injected on Startup.");
            }

            return (T)service;
        }
    }
}
