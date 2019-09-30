using FluentValidation;
using Liquid.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.OnAWS.Telemetry
{
    public class CloudWatchConfiguration : LightConfig<CloudWatchConfiguration>
    {
        public string AccessKeyID { get; set; }

        public string SecretAccessKey { get; set; }

        public bool UseHttp { get; set; }

        public override void Validate()
        {
            RuleFor(d => AccessKeyID).NotEmpty().WithMessage("AccessKeyID on DynamoDB settings should not be empty.");

            RuleFor(d => SecretAccessKey).NotEmpty().WithMessage("SecretAccessKey on DynamoDB settings should not be empty.");
        }
    }
}
