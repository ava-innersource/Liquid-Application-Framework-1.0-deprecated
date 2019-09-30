using FluentValidation;
using Liquid.Runtime.Configuration;

namespace Liquid.OnAWS
{
    /// <summary>
    ///  Configuration of the for connect a Service Bus (Queue / Topic).
    /// </summary>
    public class SqsSnsConfiguration : LightConfig<SqsSnsConfiguration>
    {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public override void Validate()
        {
            RuleFor(d => AwsAccessKeyId).NotEmpty().WithMessage("AwsAccessKeyId settings should not be empty.");
            RuleFor(d => AwsSecretAccessKey).NotEmpty().WithMessage("AwsSecretAccessKey settings should not be empty."); 
        }
    }
}
