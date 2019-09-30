using FluentValidation;
using Liquid.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Runtime
{
    public class SecretsConfiguration : LightConfig<SecretsConfiguration>
    {
        public string Module { get; set; }
        public bool Enable { get; set; }
        public string Name { get; set; }

        public override void Validate()
        {
            //Validating parameters that do not come from body by a Fluent-like sintax
            RuleFor(d => Module).NotEmpty().WithMessage("'Module' on Secrets settings should not be empty.");
            RuleFor(d => Enable).NotNull().WithMessage("'Enable' on Secrets settings should not be empty.");
            RuleFor(d => Name).NotEmpty().WithMessage("'Name' on Secrets settings should not be empty.");
        }
    }
}
