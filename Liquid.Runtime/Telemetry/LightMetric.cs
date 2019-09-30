using Liquid;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Threading;

namespace Liquid.Runtime.Telemetry
{
    /// <summary>
    /// Accepts metric values and sends the aggregated values when the method EndAggregation is called.
    /// </summary>
    public sealed class LightMetric
    {
        private LightMetricAggregator _aggregator = null;
        ///Get Reference for send to the AppInsights.
        private readonly LightTelemetry _telemetry = (LightTelemetry)WorkBench.Telemetry;
        ///Metric's name. In this case, differents class can be the same name for the metric.
        public string Name { get; }
        ///Constructor responsible for picking up the metric name and instantiating a LightMetricAggregator.
        public LightMetric(string name)
        {
            this.Name = name ?? "null";
            this._aggregator = new LightMetricAggregator(DateTimeOffset.UtcNow);
        }

        public void TrackValue(double value)
        {
            LightMetricAggregator currAggregator = _aggregator;
            if (currAggregator != null)
            {
                currAggregator.TrackValue(value);
            }
        }

        /// <summary>
        /// This method was separated because will be invoked for LighTelemetry.
        /// When the method 'EndAggregation' is called, the method above will send all data aggregated to AppInsights
        /// </summary>
        public void SendAggregationMetrics()
        {
            try

            {
                /// Atomically snap the current aggregation:
                LightMetricAggregator nextAggregator = new LightMetricAggregator(DateTimeOffset.UtcNow);
                LightMetricAggregator prevAggregator = Interlocked.Exchange(ref _aggregator, nextAggregator);

                ///  Only send anything is at least one value was measured:
                if (prevAggregator != null && prevAggregator.Count > 0)
                {
                    ///  Compute the actual aggregation period length:
                    TimeSpan aggPeriod = nextAggregator.StartTimestamp - prevAggregator.StartTimestamp;
                    if (aggPeriod.TotalMilliseconds < 1)
                    {
                        /// Just if the timestamp is smaller than one second, we send to appinsights the milliseconds
                        aggPeriod = TimeSpan.FromMilliseconds(1);
                    }

                    ///  Construct the metric telemetry item and send to AppInsights
                    var aggregatedMetricTelemetry = new MetricTelemetry(
                            Name,
                            prevAggregator.Count,
                            prevAggregator.Sum,
                            prevAggregator.Min,
                            prevAggregator.Max,
                            prevAggregator.StandardDeviation);
                    aggregatedMetricTelemetry.Properties["AggregationPeriod"] = aggPeriod.ToString("c");

                    ///Send aggregated data to AppInsights
                    _telemetry.TrackAggregateMetric(aggregatedMetricTelemetry);
                }
            }
            catch (Exception ex)
            {
                ///There are some erros AppInsights will be notified.
                this._telemetry.TrackException(ex);
            }
        }
    }
}