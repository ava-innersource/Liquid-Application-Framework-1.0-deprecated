using Liquid.Base;
using Liquid.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Runtime.Telemetry
{
    /// <summary>
    /// LightTelemetry implements only part of the management of aggregate metrics.
    /// According to the specification, all aggregate telemetry monitoring events will be managed by this class.
    /// Other features will be implemented by the lower-level class in the AppInsights case.
    /// </summary>
    public abstract class LightTelemetry : ILightTelemetry
    {
        private readonly Dictionary<string, LightMetric> _aggregators = new Dictionary<string, LightMetric>();
        public abstract void TrackTrace(params object[] trace);
        public abstract void TrackEvent(params object[] events);
        public abstract void TrackMetric(string metricLabel, double value);
        public abstract void TrackException(Exception exception);

        ///If there is a metric key registered previously, then the new request to compute a metric will be accepted.
        ///Thus we can avoid overhead on networking sending data just when the aggregation was finish.
        public void ComputeMetric(string metricLabel, double value)
        {
            LightMetric lightMetricAggregator;

            _aggregators.TryGetValue(metricLabel, out lightMetricAggregator);

            if (lightMetricAggregator == null)
            {
                throw new LightException($"There is no metric  \"{metricLabel}\" under aggregation.");
            }

            lightMetricAggregator.TrackValue(value);
        }

        ///This method its just called inside on LightMetricAggregator.
        ///All validation is done on begin and end metric computation.
        public abstract void TrackAggregateMetric(object metricTelemetry);

        ///To send an aggregate metric for telemetry it is necessary to call this method by sending the key that will be added.
        ///If the key has already been entered, an exception will be raised.
        public void BeginMetricComputation(string metricLabel)
        {
            LightMetric lightMetricAggregator;

            _aggregators.TryGetValue(metricLabel, out lightMetricAggregator);

            if (lightMetricAggregator != null)
            {
                throw new LightException($"The metric \"{metricLabel}\" is already been aggregated.");
            }

            _aggregators.Add(metricLabel, new LightMetric(metricLabel));
        }

        ///After completing all data aggregation, you must call this method to send all the aggregate telemetries to AppInsights.
        public void EndMetricComputation(string metricLabel)
        {
            LightMetric lightMetricAggregator;

            _aggregators.TryGetValue(metricLabel, out lightMetricAggregator);

            if (lightMetricAggregator == null)
            {
                throw new LightException($"There is no metric  \"{metricLabel}\" under aggregation.");
            }
            else
            {
                _aggregators.Remove(metricLabel);

                lightMetricAggregator.SendAggregationMetrics();
            }
        }
        /// Initialize will retrieve the authentication token from the configuration file set in "appsettings.json". 
        /// Also, it will instantiate a telemetry client to send all Azure portal pro requests.  
        public abstract void Initialize();
        ///SetContext is responsible for receiving a context of some method from which it was called, 
        ///thus creating a view hierarchy in the AppInsights dashboard. Some parameters can be ommitted.
        ///The develop will track the logic that need see on Azure portal. ParentID is necessary to track all events and organize it.
        public abstract void EnqueueContext(string parentID, object value = null, string operationID = "");
        /// Whenever a SetContext is declared it is necessary to terminate its operations, that is, 
        ///when completing all the operations trace, it is necessary to reconfigure all telemetry client changes in order to avoid any data inconsistencies.
        public abstract void DequeueContext();
    }
}
