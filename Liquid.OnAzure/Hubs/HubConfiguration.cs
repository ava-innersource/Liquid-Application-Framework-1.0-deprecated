using FluentValidation;
using Liquid.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.OnAzure
{
    /// <summary>
    ///  Configuration of the for connect a Hub.
    /// </summary>
    public class HubConfiguration : LightConfig<HubConfiguration>
    {
        /// <summary>
        /// String of connection with the Hub defined on the azure.
        /// </summary>
        public string ConnectionString { get; set; }

        public string StorageConnectionString { get; set; }

        public string StorageContainerName { get; set; }

        /// <summary>
        ///  The method used to properties validation of Configuration.
        /// </summary>
        public override void Validate()
        {
            RuleFor(d => ConnectionString).NotEmpty().WithMessage("ConnectionString settings should not be empty.");
            RuleFor(d => ConnectionString).Matches("Endpoint=sb://").WithMessage("No Endpoint on configuration string has been informed.");
            RuleFor(d => ConnectionString).Matches("SharedAccessKeyName=").WithMessage("No SharedAccessKeyName on configuration string has been informed.");
            RuleFor(d => ConnectionString).Matches("SharedAccessKey=").WithMessage("No SharedAccessKey on configuration string has been informed.");
        }
    }
}
