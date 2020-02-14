// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Liquid.Activation;
using Liquid.Domain.Base;
using Liquid.Interfaces;
using Liquid.Tests;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using static Microsoft.Azure.EventHubs.EventData;

namespace Liquid.OnAzure.Tests
{
    public class ServiceBusTests : IDisposable
    {
        private const string QueueConnectionString = "QueueConnection";
        private const string TopicConnectionString = "TopicConnection";

        private readonly Fixture _fixture = new Fixture();

        private readonly ILightTelemetry _telemetry = Substitute.For<ILightTelemetry>();
        private readonly IQueueClientFactory _queueClientFactory = Substitute.For<IQueueClientFactory>();
        private readonly IQueueClient _queueClient = Substitute.For<IQueueClient>();
        private readonly ISubscriptionClientFactory _subscriptionClientFactory = Substitute.For<ISubscriptionClientFactory>();
        private readonly ISubscriptionClient _subscriptionClient = Substitute.For<ISubscriptionClient>();
        private readonly IServiceBusConfigurationProvider _configurationProvider = Substitute.For<IServiceBusConfigurationProvider>();

        private readonly ServiceBus _sut;

        public ServiceBusTests()
        {
            Workbench.Instance.Reset();
            Workbench.Instance.AddToCache(WorkbenchServiceType.Telemetry, _telemetry);

            // ARRANGE IQueueClientFactory
            _queueClientFactory
                .CreateClient(null, null, default(ReceiveMode))
                .ReturnsForAnyArgs(_queueClient);

            // ARRANGE ISubscriptionClientFactory
            _subscriptionClientFactory
                .CreateClient(null, null, null, default(ReceiveMode))
                .ReturnsForAnyArgs(_subscriptionClient);

            // ARRANGE IServiceBusConfigurationProvider
            _configurationProvider
                .GetConfiguration(QueueConnectionString)
                .Returns(_fixture.Create<ServiceBusConfiguration>());

            _configurationProvider
                .GetConfiguration(TopicConnectionString)
                .Returns(_fixture.Create<ServiceBusConfiguration>());

            // ARRANGE ServiceBus
            _sut = new ServiceBus(_queueClientFactory, _subscriptionClientFactory, _configurationProvider);
        }

        [Fact]
        public void InitializeWhenExistsLightWorkerForQueueThenListenerIsCreated()
        {
            // ARRANGE

            // ACT
            _sut.Initialize();

            // ASSERT
            _queueClient
                .Received(1)
                .RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(), Arg.Any<MessageHandlerOptions>());
        }

        [Fact]
        public void InitializeWhenExistsLightWorkerForTopicThenListenerIsCreated()
        {
            // ARRANGE

            // ACT
            _sut.Initialize();

            // ASSERT
            _subscriptionClient
             .Received(1)
             .RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(), Arg.Any<MessageHandlerOptions>());
        }

        [Theory, AutoData]
        public void QueueMessageHandlerWhenMessageIsValidThenNumberOfCallsIsIncremented(string body)
        {
            // ARRANGE
            var before = QueueWorker.NumberOfCalls;

            _queueClient
                .When(_ => _.RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(), Arg.Any<MessageHandlerOptions>()))
                .Do(callbackWithArguments =>
                {
                    var func = callbackWithArguments.Arg<Func<Message, CancellationToken, Task>>();
                    var message = CreateMessageFromString(body);

                    func(message, default(CancellationToken));
                });

            // ACT
            _sut.Initialize();

            // ASSERT
            var after = QueueWorker.NumberOfCalls;

            Assert.Equal(before + 1, after);
        }

        [Theory, AutoData]
        public void TopicMessageHandlerWhenMessageIsValidThenNumberOfCallsIsIncremented(string body)
        {
            // ARRANGE
            var before = TopicWorker.NumberOfCalls;

            _subscriptionClient
                .When(_ => _.RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(), Arg.Any<MessageHandlerOptions>()))
                .Do(callbackWithArguments =>
                {
                    var func = callbackWithArguments.Arg<Func<Message, CancellationToken, Task>>();
                    var message = CreateMessageFromString(body);

                    func(message, default(CancellationToken));
                });

            // ACT
            _sut.Initialize();

            // ASSERT
            var after = TopicWorker.NumberOfCalls;

            Assert.Equal(before + 1, after);
        }

        [Theory, AutoData]
        public void QueueMessageHandlerWhenMethodThrowsBusinessValidationExceptionThenMessageIsDeadLettered(string body)
        {
            // ARRANGE
            QueueWorker.WhatToDo = _ => throw new BusinessValidationException(new List<string>());

            _queueClient
                .When(_ => _.RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(), Arg.Any<MessageHandlerOptions>()))
                .Do(callbackWithArguments =>
                {
                    var func = callbackWithArguments.Arg<Func<Message, CancellationToken, Task>>();
                    var message = CreateMessageFromString(body);

                    func(message, default(CancellationToken)).GetAwaiter().GetResult();
                });

            // ACT
            _sut.Initialize();

            // ASSERT
            _queueClient
                .Received(1)
                .DeadLetterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory, AutoData]
        public void TopicMessageHandlerWhenMethodThrowsBusinessValidationExceptionThenMessageIsDeadLettered(string body)
        {
            // ARRANGE
            TopicWorker.WhatToDo = _ => throw new BusinessValidationException(new List<string>());

            _subscriptionClient
                .When(_ => _.RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(), Arg.Any<MessageHandlerOptions>()))
                .Do(callbackWithArguments =>
                {
                    var func = callbackWithArguments.Arg<Func<Message, CancellationToken, Task>>();
                    var message = CreateMessageFromString(body);

                    func(message, default(CancellationToken)).GetAwaiter().GetResult();
                });

            // ACT
            _sut.Initialize();

            // ASSERT
            _subscriptionClient
                .Received(1)
                .DeadLetterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory, AutoData]
        public void TopicMessageHandlerWhenMethodThrowsInvalidInputExceptionThenMessageIsDeadLettered(string body)
        {
            // ARRANGE
            TopicWorker.WhatToDo = _ => throw new InvalidInputException(new List<string>());

            _subscriptionClient
                .When(_ => _.RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(), Arg.Any<MessageHandlerOptions>()))
                .Do(callbackWithArguments =>
                {
                    var func = callbackWithArguments.Arg<Func<Message, CancellationToken, Task>>();
                    var message = CreateMessageFromString(body);

                    func(message, default(CancellationToken)).GetAwaiter().GetResult();
                });

            // ACT
            _sut.Initialize();

            // ASSERT
            _subscriptionClient
                .Received(1)
                .DeadLetterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                ServiceBusTestFixture.Reset();
            }
        }

        private Message CreateMessageFromString(string body)
        {
            var payload = JsonConvert.SerializeObject(body);
            var message = new Message(Encoding.UTF8.GetBytes(payload));

            // another hack to enable the tests...
            // TODO: Remove once ServiceBus is refactored
            var field = typeof(Message.SystemPropertiesCollection)
                .GetField("sequenceNumber", BindingFlags.NonPublic | BindingFlags.Instance);

            field.SetValue(message.SystemProperties, 1);

            return message;
        }

        [MessageBus(QueueConnectionString)]
        [SuppressMessage(
            "Design",
            "CA1034:Nested types should not be visible",
            Justification = "Used by tests and must be public")]
        public class QueueWorker : LightWorker
        {
            public static int NumberOfCalls { get; private set; } = 0;

            public static Action<string> WhatToDo { get; set; } = message => { };

            [Queue("QueueName")]
            public static void MyQueueMethod(string message)
            {
                NumberOfCalls++;
                WhatToDo(message);
            }
        }

        [MessageBus(TopicConnectionString)]
        [SuppressMessage(
            "Design",
            "CA1034:Nested types should not be visible",
            Justification = "Used by tests and must be public")]
        public class TopicWorker : LightWorker
        {
            public static int NumberOfCalls { get; private set; } = 0;

            public static Action<string> WhatToDo { get; set; } = message => { };

            [Topic("TopicName", "TopicSubscriber")]
            public static void MyTopicMethod(string message)
            {
                NumberOfCalls++;
                WhatToDo(message);
            }
        }

        // To enable tests, we need a reset method
        private class ServiceBusTestFixture : ServiceBus
        {
            /// <summary>
            /// This is a total hack to enable testing. Don't do this at home!
            /// TODO: Remove this hack once the final refactor of <see cref="ServiceBus"/> is done.
            /// </summary>
            public static void Reset()
            {
                _queues.Clear();
                _topics.Clear();
            }
        }
    }
}
