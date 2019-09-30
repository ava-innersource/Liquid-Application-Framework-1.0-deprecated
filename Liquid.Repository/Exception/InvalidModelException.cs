using Liquid.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Liquid.Domain.Base
{
    /// <summary>
    /// Class indicates the errors on the model layer
    /// </summary>
    [Serializable()]
    public class InvalidModelException : LightException
    {
        public List<Critic> InputErrors { get; } = new List<Critic>();

        public override string Message => "Invalid model. Check the structure of the submitted model. " + JsonConvert.SerializeObject(new { errors = InputErrors });
         
        public InvalidModelException(List<string> inputErrors) : base()
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
        protected InvalidModelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

