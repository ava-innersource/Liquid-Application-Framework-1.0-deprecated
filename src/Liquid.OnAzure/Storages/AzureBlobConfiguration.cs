using FluentValidation;

namespace Liquid.Runtime.Configuration
{
    public class AzureBlobConfiguration : LightConfig<AzureBlobConfiguration>
    {
        public string ConnectionString { get; set; }
        public string Container { get; set; }

        public override void Validate()
        {
            RuleFor(d => ConnectionString).NotEmpty().WithMessage("'ConnectionString' on AzureBlob settings should not be empty.");

            RuleFor(d => Container).NotEmpty().WithMessage("'Container' on AzureBlob settings should not be empty.");

        }
    }
}