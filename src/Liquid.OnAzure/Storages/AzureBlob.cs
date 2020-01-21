using Liquid.Interfaces;
using Liquid.Repository;
using Liquid.Runtime.Configuration.Base;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Liquid.OnAzure
{
    /// <summary>
    /// Cartridge for Azure Blob
    /// </summary>
    public class AzureBlob : ILightMediaStorage
    {
        /// <summary>
        /// Used to access the actual Storage container.
        /// </summary>
        private CloudBlobContainer _containerReference;

        /// <summary>
        /// The name of the Azure Blob container where data is being stored.
        /// </summary>
        private string _container = string.Empty;

        /// <summary>
        /// The connection string to the Azure Blob storage.
        /// </summary>
        private string _connection; // please don't remove - we will remove the property later

        /// <summary>
        /// Permission to be applied to newly created containers.
        /// </summary>
        private string _permission; // please don't remove - we will remove the property later
        private readonly ILightTelemetry _telemetry;

        /// <summary>
        /// The connection string for the storage provider.
        /// </summary>
        [Obsolete("This property will be removed in later version. Please refrain from accessing it.")]
        public string Connection { get => _connection; set => _connection = value; }

        /// <summary>
        /// Permission to be applied to newly created containers.
        /// </summary>
        [Obsolete("This property will be removed in later version. Please refrain from accessing it.")]
        public string Permission { get => _permission; set => _permission = value; }

        /// <summary>
        /// The container name used to store data in the provider.
        /// </summary>
        [Obsolete("This property will be removed in later version. Please refrain from accessing it.")]
        public string Container
        {
            get { return _container; }
            set
            {
                _container = value;
                SetContainerReference(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AzureBlob"/>.
        /// </summary>        
        public AzureBlob()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="AzureBlob"/>.
        /// </summary>
        /// <param name="configuration">The configuration for this class.</param>
        public AzureBlob(MediaStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            Initialize(configuration);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AzureBlob"/>.
        /// </summary>
        /// <param name="configuration">The configuration for this class.</param>
        public AzureBlob(MediaStorageConfiguration configuration, ILightTelemetry telemetry)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

            Initialize(configuration);
        }

        /// <summary>
        /// Initializes the object. 
        /// </summary>
        /// <remarks>
        /// This method obtains the configuration using <see cref="LightConfigurator"/> directly.
        /// </remarks>
        public void Initialize()
        {
            // TODO: mark this as obsolete and remove it.

            // Get the configuration on appsetting. But in this case the features can be accessed outside from the repository
            var configuration = LightConfigurator.Config<MediaStorageConfiguration>("MediaStorage");
            Initialize(configuration);
        }

        /// <summary>
        /// Initializes the class based on the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuiration for this class.</param>
        private void Initialize(MediaStorageConfiguration configuration)
        {
            _connection = configuration.ConnectionString;
            _container = configuration.Container;
            _permission = configuration.Permission;

            SetContainerReference(_container);

            // If the MS has the configuration outside from the repository will be used this context and not inside
            Workbench.Instance.Repository.SetMediaStorage(this);
        }

        /// <summary>
        /// Gets a media attachment from the blob storage.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="id"></param>
        /// <returns>The attachment that was obtained from the underlying storage.</returns>
        public async Task<ILightAttachment> GetAsync(string resourceId, string id)
        {
            //var blob = _containerReference.GetBlobReference(resourceId + "/" + id);

            var blob = _containerReference.GetDirectoryReference(resourceId).GetBlockBlobReference(id);

            var stream = new MemoryStream();

            await blob.DownloadToStreamAsync(stream);

            stream.Seek(0, SeekOrigin.Begin);

            return new LightAttachment
            {
                MediaStream = stream,
                Id = id,
                ResourceId = resourceId,
                ContentType = blob.Properties.ContentType,
                Name = blob.Name,
                MediaLink = blob.Uri.AbsoluteUri
            };
        }

        public Task InsertUpdateAsync(ILightAttachment attachment)
        {
            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            var blockBlob = _containerReference.GetDirectoryReference(attachment.ResourceId).GetBlockBlobReference(attachment.Id);

            blockBlob.Properties.ContentType = attachment.ContentType;

            return blockBlob.UploadFromStreamAsync(attachment.MediaStream, attachment.MediaStream.Length);
        }

        public Task Remove(ILightAttachment attachment)
        {
            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            var targetFile = attachment.Id;
            var blockBlob = _containerReference.GetDirectoryReference(attachment.ResourceId).GetBlockBlobReference(targetFile);
            return blockBlob.DeleteIfExistsAsync();
        }


        /// <summary>
        /// Checks if the container exists. Will return unhealthy when the container doens't exists
        /// or if there's a connection problem with the Azure Blob Storage.
        /// </summary>
        /// <returns>
        /// LightHealth.HealthCheck.Healthy when the container exists and can be connected to.
        /// </returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Any exception is treated as unhealthy and shouldn't be returned to the caller.")]
        public LightHealth.HealthCheck HealthCheck(string _1, string _2)
        {
            try
            {
                var exists = _containerReference.ExistsAsync().GetAwaiter().GetResult();

                if (!exists)
                {
                    throw new Exception($"Container '{_container}' doesn't exists");
                }

                return LightHealth.HealthCheck.Healthy;
            }
            catch (Exception e)
            {
                _telemetry?.TrackException(e);

                return LightHealth.HealthCheck.Unhealthy;
            }
        }

        private CloudBlobClient GetBlobClientFromConnection()
        {
            return CloudStorageAccount.Parse(_connection).CreateCloudBlobClient();
        }

        private void SetContainerReference(string containerName)
        {
            _containerReference = GetBlobClientFromConnection().GetContainerReference(containerName);
            _containerReference.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            _containerReference.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = !string.IsNullOrEmpty(_permission) ?
                    (_permission.Equals("Blob") ? BlobContainerPublicAccessType.Blob :
                    (_permission.Equals("Off") ? BlobContainerPublicAccessType.Off :
                    (_permission.Equals("Container") ? BlobContainerPublicAccessType.Container :
                    (_permission.Equals("Unknown") ? BlobContainerPublicAccessType.Unknown : BlobContainerPublicAccessType.Blob)))) : BlobContainerPublicAccessType.Blob
            }).GetAwaiter().GetResult();
        }
    }
}
