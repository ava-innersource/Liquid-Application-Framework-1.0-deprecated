using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Liquid.Domain;
using Liquid.Runtime.Configuration.Base;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.OnAWS
{
    /// <summary>
    /// Implementation of the communication component between queues of the Azure, this class is specific to azure
    /// </summary>
    public class SqsSns : MessageBrokerWrapper
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
            GetConnection(tagConfigName);
        }

        /// <summary>
        /// Get connection settings
        /// </summary>
        /// <param name="TagConfigName"></param>
        private SqsSnsConfiguration GetConnection(string TagConfigName)
        {
            SqsSnsConfiguration config = null;
            if (string.IsNullOrEmpty(TagConfigName)) // Load specific settings if provided
                config = LightConfigurator.Config<SqsSnsConfiguration>($"{nameof(TagConfigName)}");
            else
                config = LightConfigurator.Config<SqsSnsConfiguration>($"SqsSns_{TagConfigName}");

            return config;
        }

        /// <summary>
        /// Method sender to connect and send the queue to azure.
        /// </summary>
        /// <typeparam name="T">Type of message to send</typeparam>
        /// <param name="message">Object of message to send</param>
        /// <returns>The task of Process topic</returns> 
        public override async Task SendToQueue(object message)
        {
            SqsSnsConfiguration config = GetConnection(_queueName);
            AmazonSQSClient sqsClient = new AmazonSQSClient(config.AwsAccessKeyId, config.AwsSecretAccessKey);

            string queueURL = sqsClient.CreateQueueAsync(new CreateQueueRequest
            {
                QueueName = _queueName
            }).Result.QueueUrl;
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueURL,
                MessageBody = message.ToStringCamelCase(),
                MessageGroupId = Guid.NewGuid().ToString("N"),
                MessageDeduplicationId = Guid.NewGuid().ToString("N")
            };
            var sendMessageResponse = await sqsClient.SendMessageAsync(sendMessageRequest);

        }

        /// <summary>
        /// Method sender to connect and send the topic to azure.
        /// </summary>
        /// <typeparam name="T">Type of message to send</typeparam>
        /// <param name="message">Object of message to send</param>
        /// <returns>The task of Process topic</returns> 
        public override async Task SendToTopic(object message)
        {
            SqsSnsConfiguration config = GetConnection(_queueName);
            AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient();
            var topicRequest = new CreateTopicRequest
            {
                Name = _queueName
            };
            var topicResponse = snsClient.CreateTopicAsync(topicRequest).Result;
            await snsClient.PublishAsync(new PublishRequest
            {
                TopicArn = topicResponse.TopicArn,
                Message = message.ToStringCamelCase(),
            });
        }
    }
}
