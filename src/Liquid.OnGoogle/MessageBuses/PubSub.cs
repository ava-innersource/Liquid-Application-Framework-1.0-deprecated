using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Liquid.Base.Interfaces;
using Liquid.Domain;
using Liquid.Activation;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Telemetry;

namespace Liquid.OnGoogle
{
    /// <summary>
    /// Implementation of the communication component between queues of the Google, this class is specific to google
    /// </summary>
    public class PubSub : LightWorker, IWorkbenchService
    {
        private  SubscriberClient subscriberClient;
        private  SubscriptionName _subscriptionName;

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
        private GooglePubSubConfiguration GetConnection<T>(KeyValuePair<MethodInfo, T> item)
        {
            MethodInfo method = item.Key;
            string connectionKey = GetKeyConnection(method);
            GooglePubSubConfiguration config = null;
            if (string.IsNullOrEmpty(connectionKey)) // Load specific settings if provided
            {
                config = LightConfigurator.Config<GooglePubSubConfiguration>($"{nameof(PubSub)}");
            }
            else
            {
                config = LightConfigurator.Config<GooglePubSubConfiguration>($"{nameof(PubSub)}_{connectionKey}");
            }
            return config;
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
                    GooglePubSubConfiguration config = GetConnection(topic);
                    MethodInfo method = GetMethod(topic);
                    string topicName = topic.Value.TopicName;
                    string subscriptName = topic.Value.Subscription;                

                    this._subscriptionName = new SubscriptionName(config.ProjectID, subscriptName);
                    this.subscriberClient = SubscriberClient.CreateAsync(_subscriptionName).Result;

                    //Register Trace on the telemetry 
                    Workbench.Instance.Telemetry.TrackTrace($"Topic {topicName} registered");

                    subscriberClient.StartAsync(async (PubsubMessage message, CancellationToken cancel) => {

                        try
                        {
                            string text = Encoding.UTF8.GetString(message.Data.ToArray());

                            Workbench.Instance.Telemetry.TrackEvent("Method invoked");
                            //Use of the Metrics to monitoring the queue's processes, start the metric
                            Workbench.Instance.Telemetry.BeginMetricComputation("MessageProcessed");
                            //Processing the method defined with queue
                            InvokeProcess(method, Encoding.UTF8.GetBytes(text));
                            Workbench.Instance.Telemetry.ComputeMetric("MessageProcessed", 1);
                            //Finish the monitoring the queue's processes 
                            Workbench.Instance.Telemetry.EndMetricComputation("MessageProcessed");

                            Workbench.Instance.Telemetry.TrackEvent("Method terminated");
                            Workbench.Instance.Telemetry.TrackEvent("Queue's message completed");

                            return await Task.FromResult<SubscriberClient.Reply>(SubscriberClient.Reply.Ack);

                        }
                        catch (Exception exRegister)
                        {
                            //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                            Workbench.Instance.Telemetry.TrackException(exRegister);

                            return await Task.FromResult<SubscriberClient.Reply>(SubscriberClient.Reply.Nack);
                        }                      

                    });                
                    
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
            throw new NotImplementedException();
        }
    }
}
