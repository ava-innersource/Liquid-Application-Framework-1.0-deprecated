using FluentValidation;
using Liquid.Runtime.Configuration;

namespace Liquid.Runtime
{
    public class CorsConfiguration : LightConfig<CorsConfiguration>
    {
        public string Origins { get; set; }
        public string Methods { get; set; }
        public string Headers { get; set; }

        public override void Validate()
        {
            //Validating parameters that do not come from body by a Fluent-like sintax
            RuleFor(d => Origins).NotEmpty().WithMessage("'Origins' on Cors settings should not be empty.");
            RuleFor(d => Methods).NotEmpty().WithMessage("'Methods' on Cors settings should not be empty.");
            RuleFor(d => Headers).NotEmpty().WithMessage("'Headers' on Cors settings should not be empty."); 
        }
    }
}
