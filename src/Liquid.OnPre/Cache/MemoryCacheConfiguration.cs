using FluentValidation;
using Liquid.Runtime.Configuration;

namespace Liquid.OnPre
{
    /// <summary>
    /// Configuration of MemoryCache
    /// </summary>
    public class MemoryCacheConfiguration : LightConfig<MemoryCacheConfiguration>
    {
        /// <summary>
        /// SlidingExpirationSeconds
        /// </summary>
        public int? SlidingExpirationSeconds { get; set; }
        /// <summary>
        /// AbsoluteExpirationRelativeToNowSeconds
        /// </summary>
        public int? AbsoluteExpirationRelativeToNowSeconds { get; set; }
        /// <summary>
        /// Validate of MemoryCacheConfiguration
        /// </summary>
        public override void Validate()
        {
            if (SlidingExpirationSeconds == null)
            {
                SlidingExpirationSeconds = 0;
            }
            if (AbsoluteExpirationRelativeToNowSeconds == null)
            {
                AbsoluteExpirationRelativeToNowSeconds = 0;
            }
        }
    }
}
