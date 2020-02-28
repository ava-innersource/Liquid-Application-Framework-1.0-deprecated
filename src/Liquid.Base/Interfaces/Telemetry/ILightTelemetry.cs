// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Service that enables observability for implementers and Liquid itself.
    /// </summary>
    public interface ILightTelemetry : IWorkbenchService
    {
        void TrackTrace(params object[] trace);
        void TrackEvent(params object[] events);
        void TrackMetric(string metricLabel, double value);
        void ComputeMetric(string metricLabel, double value);
        void BeginMetricComputation(string metricLabel);
        void EndMetricComputation(string metricLabel);
        void EnqueueContext(string parentID, object value = null, string operationID = "");
        void DequeueContext();

        /// <summary>
        /// Captures an exception in the telemetry provider.
        /// </summary>
        /// <param name="exception">The exception to be captured.</param>
        void TrackException(Exception exception);
    }
}
