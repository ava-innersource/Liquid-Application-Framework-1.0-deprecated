using Liquid.Activation;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Liquid.OnAzure.IntegrationTests.MessageBuses
{
    public class ServiceBusIntegrationTest : IDisposable
    {
        readonly ServiceBus _serviceBus;
        
        public ServiceBusIntegrationTest()
        {
            _serviceBus = new ServiceBus();

        }

        [Fact]
        public void ServiceBusWhenThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => _serviceBus.ProcessQueue());
        }

        [Fact]
        public void InitializeWhenThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => _serviceBus.Initialize());
        }

        [Fact]
        public void ProcessSubscriptionWhenThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => _serviceBus.ProcessSubscriptione());
        }

        public void Dispose()
        {
           
        }
    }
}
