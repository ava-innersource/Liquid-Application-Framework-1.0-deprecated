using FluentValidation;
using Liquid.Runtime.Configuration;

namespace Liquid.OnAzure
{
    /// <summary>
    ///  Configuration of the for connect a Service Bus (Queue / Topic).
    /// </summary>
    public class ServiceBusConfiguration : LightConfig<ServiceBusConfiguration>
    {
        /// <summary>
        /// String of connection with the service bus defined on the azure.
        /// </summary>
        public string ConnectionString { get; set; }

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
