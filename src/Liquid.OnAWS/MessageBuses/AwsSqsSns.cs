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
    public class AwsSqsSns : LightWorker, IWorkbenchService
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
                    Workbench.Instance.Telemetry.TrackTrace($"Queue {queueName} registered");
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
                            Workbench.Instance.Telemetry.TrackEvent("Method invoked");
                            //Use of the Metrics to monitoring the queue's processes, start the metric
                            Workbench.Instance.Telemetry.BeginMetricComputation("MessageProcessed");
                            //Processing the method defined with queue
                            InvokeProcess(method, Encoding.UTF8.GetBytes(message.Body));
                            Workbench.Instance.Telemetry.ComputeMetric("MessageProcessed", 1);
                            //Finish the monitoring the queue's processes 
                            Workbench.Instance.Telemetry.EndMetricComputation("MessageProcessed");

                            Workbench.Instance.Telemetry.TrackEvent("Method terminated");
                            Workbench.Instance.Telemetry.TrackEvent("Queue's message completed");
                        }
                        catch (Exception exRegister)
                        {
                            //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                            Workbench.Instance.Telemetry.TrackException(exRegister);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                Workbench.Instance.Telemetry.TrackException(exception);
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
                    Workbench.Instance.Telemetry.TrackTrace($"Topic {topicName} registered");
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
                            Workbench.Instance.Telemetry.TrackEvent("Method invoked");
                            //Use of the Metrics to monitoring the queue's processes, start the metric
                            Workbench.Instance.Telemetry.BeginMetricComputation("MessageProcessed");
                            //Processing the method defined with queue
                            InvokeProcess(method, Encoding.UTF8.GetBytes(subscription.SubscriptionArn));
                            Workbench.Instance.Telemetry.ComputeMetric("MessageProcessed", 1);
                            //Finish the monitoring the queue's processes 
                            Workbench.Instance.Telemetry.EndMetricComputation("MessageProcessed");

                            Workbench.Instance.Telemetry.TrackEvent("Method terminated");
                            Workbench.Instance.Telemetry.TrackEvent("Queue's message completed");
                        }
                        catch (Exception exRegister)
                        {
                            //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                            Workbench.Instance.Telemetry.TrackException(exRegister);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                Workbench.Instance.Telemetry.TrackException(exception);
            }
        }

        protected override Task ProcessAsync()
        {
            ProcessQueue();
            ProcessSubscription();
            return Task.CompletedTask;
        }
    }
}
