using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Liquid.Interfaces;
using Liquid.Repository;
using System.IO;
using System.Threading.Tasks;

namespace Liquid.OnGoogle
{
    public class GoogleStorage : ILightMediaStorage
    {
        public int MyProperty { get; set; }
        public string Connection { get; set; } // projectID
        public string Container { get; set; } // bucketName
        public dynamic mediaStorageConfiguration { get; set; }

        private StorageClient _client = null;
        private Bucket _bucket = null;

        public void Initialize()
        {
            var credential = GoogleCredential.GetApplicationDefault();
            _client = StorageClient.Create(credential);
            // Make an authenticated API request. 
            _bucket = _client.CreateBucket(this.Connection, this.Container);
            Workbench.Instance.Repository.SetMediaStorage(this);
        }

        public async Task<ILightAttachment> GetAsync(string resourceId, string id)
        {
            Stream stream = new MemoryStream();
            await _client.DownloadObjectAsync(this.Container, resourceId + "/" + id, stream);

            LightAttachment _blob = new LightAttachment()
            {
                MediaStream = stream,
                Id = id,
                ResourceId = resourceId,
                ContentType = MimeMapping.MimeUtility.GetMimeMapping(id),
                Name = id
            };
            return _blob;
        }

        public Task InsertUpdateAsync(ILightAttachment attachment)
        {
            return _client.UploadObjectAsync(this.Container, attachment.ResourceId + "/" + attachment.Name, attachment.ContentType, attachment.MediaStream);
        }

        public Task Remove(ILightAttachment attachment)
        {
            _client.DeleteObject(this.Container, attachment.ResourceId + "/" + attachment.Id);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Method to run HealthCheck for Google Storage
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                var bucket = _client.ListBuckets(this._bucket.ProjectNumber.ToString());
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }
        }

    }
}
