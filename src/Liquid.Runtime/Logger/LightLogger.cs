using Liquid.Base;
using Liquid.Interfaces;
using Liquid.Runtime.Telemetry;
using System;

namespace Liquid.Runtime
{
    /// <summary>
    /// Interface Logger inheritance to use a liquid framework
    /// </summary> 
    public abstract class LightLogger : ILightLogger
    {
        /// <summary>
        /// Registry Telemetry
        /// </summary>
        public virtual bool EnabledLogTrafic { get; set; }
        /// <summary>
        /// Registry Telemetry
        /// </summary>
        protected bool isRegistryTelemetry = false;
        /// <summary>
        /// Injected telemetry
        /// </summary>
        protected readonly ILightTelemetry telemetry = Workbench.Instance.Telemetry != null ? (ILightTelemetry)Workbench.Instance.Telemetry.CloneService() : null;
        /// <summary>
        /// Inicialize the cambright
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// Debug event 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public virtual void Debug(string message, object[] args = null)
        {
            if (isRegistryTelemetry)
            {
                telemetry.TrackTrace(message);
            }
        }
        /// <summary>
        /// Error event  
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        public virtual void Error(Exception exception, string message, object[] args = null)
        {
            if (isRegistryTelemetry)
            {
                telemetry.TrackTrace(message);
            }
        }
        /// <summary>
        /// Fatal event  
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public virtual void Fatal(Exception exception, string message, object[] args = null)
        {
            if (isRegistryTelemetry)
            {
                telemetry.TrackTrace(message);
            }
        }
        /// <summary>
        /// Info event  
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public virtual void Info(string message, object[] args = null)
        {
            if (isRegistryTelemetry)
            {
                telemetry.TrackEvent(message);
            }
        }
        /// <summary>
        /// Trace event
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Trace(string message, object[] args = null)
        {
            if (isRegistryTelemetry)
            {
                telemetry.TrackTrace(message);
            }
        }
        /// <summary>
        /// Warn event 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Warn(string message, object[] args = null)
        {
            if (isRegistryTelemetry)
            {
                telemetry.TrackTrace(message);
            }
        }
    }
}
