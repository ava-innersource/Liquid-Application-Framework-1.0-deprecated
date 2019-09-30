using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Liquid.Base
{
    /// <summary>
    /// Class responsible for building the Exception
    /// with the data that came from InvalidInputException
    /// </summary>
    [Serializable]
    public class LightException : Exception, ISerializable
    {
        /// <summary>
        /// Gets the newline string defined for this environment. 
        /// In this case splits the Sources in a list
        /// </summary
        public override string Source
        {
            get
            {
                List<string> source = new List<string>();
                source.AddRange(base.Source.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                source.RemoveAll(x => x.Contains("Liquid.Base"));
                return string.Join(Environment.NewLine, source.ToArray());
            }
        }

        /// <summary>
        /// Gets the newline string defined for this environment. 
        /// In this case splits the StackTrace in a list
        /// </summary
        public override string StackTrace
        {
            get
            {
                List<string> stackTrace = new List<string>();
                stackTrace.AddRange(base.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                stackTrace.RemoveAll(x => x.Contains("Liquid.Base"));
                return string.Join(Environment.NewLine, stackTrace.ToArray());
            }
        }
        /// <summary>
        /// Throw an exception
        /// </summary>
        public LightException() : base()
        {
        }

        /// <summary>
        /// Throw an exception with a message 
        /// </summary>
        /// <param name="message">the message showed on the ModelView</param>
        public LightException(string message) : base(message)
        {
        }

        /// <summary>
        /// Throw an exception with a message and details of the object Exception
        /// </summary>
        /// <param name="message">message showed on the ModelView</param>
        /// <param name="innerException">describes the error that caused the current exception</param>
        public LightException(string message, Exception innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// Building a LightException with detailed data
        /// </summary>
        /// <param name="info">The SerializationInfo holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected LightException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
