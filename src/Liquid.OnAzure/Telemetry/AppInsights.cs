using Liquid.Interfaces;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Liquid.OnAzure
{
    /// <summary>
    /// The AppInsights class is the lowest level of integration of WorkBench with Azure AppInsights.
    /// It directly provides a client to send the messages to the cloud.
    /// So it is possible to trace all logs, events, traces, exceptions in an aggregated and easy-to-use form.
    /// </summary>
    public class AppInsights : LightTelemetry, ILightTelemetry
    {
        //TelemetryClient is responsible for sending all telemetry requests to the azure.
        //It is still possible to make settings regarding the hierarchy of messages.
        //This setting is changeable as desired by the developer.
        private static TelemetryClient TelemetryClient;               
              
        public AppInsights() { }

        //TrackEvent is a wrapper that sends messages in the event format to AppInsights.
        public override void TrackEvent(params object[] events)
        {
            TelemetryClient.TrackEvent(new EventTelemetry() { Name = (string)events[0] });
        }
        //TrackMetric sends to the AppInsights metrics related to some point of view. 
        //For example, you can measure how much time was spent to persist data in the database.
        public override void TrackMetric(string metricLabel, double value)
        {
            TelemetryClient.TrackMetric(new MetricTelemetry() { Name = metricLabel, Sum = value });
        }
        //
        //TrackTrace will be called when it is necessary to make a diagnosis of any specific problems.
        //In this case the trace event can be customized by passing an object with more details of the problem.
        public override void TrackTrace(params object[] trace)
        {
            TelemetryClient.TrackTrace((string)trace[0]);
        }
        //TrackAggregateMetric contains the aggregation logic of just send to the AppInsights when the BeginComputeMetric and EndComputeMeric 
        //is called. With some key is not registred
        public override void TrackAggregateMetric(object metricTelemetry)
        {
            TelemetryClient.TrackMetric((MetricTelemetry)metricTelemetry);
        }

        //TrackException will send the entire monitored exception from WorkBench to AppInsights.
        public override void TrackException(Exception exception)
        {
            TelemetryClient.TrackException(exception);
        }
        
        // Initialize will retrieve the authentication token from the configuration file set in "appsettings.json". 
        // Also, it will instantiate a telemetry client to send all Azure portal pro requests.        
        public override void Initialize()
        {
            AppInsightsConfiguration appInsightsConfiguration = LightConfigurator.Config<AppInsightsConfiguration>("ApplicationInsights");

            TelemetryConfiguration aiConfig = new TelemetryConfiguration();
            aiConfig.InstrumentationKey = appInsightsConfiguration.InstrumentationKey;
                      
            // automatically correlate all telemetry data with request
            aiConfig.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());

            TelemetryClient = TelemetryClient ?? new TelemetryClient(aiConfig);
        }

        //This WrapperTelemetry was created to be an anonymous method, so it can be called by other methods as a function pointer. 
        //It receives as parameters the attributes that will be set for the telemetry client
        private delegate void WrapperTelemetry(string ParentID, object Value, string OperationID, TelemetryClient telemetryClient);


        //Instance of our delgate that will receive the parameters of SetContext and will configure the entire instance of the client.
        private readonly WrapperTelemetry wrapper = (parent, value, operation, telemtry) => 
        {
            telemtry.Context.Operation.ParentId = $"{parent}: {value}";
            telemtry.Context.Operation.Id = !string.IsNullOrEmpty(operation) ? $"{operation}: {Guid.NewGuid().ToString()}" : $"{Guid.NewGuid().ToString()}";
            telemtry.Context.Operation.Name = $"{operation} {parent}";
        };

        //SetContext is responsible for receiving a context of some method from which it was called, 
        //thus creating a view hierarchy in the AppInsights dashboard. Some parameters can be ommitted.
        //The develop will track the logic that need see on Azure portal. ParentID is necessary to track all events and organize it.
        public override void EnqueueContext(string parentID, object value = null, string operationID = "")
        {
            wrapper.Invoke(parentID, value, operationID, TelemetryClient);
        }

        // Whenever a SetContext is declared it is necessary to terminate its operations, that is, 
        //when completing all the operations trace, it is necessary to reconfigure all telemetry client changes in order to avoid any data inconsistencies.
        public override void DequeueContext()
        {
            TelemetryClient.Context.Operation.ParentId = null;
            TelemetryClient.Context.Operation.Id = null;
            TelemetryClient.Context.Operation.Name = null;
        }
    }
}
