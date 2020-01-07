// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Liquid.Interfaces;
using Liquid.Tests;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Liquid.Base.Tests
{
    public class WorkbenchTests
    {
        public WorkbenchTests()
        {
            Workbench.Instance.Reset();
        }

        [Fact]
        public void WhenUseMediaStorageThenGetMediaStorageReturnsSame()
        {
            Workbench.Instance.UseMediaStorage<MockMediaStorage>();

            Assert.IsAssignableFrom<MockMediaStorage>(Workbench.Instance.MediaStorage);
        }

        [Fact]
        public void WhenUseRepositoryThenGetRepositoryReturnsSame()
        {
            Workbench.Instance.UseRepository<MockRepository>();

            Assert.IsAssignableFrom<MockRepository>(Workbench.Instance.Repository);
        }

        [Fact]
        public void WhenUseMessageBusThenGetMessageBusReturnsSame()
        {
            Workbench.Instance.UseMessageBus<MockWorker>();

            // TODO: How to get the message bus? No shortcut?
            // Assert.IsAssignableFrom<MockWorker>(Workbench.Instance.);
        }

        [Fact]
        public void WhenUseTelemetryThenGetTelemetryReturnsSame()
        {
            Workbench.Instance.UseTelemetry<MockTelemetry>();

            Assert.IsAssignableFrom<MockTelemetry>(Workbench.Instance.Telemetry);
        }

        [Fact]
        public void WhenUseEnventHandlerThenGetEventHandlerReturnsSame()
        {
            Workbench.Instance.UseEventHandler<MockEventHandler>();

            Assert.IsAssignableFrom<MockEventHandler>(Workbench.Instance.Event);
        }

        [Fact]
        public void WhenUseCacheThenGetCacheReturnsSame()
        {
            Workbench.Instance.UseCache<MockCache>();

            Assert.IsAssignableFrom<MockCache>(Workbench.Instance.Cache);
        }

        [Fact]
        public void WhenUseLoggerThenGetLoggerReturnsSame()
        {
            Workbench.Instance.UseLogger<MockLogger>();

            Assert.IsAssignableFrom<MockLogger>(Workbench.Instance.Logger);
        }

        [Theory, AutoSubstituteData]
        public void GetRegisteredServiceReturnsWhatWasAddedByAddToCache(WorkbenchServiceType type, IWorkbenchService service)
        {
            Workbench.Instance.AddToCache(type, service);

            Assert.Same(service, Workbench.Instance.GetRegisteredService(type));
        }

        [Theory, AutoSubstituteData]
        public void AddToCacheSameServiceTypeTwiceThrows(WorkbenchServiceType type, IWorkbenchService service1, IWorkbenchService service2)
        {
            Workbench.Instance.AddToCache(type, service1);

            // throws with the same service
            Assert.ThrowsAny<Exception>(() => Workbench.Instance.AddToCache(type, service1));

            // and with a different service
            Assert.ThrowsAny<Exception>(() => Workbench.Instance.AddToCache(type, service2));
        }

        [Theory, AutoSubstituteData]
        public void GetRegisteredServiceCallsInitializeOnService(WorkbenchServiceType type, IWorkbenchService service)
        {
            Workbench.Instance.AddToCache(type, service);

            Workbench.Instance.GetRegisteredService(type);

            service.Received(1).Initialize();
        }

        private class MockMediaStorage : ILightMediaStorage
        {
            public string Conection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string Container { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public Task<ILightAttachment> GetAsync(string resourceId, string id)
            {
                throw new NotImplementedException();
            }

            public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
            {
                throw new NotImplementedException();
            }

            public void Initialize()
            {
                throw new NotImplementedException();
            }

            public Task InsertUpdateAsync(ILightAttachment attachment)
            {
                throw new NotImplementedException();
            }

            public Task Remove(ILightAttachment attachment)
            {
                throw new NotImplementedException();
            }
        }

        private class MockRepository : ILightRepository
        {
            public dynamic AccessConditionOptimistic<T>(T model)
            {
                throw new NotImplementedException();
            }

            public Task<T> AddOrUpdateAsync<T>(T model)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<T>> AddOrUpdateAsync<T>(List<T> listModels)
            {
                throw new NotImplementedException();
            }

            public Task<ILightAttachment> AddOrUpdateAttachmentAsync<T>(string entityId, string fileName, Stream attachment)
            {
                throw new NotImplementedException();
            }

            public Task<int> CountAsync<T>()
            {
                throw new NotImplementedException();
            }

            public Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync<T>(string entityId)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAttachmentAsync<T>(string entityId, string fileName)
            {
                throw new NotImplementedException();
            }

            public Task<IQueryable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            public Task<IQueryable<T>> GetAsync<T>()
            {
                throw new NotImplementedException();
            }

            public Task<ILightAttachment> GetAttachmentAsync<T>(string entityId, string fileName)
            {
                throw new NotImplementedException();
            }

            public Task<T> GetByIdAsync<T>(string entityId)
                where T : new()
            {
                throw new NotImplementedException();
            }

            public Task<ILightPaging<T>> GetByPageAsync<T>(string token, Expression<Func<T, bool>> filter, int page, int itemsPerPage)
            {
                throw new NotImplementedException();
            }

            public Task<ILightPaging<T>> GetByPageAsync<T>(Expression<Func<T, bool>> filter, int page, int itemsPerPage)
            {
                throw new NotImplementedException();
            }

            public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
            {
                throw new NotImplementedException();
            }

            public void Initialize()
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<ILightAttachment>> ListAttachmentsByIdAsync<T>(string entityId)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<T>> QueryAsync<T>(string query)
            {
                throw new NotImplementedException();
            }

            public Task<JObject> QueryAsyncJson<T>(string query)
            {
                throw new NotImplementedException();
            }

            public bool ResetData(string query)
            {
                throw new NotImplementedException();
            }

            public void SetMediaStorage(ILightMediaStorage mediaStorage)
            {
                throw new NotImplementedException();
            }
        }

        private class MockTelemetry : ILightTelemetry
        {
            public void BeginMetricComputation(string metricLabel)
            {
                throw new NotImplementedException();
            }

            public void ComputeMetric(string metricLabel, double value)
            {
                throw new NotImplementedException();
            }

            public void DequeueContext()
            {
                throw new NotImplementedException();
            }

            public void EndMetricComputation(string metricLabel)
            {
                throw new NotImplementedException();
            }

            public void EnqueueContext(string parentID, object value = null, string operationID = "")
            {
                throw new NotImplementedException();
            }

            public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
            {
                throw new NotImplementedException();
            }

            public void Initialize()
            {
                throw new NotImplementedException();
            }

            public void TrackEvent(params object[] events)
            {
                throw new NotImplementedException();
            }

            public void TrackMetric(string metricLabel, double value)
            {
                throw new NotImplementedException();
            }

            public void TrackTrace(params object[] trace)
            {
                throw new NotImplementedException();
            }
        }

        private class MockCache : ILightCache
        {
            public T Get<T>(string key)
            {
                throw new NotImplementedException();
            }

            public Task<T> GetAsync<T>(string key)
            {
                throw new NotImplementedException();
            }

            public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
            {
                throw new NotImplementedException();
            }

            public void Initialize()
            {
                throw new NotImplementedException();
            }

            public void Refresh(string key)
            {
                throw new NotImplementedException();
            }

            public Task RefreshAsync(string key)
            {
                throw new NotImplementedException();
            }

            public void Remove(string key)
            {
                throw new NotImplementedException();
            }

            public Task RemoveAsync(string key)
            {
                throw new NotImplementedException();
            }

            public void Set<T>(string key, T value)
            {
                throw new NotImplementedException();
            }

            public Task SetAsync<T>(string key, T value)
            {
                throw new NotImplementedException();
            }
        }

        private class MockLogger : ILightLogger
        {
            public bool EnabledLogTrafic { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void Debug(string message, object[] args = null)
            {
                throw new NotImplementedException();
            }

            public void Error(Exception exception, string message, object[] args = null)
            {
                throw new NotImplementedException();
            }

            public void Fatal(Exception exception, string message, object[] args = null)
            {
                throw new NotImplementedException();
            }

            public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
            {
                throw new NotImplementedException();
            }

            public void Info(string message, object[] args = null)
            {
                throw new NotImplementedException();
            }

            public void Initialize()
            {
                throw new NotImplementedException();
            }

            public void Trace(string message, object[] args = null)
            {
                throw new NotImplementedException();
            }

            public void Warn(string message, object[] args = null)
            {
                throw new NotImplementedException();
            }
        }

        private class MockWorker : ILightWorker
        {
            public void Initialize()
            {
                throw new NotImplementedException();
            }
        }

        private class MockEventHandler : ILightEvent
        {
            public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
            {
                throw new NotImplementedException();
            }

            public void Initialize()
            {
                throw new NotImplementedException();
            }

            public Task<T> SendToHub<T>(T model, string dataOperation)
            {
                throw new NotImplementedException();
            }
        }
    }
}
