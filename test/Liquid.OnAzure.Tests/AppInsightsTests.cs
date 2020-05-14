// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Liquid.Interfaces;
using NSubstitute;
using Xunit;

namespace Liquid.OnAzure.Tests
{
    public class AppInsightsTests : IDisposable
    {
        //private const string ContentType = "text/plain";
        //private const string DefaultConnectionString = "UseDevelopmentStorage=true";
        //private const string DefaultContainerName = "removecontainer";
        //private static readonly IFixture _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

        private static readonly ILightTelemetry _fakeLightTelemetry = Substitute.For<ILightTelemetry>();

        //readonly string _expectedData;

        private readonly Stream _stream;

        private readonly ILightAttachment _lightAttachment;
        private AppInsightsConfiguration _appInsightConfiguration;
        private readonly AppInsights _sut;

        private readonly object[] _nullObjectArray;

        //parametro do teste TrackMetric
        private readonly string _metricLabel;

        private readonly double _value;

        //parametro do teste TrackMetric
        private readonly object[] _nullObjectArrayTrace;

        //parametro do teste TrackAggregateMetric
        private readonly object nullObjectArrayTrace;

        //parametro do teste TrackException
        private readonly Exception _exception;

        //parametro do teste EnqueueContext
        private readonly string _parentID;
        private readonly object _valueEnqueueContext;
        private readonly string _operationID;

        public AppInsightsTests()
        {
            Workbench.Instance.Reset();

            Workbench.Instance.AddToCache(WorkbenchServiceType.Telemetry, _fakeLightTelemetry);

            _appInsightConfiguration = new AppInsightsConfiguration
            {
                EnableKubernetes = false,
                //InstrumentationKey = "qualquer",
            };

            _sut = new AppInsights(_appInsightConfiguration);
            //_sut.Initialize()

            //_expectedData = _fixture.Create<string>();
        }

        /*[Fact]
        public void CtorWhenConfigurationIsNullThrows()
        {
            Assert.ThrowsAny<Exception>(() => new AppInsights(null));
        }*/

        [Fact]
        public void TrackEventIsNullThrows()
        {
            Assert.ThrowsAny<NullReferenceException>(() => _sut.TrackEvent(_nullObjectArray));
        }

        //Unit test Track Metric
        [Fact]
        public void TrackMetricIsNullThrows()
        {
            Assert.ThrowsAny<NullReferenceException>(() => _sut.TrackMetric(_metricLabel, _value));
        }

        //Unit test Track Trace
        [Fact]
        public void TrackTraceIsNullThrows()
        {
            Assert.ThrowsAny<NullReferenceException>(() => _sut.TrackTrace(_nullObjectArrayTrace));
        }

        //Unit test Track Aggregate Metric
        [Fact]
        public void TrackAggregateMetricIsNullThrows()
        {
            Assert.ThrowsAny<NullReferenceException>(() => _sut.TrackAggregateMetric(nullObjectArrayTrace));
        }

        //Unit test Track Exception
        [Fact]
        public void TrackExceptionIsNullThrows()
        {
            Assert.ThrowsAny<NullReferenceException>(() => _sut.TrackException(_exception));
        }

        //Unit test Initialize
        [Fact]
        public void AppInitializeIsNullThrows()
        {
            _appInsightConfiguration = null;
            Assert.ThrowsAny<NullReferenceException>(() => _sut.Initialize(_appInsightConfiguration));
        }

        //Unit test Initialize
        [Fact]
        public void EnqueueContextIsNullThrows()
        {
            Assert.ThrowsAny<NullReferenceException>(() => _sut.EnqueueContext(_parentID, _valueEnqueueContext, _operationID));
        }

        public void Dispose()
        {
            DisposeB(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeB(bool isDisposing)
        {
            if (isDisposing)
            {
                _stream?.Dispose();
                _lightAttachment?.Dispose();
            }
        }
    }
}
