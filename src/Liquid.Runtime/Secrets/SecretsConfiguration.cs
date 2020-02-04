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
            RuleFor(d => Enable).NotNull().WithMessage("'Enable' on Secrets settings should not be empty.");
            
            //Validating parameters that do not come from body by a Fluent-like sintax
            RuleFor(d => Module).NotEmpty().When(d => Enable).WithMessage("'Module' on Secrets settings should not be empty.");
            RuleFor(d => Name).NotEmpty().When(d => Enable).WithMessage("'Name' on Secrets settings should not be empty.");
        }
    }
}
