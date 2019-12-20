using System;
using System.IO;

namespace Liquid.Interfaces
{
    public interface ILightAttachment
    {
        string Id { get; set; }
        string Name { get; set; }
        string ContentType { get; set; }
        string MediaLink { get; set; }
        Stream MediaStream { get; set; }
        /// <summary>
        /// The directory where the attachment will be stored into.
        /// </summary>
        string ResourceId { get; set; }

        //string DirectoryName { get; set; }
    }
}
