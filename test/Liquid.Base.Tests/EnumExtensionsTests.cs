// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoFixture.Xunit2;
using Liquid.Tests;
using Xunit;

namespace Liquid.Base.Tests
{
    public class EnumExtensionsTests
    {
        [Theory, AutoData]
        public void SafeTryParseWhenValueIsDefinedReturnsCorrectLabel(CultureTypes value)
        {
            Assert.True(EnumExtensions.SafeTryParse<CultureTypes>(value.ToString(), out var actual));
            Assert.Equal(actual, value);
        }

        [Theory, AutoData]
        public void SafeTryParseWhenValueIsNotDefinedReturnsFalse(string value)
        {
            Assert.False(EnumExtensions.SafeTryParse<CultureTypes>(value, out var _));
        }

        [Theory, AutoData]
        public void SafeTryParseWhenValueIsNumericAndNotDefinedReturnsFalse(int value)
        {
            var max = Enum.GetValues(typeof(CultureTypes)).OfType<CultureTypes>().Max();
            Assert.False(EnumExtensions.SafeTryParse<CultureTypes>((value + max).ToString(), out var _));
        }

        [Theory, AutoData]
        public void SafeParseWhenValueIsDefinedReturnsCorrectLabel(CultureTypes value)
        {
            var actual = EnumExtensions.SafeParse<CultureTypes>(value.ToString());
            Assert.Equal(actual, value);
        }

        [Theory, AutoData]
        public void SafeParseWhenValueIsNotDefinedThrowsArgumentException(string value)
        {
            Assert.Throws<ArgumentException>(() => EnumExtensions.SafeParse<CultureTypes>(value));
        }

        [Theory, AutoData]
        public void SafeParseWhenValueIsNumericAndNotDefinedThrowsOutOfRangeException(int value)
        {
            var max = Enum.GetValues(typeof(CultureTypes)).OfType<CultureTypes>().Max();
            Assert.Throws<ArgumentOutOfRangeException>(() => EnumExtensions.SafeParse<CultureTypes>((value + max).ToString()));
        }
    }
}
