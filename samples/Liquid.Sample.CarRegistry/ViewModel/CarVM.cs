using FluentValidation;
using Liquid.Domain;
using System;

namespace Liquid.Sample.CarRegistry
{
    /// <summary>
    /// Class CarVM
    /// </summary>
	public class CarVM : LightViewModel<CarVM>
    {
        /// <summary>
        /// Car VM Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Car VM Id Legacy
        /// </summary>
		public string IdLegacy { get; set; }
        /// <summary>
        /// Car VM Description
        /// </summary>
		public string Description { get; set; }

        /// <summary>
        /// Method to validate a Car VM Id
        /// </summary>
        public override void Validate()
        {
            RuleFor(i => i.Id).NotEmpty().WithErrorCode("ID_MUSTNOT_BE_EMPTY");
        }
    }
}