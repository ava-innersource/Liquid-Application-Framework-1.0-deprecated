using FluentValidation;
using Liquid.Repository;
using Liquid.Runtime.Configuration;

namespace Liquid.OnAWS
{
    public class DynamoDBConfiguration : LightConfig<DynamoDBConfiguration>
    {
        public string AccessKeyID { get; set; }

        public string SecretAccessKey { get; set; }

        public string  ServiceURL { get; set; }

        public bool UseHttp { get; set; }

  
        public MediaStorageConfiguration MediaStorage  {get; set; }

        public override void Validate()
        {
            RuleFor(d => AccessKeyID).NotEmpty().WithMessage("AccessKeyID on DynamoDB settings should not be empty.");

            RuleFor(d => SecretAccessKey).NotEmpty().WithMessage("SecretAccessKey on DynamoDB settings should not be empty.");

            RuleFor(d => ServiceURL).NotEmpty().WithMessage("ServiceURL on DynamoDB settings should not be empty.");            
        }
    }
}