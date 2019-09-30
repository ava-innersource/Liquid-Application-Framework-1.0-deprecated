using FluentValidation;
using Liquid.Repository;
using Liquid.Runtime.Configuration;

namespace Liquid.OnGoogle
{
    public class GoogleFireStoreConfiguration : LightConfig<GoogleFireStoreConfiguration>
    {
        public string ProjectID { get; set; }
        public string DatabaseID { get; set; }
        public string CollectionName { get; set; }

        /// <summary>
        /// Configuration to create an Azure blob block in containers
        /// </summary>
        public MediaStorageConfiguration MediaStorage { get; set; }
        public override void Validate()
        {
            RuleFor(d => ProjectID).NotEmpty().WithMessage("ProjectID on Google Cloud settings should not be empty.");
        }
    }
}
