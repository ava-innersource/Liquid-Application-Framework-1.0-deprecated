using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Liquid.Interfaces;
using Newtonsoft.Json.Linq;
using Xunit;
using NSubstitute;

namespace Liquid.Base.Tests
{
    public class WorkbenchTests
    {
        public WorkbenchTests()
        {
            WorkBench.Reset();
        }

        [Fact]
        public void WhenUseMediaStorageThenGetMediaStorageReturnsSame()
        {
            WorkBench.UseMediaStorage<MockMediaStorage>();

            Assert.IsAssignableFrom<MockMediaStorage>(WorkBench.MediaStorage);
        }

        [Fact]
        public void WhenUseRepositoryThenGetRepositoryReturnsSame()
        {
            WorkBench.UseRepository<MockRepository>();

            Assert.IsAssignableFrom<MockRepository>(WorkBench.Repository);
        }

        [Fact]
        public void WhenUseMessageBusThenGetMessageBusReturnsSame()
        {
            WorkBench.UseMessageBus<MockWorker>();

            // TODO: How to get the message bus? No shortcut?
            // Assert.IsAssignableFrom<MockWorker>(WorkBench.);
        }

        [Fact]
        public void WhenUseTelemetryThenGetTelemetryReturnsSame()
        {
            WorkBench.UseTelemetry<MockTelemetry>();

            Assert.IsAssignableFrom<MockTelemetry>(WorkBench.Telemetry);
        }

        [Fact]
        public void WhenUseEnventHandlerThenGetEventHandlerReturnsSame()
        {
            WorkBench.UseEnventHandler<MockEventHandler>();

            Assert.IsAssignableFrom<MockEventHandler>(WorkBench.Event);
        }

        [Fact]
        public void WhenUseCacheThenGetCacheReturnsSame()
        {
            WorkBench.UseCache<MockCache>();

            Assert.IsAssignableFrom<MockCache>(WorkBench.Cache);
        }

        [Fact]
        public void WhenUseLoggerThenGetLoggerReturnsSame()
        {
            WorkBench.UseLogger<MockLogger>();

            Assert.IsAssignableFrom<MockLogger>(WorkBench.Logger);
        }

        [Theory, AutoSubstituteData]
        public void GetRegisteredServiceReturnsWhatWasAddedByAddToCache(WorkBenchServiceType type, IWorkBenchService service)
        {
            WorkBench.AddToCache(type, service);

            Assert.Same(service, WorkBench.GetRegisteredService(type));
        }

        [Theory, AutoSubstituteData]
        public void AddToCacheSameServiceTypeTwiceThrows(WorkBenchServiceType type, IWorkBenchService service1, IWorkBenchService service2)
        {
            WorkBench.AddToCache(type, service1);

            // throws with the same service
            Assert.ThrowsAny<Exception>(() => WorkBench.AddToCache(type, service1));

            // and with a different service
            Assert.ThrowsAny<Exception>(() => WorkBench.AddToCache(type, service2));
        }

        [Theory, AutoSubstituteData]
        public void GetRegisteredServiceCallsInitializeOnService(WorkBenchServiceType type, IWorkBenchService service)
        {
            WorkBench.AddToCache(type, service);

            WorkBench.GetRegisteredService(type);

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

            public Task<T> GetByIdAsync<T>(string entityId) where T : new()
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
