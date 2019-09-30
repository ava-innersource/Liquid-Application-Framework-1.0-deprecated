using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Base.HealthCheck
{/// <summary>
/// Class with all results for active cartriges and a general status
/// </summary>
    public class LightHealthResult
    {
        public string Status { get; set; }
        /// <summary>
        /// Receive cartridges names and Healthy status
        /// </summary>
        public List<LightHealthCartridgeResult> CartridgesStatus = new List<LightHealthCartridgeResult>();
    }

    public class LightHealthCartridgeResult
    {
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
