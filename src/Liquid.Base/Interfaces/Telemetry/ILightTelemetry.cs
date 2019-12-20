using Liquid.Base.Interfaces;
using System;

namespace Liquid.Interfaces
{
    public interface ILightTelemetry : IWorkbenchHealthCheck
    {
        void TrackTrace(params object[] trace);
        void TrackEvent(params object[] events);
        void TrackMetric(string metricLabel, double value);
        void ComputeMetric(string metricLabel, double value);
        void BeginMetricComputation(string metricLabel);
        void EndMetricComputation(string metricLabel);
        void EnqueueContext(string parentID, object value = null, string operationID = "");        
        void DequeueContext();
    }
}
