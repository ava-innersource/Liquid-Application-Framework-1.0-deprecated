using FluentValidation;
using Liquid.Runtime.Configuration;
using Microsoft.Extensions.Logging;

namespace Liquid.Runtime
{
    /// <summary>
    /// The Configuration for NLogConfiguration
    /// </summary>
    public class NLoggerConfiguration : LightConfig<NLoggerConfiguration>
    {
        /// <summary>
        /// Enabled
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Enabled
        /// </summary>
        public bool EnabledLogTrafic { get; set; }
        /// <summary>
        /// Path to create files
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// LogLevel to create files
        /// </summary>
        public string LogLevel { get; set; }
        /// <summary>
        /// Layout
        /// </summary>
        public string Layout { get; set; }
        /// <summary>
        /// Telemetry
        /// </summary>
        public bool IntegratedTelemetry { get; set; }
        /// <summary>
        /// The necessary validation to create 
        /// </summary>
        public override void Validate()
        {
            RuleFor(d => FileName).NotEmpty().WithMessage("FileName on File settings should not be empty.");
            RuleFor(d => LogLevel).NotEmpty().WithMessage("LogLevel on File settings should not be empty."); 
            if (string.IsNullOrEmpty(Layout))
            {
                Layout = "${longdate} ${level} ${message}  ${exception}";
            } 
        }
    }
}
