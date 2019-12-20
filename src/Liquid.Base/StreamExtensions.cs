using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Liquid
{
    /// <summary>
    /// Extensions to simplify the manipulation of streams.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Read the stream to the end and converts it to an UTF8 string.
        /// </summary>
        /// <param name="stream">The stream that will be read to the end.</param>
        /// <param name="encoding">The encoding of the string in the stream.</param>
        /// <returns>The contents of the stream read as a string.</returns>
        public static string AsString(this Stream stream) => AsString(stream, Encoding.UTF8);

        /// <summary>
        /// Read the stream to the end and converts it to a string accordingly to the desired encoding.
        /// </summary>
        /// <param name="stream">The stream that will be read to the end.</param>
        /// <param name="encoding">The encoding of the string in the stream.</param>
        /// <returns>The contents of the stream read as a string.</returns>
        public static string AsString(this Stream stream, Encoding encoding)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            using (var reader = new StreamReader(stream, encoding))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Read the stream to the end and converts it to an UTF8 string asynchronously.
        /// </summary>
        /// <param name="stream">The stream that will be read to the end.</param>
        /// <param name="encoding">The encoding of the string in the stream.</param>
        /// <returns>Task containg the result of the conversion from stream to string.</returns>
        public static Task<string> AsStringAsync(this Stream stream) => AsStringAsync(stream, Encoding.UTF8);


        /// <summary>
        /// Read the stream to the end and converts it to a string accordingly to the desired encoding asynchronously.
        /// </summary>
        /// <param name="stream">The stream that will be read to the end.</param>
        /// <param name="encoding">The encoding of the string in the stream.</param>
        /// <returns>Task containg the result of the conversion from stream to string.</returns>
        public static async Task<string> AsStringAsync(this Stream stream, Encoding encoding)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            using (var reader = new StreamReader(stream, encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
