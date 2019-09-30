using Liquid.Interfaces;
using System.Threading.Tasks;

namespace Liquid.Domain
{
    /// <summary>
    /// Wrapper class to instatiate a message broker implementation
    /// Helping to enable a smooth integration
    /// </summary>
    public class MessageBrokerWrapper : IMessageBrokerWrapper
    {
        public string TagConfigName { get; private set; }
        public string Name { get; private set; }
        /// <summary>
        /// Inicialize the class with set Config Name, Queue Name  
        /// </summary>
        /// <param name="tagConfigName"> Config Name </param>
        /// <param name="name">Name</param> 
        public virtual void Initialize(string tagConfigName, string name)
        {
            TagConfigName = tagConfigName;
            Name = name;
        }

        /// <summary>
        /// Send a message to a queue or topic
        /// </summary>
        /// <typeparam name="T">type of message</typeparam>
        /// <param name="message">message</param>
        /// <returns>A Task that completes when the middleware has completed processing.</returns>
        public virtual Task SendToQueue<T>(T message) where T : ILightMessage { return Task.FromResult(0); }

        /// <summary>
        /// Send a message to a topic
        /// </summary>
        /// <typeparam name="T">type of message</typeparam>
        /// <param name="message">message</param>
        /// <returns>A Task that completes when the middleware has completed processing.</returns>
        public virtual Task SendToTopic<T>(T message) where T : ILightMessage { return Task.FromResult(0); }
    }
}
