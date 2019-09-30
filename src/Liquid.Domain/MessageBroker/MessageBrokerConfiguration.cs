using FluentValidation;
using Liquid.Runtime.Configuration;
using System;

namespace Liquid.Domain
{
    public class MessageBrokerConfiguration : LightConfig<MessageBrokerConfiguration>
    {
        public string ConnectionString { get; set; }

        public override void Validate()
        {
            RuleFor(d => ConnectionString).NotEmpty().WithMessage("ConnectionString settings should not be empty.");
            RuleFor(d => ConnectionString).Matches("Endpoint=sb://").WithMessage("No Endpoint on configuration string has been informed.");
            RuleFor(d => ConnectionString).Matches("SharedAccessKeyName=").WithMessage("No SharedAccessKeyName on configuration string has been informed.");
            RuleFor(d => ConnectionString).Matches("SharedAccessKey=").WithMessage("No SharedAccessKey on configuration string has been informed.");
        }
    }
}
