// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Liquid.Base
{
    /// <summary>
    /// Helper methods for dealing with enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Parses a stringified as a <typeparamref name="TEnum"/>, making sure it is a defined Enum label.
        /// </summary>
        /// <typeparam name="TEnum">The type of the Enum that the value should correspond to.</typeparam>
        /// <param name="value">An enum in a string representation.</param>
        /// <returns>The corresponding enum value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When the value doesn't correspond to an Enum label.
        /// </exception>
        /// <remarks>
        /// This method is safer than <see cref="Enum.Parse{TEnum}(string)"/> because it will make sure
        /// that numeric values correspond to enum labels.
        /// </remarks>
        public static TEnum SafeParse<TEnum>(string value)
            where TEnum : struct
        {
            var result = Enum.Parse<TEnum>(value);

            if (!Enum.IsDefined(typeof(TEnum), result))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must correspond to a defined Enum label.");
            }

            return result;
        }

        /// <summary>
        /// Tries to parse a stringified as a <typeparamref name="TEnum"/>, making sure it is a defined Enum label.
        /// </summary>
        /// <typeparam name="TEnum">The type of the Enum that the value should correspond to.</typeparam>
        /// <param name="value">An enum in a string representation.</param>
        /// <param name="result">The corresponding enum value.</param>
        /// <returns>True when the value corresponds to a defined Enum label.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When the value doesn't correspond to an Enum label.
        /// </exception>
        /// <remarks>
        /// This method is safer than <see cref="Enum.TryParse{TEnum}(string, out TEnum)"/> because it will make sure
        /// that numeric values correspond to Enum labels.
        /// </remarks>
        public static bool SafeTryParse<TEnum>(string value, out TEnum result)
            where TEnum : struct
        {
            if (!Enum.TryParse<TEnum>(value, out result))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(TEnum), result))
            {
                return false;
            }

            return true;
        }
    }
}
