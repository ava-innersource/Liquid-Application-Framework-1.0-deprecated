// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Liquid.Activation;
using Liquid.Domain;
using Liquid.Domain.Base;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Telemetry;
using Microsoft.Azure.ServiceBus;

namespace Liquid.OnAzure
{
    /// <summary>
    /// Defines an object capable of creating instances of <see cref="IQueueClient"/>.
    /// </summary>
    public interface IQueueClientFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="IQueueClient"/>.
        /// </summary>
        /// <param name="connectionString">The connection string for the client.</param>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="receiveMode">The receive mode that the client will connect to the queue.</param>
        /// <returns>A new instance of <see cref="IQueueClient"/>.</returns>
        IQueueClient CreateClient(string connectionString, string queueName, ReceiveMode receiveMode);
    }

    /// <summary>
    /// Defines an object capable of creating instances of <see cref="ISubscriptionClient"/>.
    /// </summary>
    public interface ISubscriptionClientFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ISubscriptionClient"/>.
        /// </summary>
        /// <param name="connectionString">The connection string for the client.</param>
        /// <param name="topicName">The name of the topic to connect to.</param>
        /// <param name="subscriptionName">Identifies the subscription to this topic.</param>
        /// <param name="receiveMode">The receive mode that the client will connect to the queue.</param>
        /// <returns>A new instance of <see cref="ISubscriptionClient"/>.</returns>
        ISubscriptionClient CreateClient(string connectionString, string topicName, string subscriptionName, ReceiveMode receiveMode);
    }

    /// <summary>
    /// Configuration source for <see cref="ServiceBus"/>.
    /// </summary>
    // TODO: should remove this class once we move to .NET configuration system
    public interface IServiceBusConfigurationProvider
    {
        /// <summary>
        /// Gets the configuration for a <see cref="ServiceBus"/>.
        /// </summary>
        /// <returns>
        /// The current configuration for a service bus.
        /// </returns>
        ServiceBusConfiguration GetConfiguration();

        /// <summary>
        /// Gets the configuration for a <see cref="ServiceBus"/>.
        /// </summary>
        /// <param name="connectionName">
        /// Identifies which connection should be retrieved from the file.
        /// </param>
        /// <returns>
        /// The current configuration for a service bus.
        /// </returns>
        ServiceBusConfiguration GetConfiguration(string connectionName);
    }

    /// <summary>
    /// Implementation of the communication component between queues and topics of the Azure, this class is specific to azure.
    /// </summary>
    public class ServiceBus : LightWorker, IWorkbenchService
    {
        /// <summary>
        /// Factory used to create a <see cref="IQueueClient"/>.
        /// </summary>
        private readonly IQueueClientFactory _queueClientFactory = new DefaultQueueClientFactory();

        /// <summary>
        /// Factory used to create a <see cref="ISubscriptionClient"/>.
        /// </summary>
        private readonly ISubscriptionClientFactory _subscriptionClientFactory = new DefaultSubscriptionClientFactory();

        /// <summary>
        /// Service that retrives a <see cref="ServiceBusConfiguration"/>.
        /// </summary>
        private readonly IServiceBusConfigurationProvider _configurationProvider = new DefaultServiceBusConfigurationProvider();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBus"/> class.
        /// </summary>
        public ServiceBus()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBus"/> class.
        /// </summary>
        /// <param name="queueClientFactory">
        /// Dependency. Used to obtain new instances of a <see cref="IQueueClient"/>.
        /// </param>
        /// <param name="subscriptionClientFactory">
        /// Dependency. Used to obtain new instances of a <see cref="ISubscriptionClient"/>.
        /// </param>
        /// <param name="configurationProvider">
        /// Dependency. Used to retrieve a configuration for this class.
        /// </param>
        public ServiceBus(
            IQueueClientFactory queueClientFactory,
            ISubscriptionClientFactory subscriptionClientFactory,
            IServiceBusConfigurationProvider configurationProvider)
        {
            _queueClientFactory = queueClientFactory ?? throw new ArgumentNullException(nameof(queueClientFactory));
            _subscriptionClientFactory = subscriptionClientFactory ?? throw new ArgumentNullException(nameof(subscriptionClientFactory));
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
        }

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
            var method = item.Key;
            var connectionKey = GetKeyConnection(method);

            ServiceBusConfiguration config;
            if (string.IsNullOrEmpty(connectionKey)) // Load specific settings if provided
            {
                config = _configurationProvider.GetConfiguration();//LightConfigurator.Config<ServiceBusConfiguration>($"{nameof(ServiceBus)}");
            }
            else
            {
                config = _configurationProvider.GetConfiguration(connectionKey);//LightConfigurator.Config<ServiceBusConfiguration>($"{nameof(ServiceBus)}_{connectionKey}");
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
                    ReceiveMode receiveMode = (queue.Value.DeleteAfterRead) ? ReceiveMode.ReceiveAndDelete : ReceiveMode.PeekLock;
                    int takeQuantity = queue.Value.TakeQuantity;

                    //Register Trace on the telemetry 
                    var queueReceiver = _queueClientFactory.CreateClient(GetConnection(queue), queueName, receiveMode);

                    //Register the method to process receive message
                    //The RegisterMessageHandler is validate for all register exist on the queue, without need loop for items
                    queueReceiver.RegisterMessageHandler(
                        async (message, token) =>
                        {
                            try
                            {
                                InvokeProcess(method, message.Body);
                                await queueReceiver.CompleteAsync(message.SystemProperties.LockToken);
                            }
                            catch (Exception exRegister)
                            {
                                Exception moreInfo = new Exception($"Exception reading message from queue {queueName}. See inner exception for details. Message={exRegister.Message}", exRegister);
                                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                                Workbench.Instance.Telemetry.TrackException(moreInfo);

                                //If there is a error , set DeadLetter on register
                                if (queueReceiver.ReceiveMode == ReceiveMode.PeekLock)
                                {
                                    //This operation is only allowed  PeekLock
                                    await queueReceiver.DeadLetterAsync(message.SystemProperties.LockToken,
                                        $"{exRegister.Message}\n {exRegister.InnerException?.Message}", $"{exRegister.StackTrace}");
                                }
                            }
                        },
                        new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = takeQuantity, AutoComplete = false });
                }
            }
            catch (Exception exception)
            {
                Exception moreInfo = new Exception($"Error setting up queue consumption from service bus. See inner exception for details. Message={exception.Message}", exception);
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                Workbench.Instance.Telemetry.TrackException(moreInfo);
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
                    ReceiveMode receiveMode = ReceiveMode.PeekLock;

                    if (topic.Value.DeleteAfterRead)
                    {
                        receiveMode = ReceiveMode.ReceiveAndDelete;
                    }

                    int takeQuantity = topic.Value.TakeQuantity;

                    //Register Trace on the telemetry 
                    var subscriptionClient = _subscriptionClientFactory.CreateClient(
                        GetConnection(topic), topicName, subscriptName, receiveMode);

                    //Register the method to process receive message
                    //The RegisterMessageHandler is validate for all register exist on the queue, without need loop for items
                    subscriptionClient.RegisterMessageHandler(
                        async (message, cancellationToken) =>
                        {
                            try
                            {
                                InvokeProcess(method, message.Body);
                                await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                            }
                            catch (Exception exRegister)
                            {
                                Exception moreInfo = new Exception($"Exception reading message from topic {topicName} and subscriptName {subscriptName}. See inner exception for details. Message={exRegister.Message}", exRegister);
                                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                                Workbench.Instance.Telemetry.TrackException(moreInfo);

                                var exceptionDetails = $"{exRegister.Message}";

                                //If there is a business error or a invlida input, set DeadLetter on register
                                if (subscriptionClient.ReceiveMode == ReceiveMode.PeekLock)
                                {
                                    if (exRegister.InnerException != null)
                                    {
                                        exceptionDetails = $"{exceptionDetails} \n {exRegister.InnerException?.Message}";

                                        if (exRegister.InnerException is InvalidInputException)
                                        {
                                            var inputErrors = (exRegister.InnerException as InvalidInputException).InputErrors;

                                            string jsonString = (new { critics = inputErrors }).ToStringCamelCase();
                                            exceptionDetails = $"{exceptionDetails} \n {jsonString}";

                                            //This operation is only allowed  PeekLock
                                            await subscriptionClient.DeadLetterAsync(message.SystemProperties.LockToken,
                                            exceptionDetails, $"{exRegister.StackTrace}");
                                        }

                                        if (exRegister.InnerException is BusinessValidationException)
                                        {
                                            var inputErrors = (exRegister.InnerException as BusinessValidationException).InputErrors;

                                            string jsonString = (new { critics = inputErrors }).ToStringCamelCase();
                                            exceptionDetails = $"{exceptionDetails} \n {jsonString}";

                                            //This operation is only allowed  PeekLock
                                            await subscriptionClient.DeadLetterAsync(message.SystemProperties.LockToken,
                                            exceptionDetails, $"{exRegister.StackTrace}");
                                        }
                                    }
                                }
                            }

                        }, new MessageHandlerOptions((e) => ExceptionReceivedHandler(e)) { AutoComplete = false, MaxConcurrentCalls = takeQuantity });
                }
            }
            catch (Exception exception)
            {
                Exception moreInfo = new Exception($"Error setting up subscription consumption from service bus. See inner exception for details. Message={exception.Message}", exception);
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                Workbench.Instance.Telemetry.TrackException(moreInfo);
            }
        }

        protected override Task ProcessAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Default implementation for <see cref="IQueueClientFactory"/>,
        /// creates instances of <see cref="IQueueClient"/>.
        /// </summary>
        private class DefaultQueueClientFactory : IQueueClientFactory
        {
            /// <inheritdoc/>
            public IQueueClient CreateClient(string connectionString, string queueName, ReceiveMode receiveMode)
            {
                return new QueueClient(connectionString, queueName, receiveMode);
            }
        }

        /// <summary>
        /// Default implementation for <see cref="ISubscriptionClientFactory"/>,
        /// creates instances of <see cref="SubscriptionClient"/>.
        /// </summary>
        private class DefaultSubscriptionClientFactory : ISubscriptionClientFactory
        {
            /// <inheritdoc/>
            public ISubscriptionClient CreateClient(string connectionString, string topicName, string subscriptionName, ReceiveMode mode)
            {
                return new SubscriptionClient(connectionString, topicName, subscriptionName, mode, null);
            }
        }

        /// <summary>
        /// Retrieves configuration using <see cref="LightConfigurator"/>.
        /// </summary>
        private class DefaultServiceBusConfigurationProvider : IServiceBusConfigurationProvider
        {
            /// <inheritdoc/>
            public ServiceBusConfiguration GetConfiguration()
            {
                return LightConfigurator.Config<ServiceBusConfiguration>($"{nameof(ServiceBus)}");
            }

            /// <inheritdoc/>
            public ServiceBusConfiguration GetConfiguration(string connectionKey)
            {
                return LightConfigurator.Config<ServiceBusConfiguration>($"{nameof(ServiceBus)}_{connectionKey}");
            }
        }
    }
}
