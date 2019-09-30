using Liquid.Runtime.Configuration;
using System;
using System.Text;
using System.Collections.Generic;
using FluentValidation;

namespace Liquid.OnGoogle
{
    /// <summary>
    /// StackDriver Configurator will dynamically assign the authorization key for writing to StackDriver.
    /// The function caller should pass a configuration file containing the Azure key.
    /// </summary>
    public class GoogleStackDriverConfiguration : LightConfig<GoogleStackDriverConfiguration>
    {
        public string ProjectId { get; set; }

        public string LogId { get; set; }

        public string ServiceName { get; set; }

        public string Version { get; set; }

        public override void Validate()
        {
            RuleFor(d => ProjectId).NotEmpty().WithMessage("ProjectId on GoogleStorage settings should not be empty.");

            RuleFor(d => ServiceName).NotEmpty().WithMessage("SecretAccessKey on GoogleStorage settings should not be empty.");
        }
    }
}
