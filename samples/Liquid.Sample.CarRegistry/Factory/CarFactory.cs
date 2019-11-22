using System;

namespace Liquid.Sample.CarRegistry
{
    /// <summary>
    /// Factory of Car
    /// </summary>
    public class CarFactory
    {
        /// <summary>
        /// Create an object Car from a LegacyCarVM
        /// </summary>
        /// <param name="legacy">Object LegacyCarVM</param>
        /// <returns>Returns a object Car</returns>
        public static Car Create(LegacyCarVM legacy)
        {
            if (legacy == null) throw new ArgumentNullException(nameof(legacy));

            return new Car
            {
                Id = Guid.NewGuid(),
                IdLegacy = legacy.Id,
                Description = legacy.Description
            };
        }
    }
}
