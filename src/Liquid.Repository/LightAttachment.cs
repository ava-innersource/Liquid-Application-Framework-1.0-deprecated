using System;
using System.IO;
using Liquid.Interfaces;
using Newtonsoft.Json;

namespace Liquid.Repository
{
    public  class LightAttachment : ILightAttachment
    {
        [JsonProperty(PropertyName = "_id")]
        public virtual string Id { get; set; }
        [JsonProperty(PropertyName = "_name")]
        public virtual string Name { get; set; }
        [JsonProperty(PropertyName = "_rid")]
        public virtual string ResourceId { get; set; } 
        [JsonProperty(PropertyName = "contentType")]
        public virtual string ContentType { get; set; }
        [JsonIgnore]
        public virtual Stream MediaStream { get; set; }
        [JsonIgnore]
        public string MediaLink { get; set; }

        /// <summary>
        /// Destructor, calls disposing to free unmanaged resources.
        /// </summary>
        ~LightAttachment()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipose of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether this is being called from Dispose or from the GC.</param>
        /// <remarks>
        /// Implements the disposing pattern.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (MediaStream != null)
                {
                    MediaStream.Dispose();
                    MediaStream = null;
                }
            }
        }
    }
} 
