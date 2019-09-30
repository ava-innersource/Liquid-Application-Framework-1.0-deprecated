using Liquid.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Liquid.Domain.Localization
{
    public class LocalizationConfig : LightConfig<LocalizationConfig>
    {
        public string DefaultCulture { get; set; }

        public string[] SupportedCultures { get; set; }

        public override void Validate()
        {
            RuleFor(x => DefaultCulture).NotEmpty().WithMessage("A Default Culture should be defined.");
        }
    }
}
