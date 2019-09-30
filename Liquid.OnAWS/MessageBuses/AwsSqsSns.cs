using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Liquid.Base.Interfaces;
using Liquid.Activation;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Telemetry;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.OnAWS
{
    /// <summary>
    /// Implementation of the communication component between queues and topics of the Azure, this class is specific to azure
    /// </summary>
    public class AwsSqsSns : LightWorker, IWorkBenchHealthCheck
    {
        /// <summary>
        /// Implementation of the start process queue and process topic. It must be called  parent before start processes.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Implementation of connection service bus
        /// </summary>
        /// <typeparam name="T">Type of the queue or topic</typeparam>
        /// <param name="item">Item queue or topic</param>
        /// <returns>Config Connection</returns>
        private SqsSnsConfiguration GetConnection<T>(KeyValuePair<MethodInfo, T> item)
        {
            MethodInfo method = item.Key;
            string connectionKey = GetKeyConnection(method);
            SqsSnsConfiguration config = null;
            if (string.IsNullOrEmpty(connectionKey)) // Load specific settings if provided
            {
                config = LightConfigurator.Config<SqsSnsConfiguration>($"{nameof(SqsSns)}");
            }
            else
            {
                config = LightConfigurator.Config<SqsSnsConfiguration>($"{nameof(SqsSns)}_{connectionKey}");
            }
            return config;
        }

        /// <summary>
        /// Process the queue defined on the Amazon  
        /// </summary>
        /// <returns>The task of Process queue</returns> 
        public void ProcessQueue()
        {
            try
            {
                foreach (var queue in _queues)
                {
                    SqsSnsConfiguration config = GetConnection(queue);
                    MethodInfo method = GetMethod(queue);
                    string queueName = queue.Value.QueueName;
                    int takeQuantity = queue.Value.TakeQuantity;

                    //Register Trace on the telemetry 
                    WorkBench.Telemetry.TrackTrace($"Queue {queueName} registered");
                    AmazonSQSClient sqsClient = new AmazonSQSClient(config.AwsAccessKeyId, config.AwsSecretAccessKey);
                    string queueURL = sqsClient.CreateQueueAsync(new CreateQueueRequest
                    {
                        QueueName = queueName
                    }).Result.QueueUrl;

                    ReceiveMessageResponse queueReceiveMessageResponse =   sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest()
                    {
                        QueueUrl = queueURL,
                        MaxNumberOfMessages = takeQuantity
                    }).Result;

                    List<Message> messagesList = new List<Message>();
                    messagesList = queueReceiveMessageResponse.Messages;
                    foreach (Message message in messagesList)
                    {
                        try
                        {
                            WorkBench.Telemetry.TrackEvent("Method invoked");
                            //Use of the Metrics to monitoring the queue's processes, start the metric
                            WorkBench.Telemetry.BeginMetricComputation("MessageProcessed");
                            //Processing the method defined with queue
                            InvokeProcess(method, Encoding.UTF8.GetBytes(message.Body));
                            WorkBench.Telemetry.ComputeMetric("MessageProcessed", 1);
                            //Finish the monitoring the queue's processes 
                            WorkBench.Telemetry.EndMetricComputation("MessageProcessed");

                            WorkBench.Telemetry.TrackEvent("Method terminated");
                            WorkBench.Telemetry.TrackEvent("Queue's message completed");
                        }
                        catch (Exception exRegister)
                        {
                            //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                            ((LightTelemetry)WorkBench.Telemetry).TrackException(exRegister);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                ((LightTelemetry)WorkBench.Telemetry).TrackException(exception);
            }
        }
        /// <summary>
        /// Method created to connect and process the Topic/Subscription in the AWS.
        /// </summary>
        /// <returns></returns>
        public void ProcessSubscription()
        {
            try
            {
                foreach (var topic in _topics)
                {
                    SqsSnsConfiguration config = GetConnection(topic);
                    MethodInfo method = GetMethod(topic);
                    string topicName = topic.Value.TopicName;
                    string subscriptName = topic.Value.Subscription;

                    //Register Trace on the telemetry 
                    WorkBench.Telemetry.TrackTrace($"Topic {topicName} registered");
                    AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(config.AwsAccessKeyId, config.AwsSecretAccessKey); 
                    var topicRequest = new CreateTopicRequest
                    {
                        Name = topicName
                    };
                    var topicResponse = snsClient.CreateTopicAsync(topicRequest).Result; 
                    var subsRequest = new ListSubscriptionsByTopicRequest
                    {
                        TopicArn = topicResponse.TopicArn
                    };

                    var subs = snsClient.ListSubscriptionsByTopicAsync(subsRequest).Result.Subscriptions;  
                    foreach (Subscription subscription in subs)
                    {
                        try
                        {
                            WorkBench.Telemetry.TrackEvent("Method invoked");
                            //Use of the Metrics to monitoring the queue's processes, start the metric
                            WorkBench.Telemetry.BeginMetricComputation("MessageProcessed");
                            //Processing the method defined with queue
                            InvokeProcess(method, Encoding.UTF8.GetBytes(subscription.SubscriptionArn));
                            WorkBench.Telemetry.ComputeMetric("MessageProcessed", 1);
                            //Finish the monitoring the queue's processes 
                            WorkBench.Telemetry.EndMetricComputation("MessageProcessed");

                            WorkBench.Telemetry.TrackEvent("Method terminated");
                            WorkBench.Telemetry.TrackEvent("Queue's message completed");
                        }
                        catch (Exception exRegister)
                        {
                            //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                            ((LightTelemetry)WorkBench.Telemetry).TrackException(exRegister);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                ((LightTelemetry)WorkBench.Telemetry).TrackException(exception);
            }
        }
        protected override Task ProcessAsync()
        {
            ProcessQueue();
            ProcessSubscription();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method to run light work Health Check for AWS 
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                if (_queues.Count > 0)
                {
                    var queueEnum = _queues.GetEnumerator();
                    var queue = queueEnum.Current;

                    SqsSnsConfiguration config = GetConnection(queue);
                    MethodInfo method = GetMethod(queue);
                    string queueName = queue.Value.QueueName;
                    int takeQuantity = queue.Value.TakeQuantity;
                    AmazonSQSClient sqsClient = new AmazonSQSClient(config.AwsAccessKeyId, config.AwsSecretAccessKey);
                    sqsClient.ListQueuesAsync("healthQueue");
                }

                if (_topics.Count > 0)
                {
                    var topicEnum = _topics.GetEnumerator();
                    var topic = topicEnum.Current;
                    SqsSnsConfiguration config = GetConnection(topic);
                    MethodInfo method = GetMethod(topic);
                    string topicName = topic.Value.TopicName;
                    string subscriptName = topic.Value.Subscription;

                    AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(config.AwsAccessKeyId, config.AwsSecretAccessKey);
                    snsClient.ListTopicsAsync("healthTopic");
                }

                return LightHealth.HealthCheck.Healthy;
            }
            catch 
            {
                return LightHealth.HealthCheck.Unhealthy;
            }               
        }
    }
}
