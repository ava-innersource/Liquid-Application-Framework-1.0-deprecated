using Liquid.Base;
using System;
using System.Runtime.Serialization;

namespace Liquid.Repository
{ 
    [Serializable]
    public class BadRepositoryInitializationException : LightException
    {
        public BadRepositoryInitializationException(string lightRepositoryTypeName) : 
            base($"{lightRepositoryTypeName} repository not was correctly initialized. For direct instantiation, it must be constructed as the following example: {lightRepositoryTypeName} myNewRepo = new {lightRepositoryTypeName}(\"MYNEWREPO\")")
        {}

        /// <summary>
        /// Building a LightException with detailed data
        /// </summary>
        /// <param name="info">The SerializationInfo holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected BadRepositoryInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}