using Liquid.Base;
using Liquid.Interfaces;
using System;
using System.Threading.Tasks;

namespace Liquid.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageBus<T> where T : MessageBrokerWrapper, new()
    {
        private readonly T _process;
        /// <summary>
        /// Initialize the wrapper to comunicate with the message broker
        /// </summary>
        /// <param name="tagConfigName">Configuration Tag Name</param>
        /// <param name="name">Queue or Topic name</param> 
        public MessageBus(string tagConfigName, string name)
        {
            _process = new T();
            _process.Initialize(tagConfigName, name);
        }

        /// <summary>
        /// Method to send a message to broker
        /// </summary>
        /// <typeparam name="U">type of Message</typeparam>
        /// <param name="message">Messade Object</param>
        /// <returns>Task</returns>
        public Task SendToQueue(object message) 
        { 
            return _process.SendToQueue(message);
        }

        /// <summary>
        /// Method to send a message to broker
        /// </summary>
        /// <typeparam name="U">tyep of Message</typeparam>
        /// <param name="message">Messade Object</param>
        /// <returns>Task</returns>
        public Task SendToTopic(object message) 
        {
            return _process.SendToTopic(message);
        }
    }
}
