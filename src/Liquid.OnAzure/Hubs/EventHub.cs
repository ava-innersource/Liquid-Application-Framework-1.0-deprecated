using Liquid.Base;
using Liquid.Base.Interfaces;
using Liquid.Activation;
using Liquid.Runtime;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Telemetry;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.OnAzure.Hubs
{
    public class EventHub : LightWorker, IWorkbenchService
    {

        HubConfiguration config = null;


        /// <summary>
        /// Implementation of the start process queue and process topic. It must be called  parent before start processes.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            ProcessHub();
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

            if (string.IsNullOrEmpty(connectionKey)) // Load specific settings if provided
                config = LightConfigurator.Config<HubConfiguration>($"{nameof(EventHub)}");
            else
            {
                config = LightConfigurator.Config<HubConfiguration>($"{nameof(EventHub)}_{connectionKey}");
            }

            return config.ConnectionString;
        }

        /// <summary>
        /// If  an error occurs in the processing, this method going to called
        /// </summary>
        /// <param name="exceptionReceivedEventArgs">The Exception Received Event</param>
        /// <returns>The task of processs</returns>
        public Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
            Workbench.Instance.Telemetry.TrackException(exceptionReceivedEventArgs.Exception);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method created to connect and process the Topic/Subscription in the azure.
        /// </summary>
        /// <returns></returns>
        private void ProcessHub()
        {
            if (_topics.Count != 1)
            {
                throw new LightException($"EventHub implementation must have one Entity Model related!");
            }

            var e = _topics.GetEnumerator();
            e.MoveNext();
            var topic = e.Current;

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(GetConnection(topic))
            {
                EntityPath = topic.Value.TopicName
            };

            EventHubClient client = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            PartitionReceiver receiver = client.CreateReceiver(topic.Value.Subscription, "0", EventPosition.FromStart(), null);
            IEnumerable<EventData> receivedEvents = receiver.ReceiveAsync(topic.Value.TakeQuantity).Result;
            try
            {
                while (true)
                {
                    if (receivedEvents != null)
                    {
                        foreach (EventData receivedEvent in receivedEvents)
                        {
                            LightWorker.InvokeProcess(topic.Key, receivedEvent.Body.ToArray());
                        }
                    }
                    receivedEvents = receiver.ReceiveAsync(topic.Value.TakeQuantity).Result;
                }
            }
            catch (Exception exception)
            {
                Exception moreInfo = new Exception($"Exception reading topic={topic.Value.TopicName} with subscription={topic.Value.Subscription} from event hub. See inner exception for details. Message={exception.Message}", exception);

                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                Workbench.Instance.Telemetry.TrackException(moreInfo);
            }
        }

        protected override Task ProcessAsync()
        {
            throw new NotImplementedException();
        }
    }
}
