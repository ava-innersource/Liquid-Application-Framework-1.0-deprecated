using FluentValidation;
using Liquid.Runtime.Configuration;

namespace Liquid.OnAzure
{
    public class AzureRedisConfiguration : LightConfig<AzureRedisConfiguration>
    {
        public string Configuration { get; set; }
        public string InstanceName { get; set; }
        public int SlidingExpirationSeconds { get; set; }
        public int AbsoluteExpirationRelativeToNowSeconds { get; set; }
        public override void Validate()
        {
            RuleFor(b => b.Configuration).NotEmpty().WithErrorCode("CONFIGURATION_CANNOT_BE_EMPTY_ON_THE_CONFIG");
            RuleFor(b => b.InstanceName).NotEmpty().WithErrorCode("INSTANCE_NAME_CANNOT_BE_EMPTY_ON_THE_CONFIG");
            RuleFor(b => b.SlidingExpirationSeconds).NotEmpty().WithErrorCode("SLIDING_EXPIRATION_SECONDS_CANNOT_BE_EMPTY_ON_THE_CONFIG");
            RuleFor(b => b.AbsoluteExpirationRelativeToNowSeconds).NotEmpty().WithErrorCode("ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW_SECONDS__CANNOT_BE_EMPTY_ON_THE_CONFIG");
        }
    }
}
