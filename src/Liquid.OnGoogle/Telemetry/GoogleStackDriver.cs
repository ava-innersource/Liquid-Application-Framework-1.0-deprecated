using Liquid.Interfaces;
using Liquid.Runtime.Telemetry;
using Liquid.OnGoogle;
using Liquid.Runtime.Configuration.Base;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Diagnostics.AspNetCore;
using Google.Cloud.Diagnostics.Common;
using System;
using Google.Cloud.Trace.V1;
using Google.Cloud.Logging.V2;
using Google.Api;

namespace Liquid.OnGoogle
{
    /// <summary>
    /// The StackDriver class is the lowest level of integration of WorkBench with Google StackDriver.
    /// It directly provides a client to send the messages to the cloud.
    /// So it is possible to trace all logs, events, traces, exceptions in an aggregated and easy-to-use form.
    /// </summary>
    public class GoogleStackDriver : LightTelemetry, ILightTelemetry
    {
        /// <summary>
        /// Creating Google Stack Driver Configuration
        /// </summary>
        GoogleStackDriverConfiguration googleStackDriverConfiguration;
        /// <summary>
        /// Creating the Client to point to a Google Cloud Project ID
        /// </summary>
        LoggingServiceV2Client clientV2;
        /// <summary>
        /// Creating the LogName to receive project Id and Log Id
        /// </summary>
        LogName logName;

        public ConfigServiceV2Client configServiceV2Client { get; set; }
        public string Unknow { get; private set; }

        public override void Initialize()
        {            
            googleStackDriverConfiguration = LightConfigurator.Config<GoogleStackDriverConfiguration>("GoogleStackDriver");
            clientV2 = LoggingServiceV2Client.Create();
            logName = new LogName(googleStackDriverConfiguration.ProjectId, googleStackDriverConfiguration.LogId);            
        }

        /// <summary>
        /// Not Implemented because the driver isn't implemented.
        /// </summary>
        /// <param name="metricTelemetry"></param>
        public override void TrackAggregateMetric(object metricTelemetry)
        {
            //Create a object to write trace log Entries            
            LogEntry logEntry = new LogEntry
            {
                LogName = logName.ToString(),               
                TextPayload = (string)metricTelemetry.GetType().GetProperty("Name").GetValue(metricTelemetry) +" - "+ (string)metricTelemetry.GetType().GetProperty("Sum").GetValue(metricTelemetry),
            };
            //Adding Metric log entries to send to google Cloud 
            MonitoredResource resource = new MonitoredResource { Type = "TrackAggregateMetric" };
            IDictionary<string, string> entryLabels = new Dictionary<string, string>
                {
                    { "size", "large" },
                    { "color", "red" }
                };
                       
            //Send Metric Log to Google Cloud Stack Driver
            clientV2.WriteLogEntries(LogNameOneof.From(logName), resource, entryLabels,new[] { logEntry });
        }

        public override void TrackEvent(params object [] events)
        {
            LogEntry logEntry = new LogEntry
            {
                LogName = IsAllowedSet(events, 0) ? (string)events[0] : Unknow,
                TextPayload = IsAllowedSet(events, 1) ? (string)events[1] : Unknow,
                Resource = (MonitoredResource)events[2],
                SourceLocation = IsAllowedSet(events, 3) ? (LogEntrySourceLocation)events[3] : null,
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now)
            };
            //Adding Metric log entries to send to google Cloud 
            MonitoredResource resource = new MonitoredResource { Type = "TrackEvent" };
            IDictionary<string, string> entryLabels = new Dictionary<string, string>
                {
                    { "size", "large" },
                    { "color", "red" }
                };

            //Send Metric Log to Google Cloud Stack Driver
            clientV2.WriteLogEntries(LogNameOneof.From(logName), resource, entryLabels, new[] { logEntry });
        }

        public override void TrackException(Exception exception)
        {
            LogEntry logEntry = new LogEntry
            {
                LogName = exception.HResult.ToString(),
                TextPayload = exception.Message,
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now)
            };
            //Adding Metric log entries to send to google Cloud 
            MonitoredResource resource = new MonitoredResource { Type = "TrackException" };
            IDictionary<string, string> entryLabels = new Dictionary<string, string>
                {
                    { "size", "large" },
                    { "color", "red" }
                };

            //Send Metric Log to Google Cloud Stack Driver
            clientV2.WriteLogEntries(LogNameOneof.From(logName), resource, entryLabels, new[] { logEntry });
        }

        //TrackMetric sends to the Google Stack Driver metrics related to some point of view. 
        //For example, you can measure how much time was spent to persist data in the database.
        public override void TrackMetric(string metricLabel, double value)
        {
            LogEntry logEntry = new LogEntry
            {
                LogName = metricLabel,
                TextPayload = value.ToString(),                
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now)
            };
            //Adding Metric log entries to send to google Cloud 
            MonitoredResource resource = new MonitoredResource { Type = "TrackMetric" };
            IDictionary<string, string> entryLabels = new Dictionary<string, string>
                {
                    { "size", "large" },
                    { "color", "red" }
                };

            //Send Metric Log to Google Cloud Stack Driver
            clientV2.WriteLogEntries(LogNameOneof.From(logName), resource, entryLabels, new[] { logEntry });
        }

        public override void TrackTrace(params object[] trace)
        {
            LogEntry logEntry = new LogEntry
            {
                LogName = IsAllowedSet(trace, 0) ? (string)trace[0] : Unknow,
                TextPayload = IsAllowedSet(trace, 1) ? (string)trace[1] : Unknow,
                Resource = (MonitoredResource)trace[2],
                SourceLocation = IsAllowedSet(trace, 3) ? (LogEntrySourceLocation)trace[3] : null,
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now)
            };
            //Adding Metric log entries to send to google Cloud 
            MonitoredResource resource = new MonitoredResource { Type = "TrackTrace" };
            IDictionary<string, string> entryLabels = new Dictionary<string, string>
                {
                    { "size", "large" },
                    { "color", "red" }
                };

            //Send Metric Log to Google Cloud Stack Driver
            clientV2.WriteLogEntries(LogNameOneof.From(logName), resource, entryLabels, new[] { logEntry });
        }


        //Check if object is have value in some index position.
        //Otherwise, will fault a complement of objects. On the microservices must be adjusted for write in the correct way
        private bool IsAllowedSet(object[] parameters, int position)
        {
            return parameters.Length > position;
        }
        /// <summary>
        /// Not implemented for Google Stack Driver
        /// </summary>
        public override void DequeueContext()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Not implemented for Google Stack Driver
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="value"></param>
        /// <param name="operationID"></param>
        public override void EnqueueContext(string parentID, object value = null, string operationID = "")
        {
            throw new NotImplementedException();
        }
    }
}
