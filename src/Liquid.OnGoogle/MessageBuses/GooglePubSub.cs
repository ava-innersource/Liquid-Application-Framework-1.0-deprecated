using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Liquid.Domain;
using Liquid.Runtime.Configuration.Base;

namespace Liquid.OnGoogle.MessageBuses
{
    public class GooglePubSub : MessageBrokerWrapper
    {
        private string ProjectID;
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
            GooglePubSubConfiguration config = null;
            if (string.IsNullOrEmpty(TagConfigName)) // Load specific settings if provided
                config = LightConfigurator.Config<GooglePubSubConfiguration>($"GooglePubSub");
            else
                config = LightConfigurator.Config<GooglePubSubConfiguration>($"GooglePubSub_{TagConfigName}");

            ProjectID = config.ProjectID;
        }

        /// <summary>
        /// Method sender to connect and send the queue to azure.
        /// </summary>
        /// <typeparam name="T">Type of message to send</typeparam>
        /// <param name="message">Object of message to send</param>
        /// <returns>The task of Process topic</returns> 
        public override async Task SendToQueue<T>(T message)
        {
            var _pub = PublisherClient.CreateAsync(new TopicName(ProjectID, _queueName), null, null).Result;
            await _pub.PublishAsync(Google.Protobuf.ByteString.CopyFromUtf8(message.ToStringCamelCase()));
        }

        /// <summary>
        /// Method sender to connect and send the topic to azure.
        /// </summary>
        /// <typeparam name="T">Type of message to send</typeparam>
        /// <param name="message">Object of message to send</param>
        /// <returns>The task of Process topic</returns> 
        public override async Task SendToTopic<T>(T message)
        {
            var _pub = PublisherClient.CreateAsync(new TopicName(ProjectID, _queueName), null, null).Result;
            await _pub.PublishAsync(Google.Protobuf.ByteString.CopyFromUtf8(message.ToStringCamelCase()));
        }
    }
}
