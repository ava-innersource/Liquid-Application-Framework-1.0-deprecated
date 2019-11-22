using FluentValidation;
using Liquid.Repository;
using System;

namespace Liquid.Sample.CarRegistry
{
    /// <summary>
    /// Describes a Car 
    /// </summary>
    public class Car : LightModel<Car>
    {
        /// <summary>
        /// Identifies the car
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the car in the legacy system
        /// </summary>
        public string IdLegacy { get; set; }

        /// <summary>
        /// Describes the car object
        /// </summary>
        public string Description { get; set; }

        /// <inheritdoc/>
        public override void Validate()
        {
            RuleFor(i => i.Id).NotEmpty().WithErrorCode("ID_MUSTNOT_BE_EMPTY");
        }
    }
}
