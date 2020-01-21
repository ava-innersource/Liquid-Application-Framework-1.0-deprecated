// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Liquid.Base.Interfaces;

namespace Liquid.Interfaces
{
    /// <summary>
    /// Used to gather telemetry from the application.
    /// </summary>
    /// <remarks>
    /// This enables Liquid to manually track telemetry and provides an abstraction for multiple
    /// telemetry providers. Users, however, are encouraged to use the proper middlewares from
    /// the providers that will make capturing some standard metrics easier.
    /// </remarks>
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

        /// <summary>
        /// Tracks an exception from the application.
        /// </summary>
        /// <param name="e">The exception that will be tracked in the current context.</param>
        void TrackException(Exception e);
    }
}
