using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace Liquid.Base.Tests
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
    ///     WorkBench.AddToCache(type, service1);
    ///
    ///     Assert.ThrowsAny<Exception>(() => WorkBench.AddToCache(type, service1));
    ///
    ///     Assert.ThrowsAny<Exception>(() => WorkBench.AddToCache(type, service2));
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
