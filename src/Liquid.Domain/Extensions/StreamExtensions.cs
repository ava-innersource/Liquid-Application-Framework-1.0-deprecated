using System.IO;

namespace Liquid.Domain
{
    /// <summary>
    /// Implement extensions of stream
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Read bytes from stream (e.g: byte[] bytePicture = streamPicure.ReadAllBytes())
        /// </summary>
        /// <param name="stream">the desired Stream </param>
        /// <returns>the array of byte from stream</returns>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            var memStream = stream as MemoryStream;
            if (stream is MemoryStream)
                return memStream.ToArray();

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
