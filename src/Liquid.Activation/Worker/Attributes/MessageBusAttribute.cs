using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Activation
{
    /// <summary>
    /// Attribute used for connect a Service Bus (Queue / Topic).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageBusAttribute : Attribute
    {
        /// <summary>
        /// Private configuration name.
        /// </summary>
        private readonly string _config;



        /// <summary>
        /// Constructor used to inform a Configuration Tag Name.
        /// </summary>
        /// <param name="configTagName">Configuration Tag Name</param>
        public MessageBusAttribute(string configTagName)
        {
            _config = configTagName;
        }


        /// <summary>
        /// Get a Configuration Tag Name.
        /// </summary>
        public virtual string ConfigTagName
        {
            get { return _config; }
        }

    }
}
