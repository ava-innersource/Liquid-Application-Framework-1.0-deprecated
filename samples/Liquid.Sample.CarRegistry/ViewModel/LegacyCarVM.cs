using FluentValidation;
using Liquid.Domain;

namespace Liquid.Sample.CarRegistry
{
    /// <summary>
    /// Class LegacyCarVM
    /// </summary>
	public class LegacyCarVM : LightViewModel<LegacyCarVM>
    {
        /// <summary>
        /// Legacy Car Id
        /// </summary>
		public string Id { get; set; }
        /// <summary>
        /// Description Id
        /// </summary>
		public string Description { get; set; }

        /// <summary>
        /// Method to validate a Car Id
        /// </summary>
		public override void Validate()
        {
            RuleFor(i => i.Id).NotEmpty().WithErrorCode("ID_MUSTNOT_BE_EMPTY");
        }
    }
}
