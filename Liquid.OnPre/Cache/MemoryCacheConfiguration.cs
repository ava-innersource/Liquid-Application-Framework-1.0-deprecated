using FluentValidation;
using Liquid.Runtime.Configuration;

namespace Liquid.OnWindowsClient
{
    /// <summary>
    /// Configuration of MemoryCache
    /// </summary>
    public class MemoryCacheConfiguration : LightConfig<MemoryCacheConfiguration>
    {
        /// <summary>
        /// SlidingExpirationSeconds
        /// </summary>
        public int SlidingExpirationSeconds { get; set; }
        /// <summary>
        /// AbsoluteExpirationRelativeToNowSeconds
        /// </summary>
        public int AbsoluteExpirationRelativeToNowSeconds { get; set; }
        /// <summary>
        /// Validate of MemoryCacheConfiguration
        /// </summary>
        public override void Validate()
        { 
            RuleFor(b => b.SlidingExpirationSeconds).NotEmpty().WithErrorCode("SLIDING_EXPIRATION_SECONDS_CANNOT_BE_EMPTY_ON_THE_CONFIG");
            RuleFor(b => b.AbsoluteExpirationRelativeToNowSeconds).NotEmpty().WithErrorCode("ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW_SECONDS__CANNOT_BE_EMPTY_ON_THE_CONFIG");
        }
    }
}
