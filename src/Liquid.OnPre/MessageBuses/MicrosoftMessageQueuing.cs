using Liquid.Base.Interfaces;
using Liquid.Domain;
using Liquid.Domain.Base;
using Liquid.Activation;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Telemetry; 
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.OnWindowsClient
{
    /// <summary>
    /// Implementation of the communication component between queues and topics of the Azure, this class is specific to azure
    /// </summary>
    public class MicrosoftMessageQueuing : LightWorker 
    {
        /// <summary>
        /// Implementation of the start process queue and process topic. It must be called  parent before start processes.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            ProcessQueue();
            ProcessSubscription();
        }

        /// <summary>
        /// Implementation of connection service bus
        /// </summary>
        /// <typeparam name="T">Type of the queue or topic</typeparam>
        /// <param name="item">Item queue or topic</param>
        /// <returns>StringConnection of the ServiceBus</returns>
        private string GetConnection<T>(KeyValuePair<MethodInfo, T> item)
        {
            MethodInfo method = item.Key;
            string connectionKey = GetKeyConnection(method);
            MicrosoftMessageQueuingConfiguration config = null;
            if (string.IsNullOrEmpty(connectionKey)) // Load specific settings if provided
            {
                config = LightConfigurator.Config<MicrosoftMessageQueuingConfiguration>($"{nameof(MicrosoftMessageQueuing)}");
            }
            else
            {
                config = LightConfigurator.Config<MicrosoftMessageQueuingConfiguration>($"{nameof(MicrosoftMessageQueuing)}_{connectionKey}");
            }

            return config.ConnectionString;
        }



        /// <summary>
        /// Process the queue defined on the Azure 
        /// </summary>
        /// <returns>The task of Process Queue</returns> 
        public void ProcessQueue()
        {
            try
            {
                foreach (var queue in _queues)
                {
                    MethodInfo method = GetMethod(queue);
                    string queueName = queue.Value.QueueName;
                    int takeQuantity = queue.Value.TakeQuantity;

                    throw new NotImplementedException();
                }
            }
            catch (Exception exception)
            {
                Exception moreInfo = new Exception($"Error setting up queue consumption from service bus. See inner exception for details. Message={exception.Message}", exception);
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                ((LightTelemetry)WorkBench.Telemetry).TrackException(moreInfo);
            }
        }

        /// <summary>
        /// Method created to connect and process the Topic/Subscription in the azure.
        /// </summary>
        /// <returns></returns>
        private void ProcessSubscription()
        {
            try
            {
                foreach (var topic in _topics)
                {
                    MethodInfo method = GetMethod(topic);
                    string topicName = topic.Value.TopicName;
                    string subscriptName = topic.Value.Subscription;
                    int takeQuantity = topic.Value.TakeQuantity;

                    throw new NotImplementedException();

                }
            }
            catch (Exception exception)
            {
                Exception moreInfo = new Exception($"Error setting up subscription consumption from service bus. See inner exception for details. Message={exception.Message}", exception);
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                ((LightTelemetry)WorkBench.Telemetry).TrackException(moreInfo);
            }
        }

        protected override Task ProcessAsync()
        {
            throw new NotImplementedException();
        }

    }
}
