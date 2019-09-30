using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Liquid.Runtime.Configuration.Base
{[Serializable]
    public class InvalidConfigurationException : Exception
    {
        public List<string> InputErrors { get; } = new List<string>();

        public InvalidConfigurationException(string message) : base(message) { }

        public InvalidConfigurationException(List<string> inputErrors) : base(String.Concat(inputErrors))
        {
            InputErrors = inputErrors;
        }

        protected InvalidConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
