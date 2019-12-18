// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace Liquid.Tests
{
    /// <summary>
    /// Configure a <see cref="Xunit.TheoryAttribute"/> to use NSubstitute to create mocks for interfaces.
    /// </summary>
    /// <example>
    /// Configure your Theory with this attribute and use a interface as a parameter.
    /// <code>
    /// [Theory, AutoSubstituteData]
    /// public void AddToCacheSameServiceTypeTwiceThrows(WorkBenchServiceType type, IWorkBenchService service1, IWorkBenchService service2)
    /// {
    ///     Workbench.Instance.AddToCache(type, service1);
    ///
    ///     Assert.ThrowsAny<Exception>(() => Workbench.Instance.AddToCache(type, service1));
    ///
    ///     Assert.ThrowsAny<Exception>(() => Workbench.Instance.AddToCache(type, service2));
    /// }
    /// </code>
    /// </example>
    public class AutoSubstituteDataAttribute : AutoDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSubstituteDataAttribute"/> class.
        /// </summary>
        public AutoSubstituteDataAttribute()
            : base(() => new Fixture().Customize(new AutoNSubstituteCustomization()))
        {
            // empty
        }
    }
}
