using Liquid.Interfaces;
using Liquid.Runtime.Configuration.Base;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;

namespace Liquid.Runtime
{
    /// <summary>
    /// The NLog class is the lowest level of integration of WorkBench with Azure NLog.
    /// It directly provides a client to send the messages to the cloud.
    /// So it is possible to trace all logs, events, traces, exceptions in an aggregated and easy-to-use form.
    /// </summary>
    public class NLogger : LightLogger, ILightLogger
    {
        /// <summary>
        /// Logger of NLog
        /// </summary>
        private Logger logger;
        /// <summary>
        /// Configuration of NLogger
        /// </summary>
        private NLoggerConfiguration nLoggerConfiguration;
        /// <summary>
        /// Inicialize the cambright
        /// </summary>
        public override void Initialize()
        {
            nLoggerConfiguration = LightConfigurator.Config<NLoggerConfiguration>("NLogger");

            EnabledLogTrafic = nLoggerConfiguration.EnabledLogTrafic;
            isRegistryTelemetry = nLoggerConfiguration.IntegratedTelemetry;

            LoggingConfiguration config = new LoggingConfiguration();
            // Step 2. Create targets
            var consoleTarget = new ColoredConsoleTarget("default")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception}"
            };
            config.AddTarget(consoleTarget);

            var fileTarget = new FileTarget("onFile")
            {
                Layout = nLoggerConfiguration.Layout,
                FileName = nLoggerConfiguration.FileName,
                ArchiveFileName = nLoggerConfiguration.FileName + "-{##}.log",

                FileNameKind = FilePathKind.Relative,
                FileAttributes = Win32FileAttributes.Normal,
                CreateDirs= true, 
                ArchiveAboveSize = 102400,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                MaxArchiveFiles = 100,
                ConcurrentWrites = true,
                KeepFileOpen = true,
                DeleteOldFileOnStartup = true 
            };
            config.AddTarget(fileTarget);

            // Step 3. Define rules
            config.AddRuleForOneLevel(NLog.LogLevel.FromString(nLoggerConfiguration.LogLevel), fileTarget); // only errors to file
            config.AddRuleForAllLevels(consoleTarget); // all to console

            // Step 4. Activate the configuration
            LogManager.Configuration = config;
            logger = LogManager.GetLogger("NLogger");
        }

        /// <summary>
        /// Debug event 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public virtual void Debug(string message, object[] args = null)
        {
            base.Debug(message, args);
            logger.Debug(message, args);
        }
        /// <summary>
        /// Error event  
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public virtual void Error(Exception exception, string message, object[] args = null)
        {
            base.Error(exception, message, args);
            logger.Error(exception, message, args);
        }
        /// <summary>
        /// Fatal event  
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public virtual void Fatal(Exception exception, string message, object[] args = null)
        {
            base.Fatal(exception, message, args);
            logger.Fatal(exception, message, args);
        }
        /// <summary>
        /// Info event  
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public virtual void Info(string message, object[] args = null)
        {
            base.Info(message, args);
            logger.Info(message, args);
        }
        /// <summary>
        /// Trace event
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Trace(string message, object[] args = null)
        {
            base.Trace(message, args);
            logger.Trace(message, args);
        }
        /// <summary>
        /// Warn event 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Warn(string message, object[] args = null)
        {
            base.Warn(message, args);
            logger.Warn(message, args);
        }
        /// <summary>
        /// HealthCheck
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }
        }
    }
}
