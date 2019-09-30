using FluentValidation;
using Liquid.Repository;
using Liquid.Runtime.Configuration;
using Microsoft.Azure.Documents.Client;

namespace Liquid.OnAzure
{/// <summary>
/// The Configuration for CosmosDBConfiguration
/// </summary>
    public class CosmosDBConfiguration : LightConfig<CosmosDBConfiguration>
    {
        public string Endpoint { get; set; }
        public string AuthKey { get; set; }
        public string DatabaseId { get; set; }
        public string CollectionName { get; set; }
        public bool CreateIfNotExists { get; set; }
        public int DatabaseRUs { get; set; }
        public string ConnectionMode { get; set; }
        public string ConnectionProtocol { get; set; }

        /// <summary>
        /// Configuration to create an Azure blob block in containers
        /// </summary>
        public MediaStorageConfiguration MediaStorage { get; set; }

        /// <summary>
        /// The necessary validation to create an blob block
        /// </summary>
        public override void Validate()
        {
            RuleFor(d => Endpoint).NotEmpty().WithMessage("Endpoint on CosmosDB settings should not be empty.");

            RuleFor(d => AuthKey).NotEmpty().WithMessage("AuthKey on CosmosDB settings should not be empty.");

            RuleFor(d => DatabaseId).NotEmpty().WithMessage("DatabaseId on CosmosDB settings should not be empty.");

            RuleFor(d => CreateIfNotExists).NotNull().WithMessage("CreateIfNotExists on CosmosDB settings should not be empty.");

            RuleFor(d => DatabaseRUs).NotEmpty().WithMessage("DatabaseRUs on CosmosDB settings should not be empty.");
        }
    }
}