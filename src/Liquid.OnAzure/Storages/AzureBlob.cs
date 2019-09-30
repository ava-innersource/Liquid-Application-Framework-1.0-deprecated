using Liquid.Interfaces;
using Liquid.Repository;
using Liquid.Runtime.Configuration.Base;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount  
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types  
using System;
using System.IO;
using System.Threading.Tasks;

namespace Liquid.OnAzure
{

    /// <summary>
    /// Cartridge for Azure Blob
    /// </summary>
    public class AzureBlob : ILightMediaStorage
    {
        public MediaStorageConfiguration mediaStorageConfiguration { get; set; }
        public String Conection { get; set; }
        public string Permission { get; set; }
        private CloudBlobContainer _containerReference { get; set; }
        private string _container = string.Empty;
        public String Container
        {
            get { return _container; }
            set
            {
                _container = value;
                _containerReference = _blobClient.GetContainerReference(value);
                _containerReference.CreateIfNotExistsAsync();
                _containerReference.SetPermissionsAsync(
                    new BlobContainerPermissions
                    {
                        PublicAccess = !string.IsNullOrEmpty(Permission) ?
                        (Permission.Equals("Blob") ? BlobContainerPublicAccessType.Blob :
                        (Permission.Equals("Off") ? BlobContainerPublicAccessType.Off :
                        (Permission.Equals("Container") ? BlobContainerPublicAccessType.Container :
                        (Permission.Equals("Unknown") ? BlobContainerPublicAccessType.Unknown : BlobContainerPublicAccessType.Blob)))) : BlobContainerPublicAccessType.Blob
                    });
            }
        }

        private CloudStorageAccount _storageAAccountConnection
        {
            get { return CloudStorageAccount.Parse(Conection); }
        }

        private CloudBlobClient _blobClient
        {
            get { return _storageAAccountConnection.CreateCloudBlobClient(); }
        }

        public void Initialize()
        {
            //Get the configuration on appsetting. But in this case the features can be accessed outside from the repository
            this.mediaStorageConfiguration = LightConfigurator.Config<MediaStorageConfiguration>("MediaStorage");
            this.Conection = mediaStorageConfiguration.ConnectionString;
            this.Permission = mediaStorageConfiguration.Permission;
            this.Container = mediaStorageConfiguration.Container;

            // If the MS has the configuration outside from the repository will be used this context and not inside
            if (WorkBench.Repository != null)
            {
                WorkBench.Repository.SetMediaStorage(this);
            }
        }

        public async Task<ILightAttachment> GetAsync(string resourceId, string id)
        {
            var blob = _containerReference.GetBlobReference(id);
            Stream stream = new MemoryStream();
            await blob.DownloadToStreamAsync(stream);
            LightAttachment _blob = new LightAttachment()
            {
                MediaStream = stream,
                Id = id,
                ResourceId = resourceId,
                ContentType = blob.Properties.ContentType,
                Name = blob.Name,
                MediaLink = blob.Uri.AbsoluteUri
            };
            return _blob;
        }

        private byte[] ReadFully(Stream input, int size)
        {
            byte[] buffer = new byte[size];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, size)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public async void InsertUpdateAsync(ILightAttachment attachment)
        {
            var targetFile = attachment.Id;
            var blockBlob = _containerReference.GetDirectoryReference(attachment.ResourceId).GetBlockBlobReference(targetFile);
            blockBlob.Properties.ContentType = attachment.ContentType;
            await blockBlob.UploadFromByteArrayAsync(ReadFully(attachment.MediaStream, blockBlob.StreamWriteSizeInBytes),
                            0, (int)attachment.MediaStream.Length);

        }

        public void Remove(ILightAttachment attachment)
        {
            var targetFile = attachment.Id;
            var blockBlob = _containerReference.GetDirectoryReference(attachment.ResourceId).GetBlockBlobReference(targetFile);
            blockBlob.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Method to run Health Check for AzureBlob Media Storage
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                TimeSpan span = new TimeSpan(0, 0, 1);
                this._containerReference.AcquireLeaseAsync(span);
                this._containerReference.BreakLeaseAsync(span);
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }
        }
    }
}
