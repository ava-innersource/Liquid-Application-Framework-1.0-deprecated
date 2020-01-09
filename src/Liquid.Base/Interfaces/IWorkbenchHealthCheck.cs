// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Liquid.Base.Interfaces
{
    /// <summary>
    /// Interface created to force cartridges to implement the method for it's HealthCheck
    /// </summary>
    public interface IWorkbenchHealthCheck : IWorkbenchService
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
