using Liquid.Base.Interfaces;
using System;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Interface Logger inheritance to use a liquid framework
    /// </summary> 
    public interface ILightLogger : IWorkBenchHealthCheck
    {
        /// <summary>
        /// LogTrafic
        /// </summary>
        bool EnabledLogTrafic { get; set; }
        /// <summary>
        /// Trace event
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void Trace(string message, object[] args = null);
        /// <summary>
        /// Debug event 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void Debug(string message, object[] args = null);

        /// <summary>
        /// Info event  
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void Info(string message, object[] args = null);

        /// <summary>
        /// Warn event 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void Warn(string message, object[] args = null);

        /// <summary>
        /// Error event  
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void Error(Exception exception, string message, object[] args = null);

        /// <summary>
        /// Fatal event  
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void Fatal(Exception exception, string message, object[] args = null); 
    }
}
