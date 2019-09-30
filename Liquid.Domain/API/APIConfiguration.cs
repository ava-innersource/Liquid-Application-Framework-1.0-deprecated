using FluentValidation;
using Liquid.Runtime.Polly;
using System;

namespace Liquid.Runtime.Configuration
{
    /// <summary>
    /// Validates the host property from APIWrapper
    /// </summary>
    public class ApiConfiguration : LightConfig<ApiConfiguration>
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Suffix { get; set; }
        public PollyConfiguration Polly { get; set; }
        public Boolean Stub { get; set; }
        /// <summary>
        ///  The method used to validate settings retrieved from LightConfiguration.
        /// </summary>
        public override void Validate()
        { 
            RuleFor(x => x.Host).NotEmpty().WithMessage("The Host property should be informed on API settings");
        }
    }
}