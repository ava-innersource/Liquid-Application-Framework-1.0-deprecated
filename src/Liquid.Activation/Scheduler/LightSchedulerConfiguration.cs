using FluentValidation; 
using Liquid.Runtime.Configuration;

namespace Liquid.Activation
{
    /// <summary>
    /// The Configuration for FileConfiguration
    /// </summary>
    public class LightSchedulerConfiguration : LightConfig<LightSchedulerConfiguration>
    {
        /// <summary>
        /// Interval on minute for process
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Additional on minute for process
        /// </summary>
        public int RandonAdditional { get; set; }

        /// <summary>
        /// The necessary validation to create
        /// </summary>
        public override void Validate()
        { 
            RuleFor(d => Interval).NotEmpty().WithMessage("Interval should not be empty.");
            RuleFor(d => RandonAdditional).NotEmpty().WithMessage("Randon Additional should not be empty.");
        }
    }
}