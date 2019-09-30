using Liquid.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Liquid.Runtime
{
    /// <summary>
    /// Configuration section about Localization on Business Critics Messages 
    /// </summary>
    public class LocalizationConfig : LightConfig<LocalizationConfig>
    {
        /// <summary>
        /// Defines the chosen culture when none is requested by client
        /// </summary>
        public string DefaultCulture { get; set; }

        /// <summary>
        /// Defines the list of cultures available
        /// </summary>
        public string[] SupportedCultures { get; set; }

        public override void Validate()
        {
            RuleFor(x => DefaultCulture).NotEmpty().WithMessage("A Default Culture should be defined.");
        }
    }
}
