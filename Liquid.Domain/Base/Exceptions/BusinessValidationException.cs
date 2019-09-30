using Liquid.Base;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Liquid.Domain.Base
{
    /// <summary>
    /// Class responsible for return the InvalidInputException object
    /// to build the LightException
    /// 
    /// Important: This attribute is NOT inherited from Exception, and MUST be specified 
    /// otherwise serialization will fail with a SerializationException stating that
    /// "Type X in Assembly Y is not marked as serializable."
    /// </summary>
    [Serializable]
    public class BusinessValidationException : LightException
    {
        public List<Critic> InputErrors { get; } = new List<Critic>();

        /// <summary>
        /// Build the object Critic and add to inputErrors list
        /// to send the object InvalidInputException to LightController
        /// </summary>
        /// <param name="inputErrors"></param>
        public BusinessValidationException(List<string> inputErrors) : base()
        {
            InputErrors.Clear();
            foreach (string errorCode in inputErrors)
            {
                Critic critic = new Critic();
                critic.AddError(errorCode);
                InputErrors.Add(critic);
            }
        }

        /// <summary>
        /// Building a LightException with detailed data
        /// </summary>
        /// <param name="info">The SerializationInfo holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected BusinessValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
