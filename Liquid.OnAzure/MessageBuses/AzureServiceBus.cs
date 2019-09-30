using Liquid.Domain;
using Liquid.Runtime.Configuration.Base;
using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.OnAzure
{
    /// <summary>
    /// Implementation of the communication component between queues of the Azure, this class is specific to azure
    /// </summary>
    public class AzureServiceBus : MessageBrokerWrapper
    {
        private string ConnectionString;
        private string _queueName;
        /// <summary>
        /// Inicialize the class with set Config Name and Queue Name, must called the parent method
        /// </summary>
        /// <param name="tagConfigName"> Config Name </param>
        /// <param name="name">Queue Name</param> 
        public override void Initialize(string tagConfigName, string name)
        {
            base.Initialize(tagConfigName, name);
            _queueName = name;
            SetConnection(tagConfigName);
        }

        /// <summary>
        /// Get connection settings
        /// </summary>
        /// <param name="TagConfigName"></param>
        private void SetConnection(string TagConfigName)
        {
            MessageBrokerConfiguration config = null;
            if (string.IsNullOrEmpty(TagConfigName)) // Load specific settings if provided
                config = LightConfigurator.Config<MessageBrokerConfiguration>($"{nameof(TagConfigName)}");
            else
                config = LightConfigurator.Config<MessageBrokerConfiguration>($"ServiceBus_{TagConfigName}");

            ConnectionString = config.ConnectionString;
        }

        /// <summary>
        /// Method sender to connect and send the queue to azure.
        /// </summary>
        /// <typeparam name="T">Type of message to send</typeparam>
        /// <param name="message">Object of message to send</param>
        /// <returns>The task of Process topic</returns> 
        public override async Task SendToQueue<T>(T message)
        {
            QueueClient queueClient;
            queueClient = new QueueClient(ConnectionString, _queueName);
            byte[] messageSerialize = Encoding.UTF8.GetBytes(message.ToStringCamelCase());
            var messageData = new Message(messageSerialize)
            {
                ContentType = "application/json;charset=utf-8",
                Label = typeof(T).ToString(),
                MessageId = Guid.NewGuid().ToString(),
                TimeToLive = TimeSpan.FromMinutes(2)
            };

            await queueClient.SendAsync(messageData);
        }

        /// <summary>
        /// Method sender to connect and send the topic to azure.
        /// </summary>
        /// <typeparam name="T">Type of message to send</typeparam>
        /// <param name="message">Object of message to send</param>
        /// <returns>The task of Process topic</returns> 
        public override async Task SendToTopic<T>(T message)
        {
            TopicClient topicClient;
            topicClient = new TopicClient(ConnectionString, _queueName);
            var messageData = new Message(Encoding.UTF8.GetBytes(message.ToStringCamelCase()))
            {
                ContentType = "application/json;charset=utf-8",
                Label = typeof(T).ToString(),
                MessageId = Guid.NewGuid().ToString(),
                TimeToLive = TimeSpan.FromMinutes(2)
            };

            await topicClient.SendAsync(messageData);
        }
    }
}
