using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchEvents.Model;
using Liquid.OnAWS.Telemetry;
using Liquid.Runtime.Configuration.Base;
using Liquid.Runtime.Telemetry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Liquid.OnAWS
{
    /// <summary>
    /// The CloudWatch class is the lowest level of integration of WorkBench with AWS CloudWatch.
    /// It directly provides a client to send the messages to the cloud.
    /// So it is possible to trace all logs, events, traces, exceptions in an aggregated and easy-to-use form.
    /// </summary>
    public class CloudWatch : LightTelemetry
    {
        //AmazonCloudWatchEventsClient is responsible for sending all telemetry requests to the AWS.
        //It is still possible to make settings regarding the hierarchy of messages.
        //This setting is changeable as desired by the developer.
        private AmazonCloudWatchEventsClient _amazonCloudEvents { get; set; }
        //AmazonCloudWatchClient is responsible for sending all telemetry requests to the AWS.
        //It is still possible to make settings regarding the hierarchy of messages.
        //This setting is changeable as desired by the developer.
        private AmazonCloudWatchClient _amazonCloud { get; set; }
        private string Unknow { get { return "Unknow"; } }


        // Initialize all clients for send to cloudWatch with the clients AmazonCloudWatchEventsClient and AmazonCloudWatchClient
        public override void Initialize()
        {
            //Get all configuration on appsettings.
            CloudWatchConfiguration cloudWatchConfiuration = LightConfigurator.Config<CloudWatchConfiguration>("CloudWatch");
            //Generate a client to send events with te configuration access key and secret key.
            _amazonCloudEvents = new AmazonCloudWatchEventsClient(cloudWatchConfiuration.AccessKeyID, cloudWatchConfiuration.SecretAccessKey);
            //Generate a client to send metrics and alarms with te configuration access key and secret key.
            _amazonCloud = new AmazonCloudWatchClient(cloudWatchConfiuration.AccessKeyID, cloudWatchConfiuration.SecretAccessKey);
        }

        //TrackEvent is a wrapper that sends messages in the event format to CloudWatch.
        public override void TrackEvent(params object[] events)
        {
            //Create a object to send the event PutEventsRequest
            PutEventsRequest putEventsRequest = new PutEventsRequest()
            {
                //Create a Entries of the request
                Entries = new List<PutEventsRequestEntry>()
                {
                    //Compose the object dynamically
                    new PutEventsRequestEntry()
                    {
                        Detail      = IsAllowedSet(events, 0)   ? (string) events[0]             : Unknow,
                        DetailType  = IsAllowedSet(events, 1)   ? (string) events[1]             : Unknow,
                        Resources   = IsAllowedSet(events, 2)   ? (List<string>) events[2]       : new List<string> { Unknow },
                        Source      = IsAllowedSet(events, 3)   ? (string) events[3]             : Unknow,
                        Time        = DateTime.Now
                    }
                }
            };
            //Send event to AWS
            _amazonCloudEvents.PutEventsAsync(putEventsRequest);
        }

        //TrackMetric sends to the AppInsights metrics related to some point of view. 
        //For example, you can measure how much time was spent to persist data in the database.
        public override void TrackMetric(string metricLabel, double value)
        {
            //Create a object to send the event PutMetricDataRequest
            PutMetricDataRequest putMetricDataRequest = new PutMetricDataRequest()
            {
                //Create a Entries of the request
                MetricData = new List<MetricDatum>()
                {
                    new MetricDatum()
                    {
                        MetricName = metricLabel,
                        Value = value,
                        Timestamp = DateTime.Now,
                        Unit = "none"
                    }
                },
                //Directory that will be save all telemetrys
                Namespace = "AMAW/Logs"
            };
            //Client send to AWS a metric for be registred.
            this._amazonCloud.PutMetricDataAsync(putMetricDataRequest);
        }

        //TrackTrace will be called when it is necessary to make a diagnosis of any specific problems.
        //In this case the trace event can be customized by passing an object with more details of the problem.
        public override void TrackTrace(params object[] trace)
        {
            //Create a object to send the event PutEventsRequest
            PutEventsRequest putEventsRequest = new PutEventsRequest()
            {
                //Create a Entries of the request
                Entries = new List<PutEventsRequestEntry>()
                {
                    //Compose the object dynamically
                    new PutEventsRequestEntry()
                    {
                        Detail      = IsAllowedSet(trace, 0)   ? (string) trace[0]             : Unknow,
                        DetailType  = IsAllowedSet(trace, 1)   ? (string) trace[1]             : Unknow,
                        Resources   = IsAllowedSet(trace, 2)   ? (List<string>) trace[2]       : new List<string> { Unknow },
                        Source      = IsAllowedSet(trace, 3)   ? (string) trace[3]             : Unknow,
                        Time        = DateTime.Now
                    }
                }
            };
            //Client send to AWS am event for be registred.
            _amazonCloudEvents.PutEventsAsync(putEventsRequest);
        }

        //TrackException will send the entire monitored exception from WorkBench to CloudWatch.
        public override void TrackException(Exception exception)
        {
            //Create a object to send the event PutEventsRequest
            PutEventsRequest putEventsRequest = new PutEventsRequest()
            {
                //Create a Entries of the request
                Entries = new List<PutEventsRequestEntry>()
                {
                     //Compose the object dynamically
                    new PutEventsRequestEntry()
                    {
                        Detail      = exception.Message,
                        DetailType  = "Exception",
                        Resources   = {exception.TargetSite.Name, exception.HelpLink },
                        Source      = exception.StackTrace,
                        Time        = DateTime.Now
                    }
                }
            };

            //Client send to AWS am event for be registred.
            _amazonCloudEvents.PutEventsAsync(putEventsRequest);
        }

        //TrackAggregateMetric contains the aggregation logic of just send to the AppInsights when the BeginComputeMetric and EndComputeMeric 
        //is called. With some key is not registred
        public override void TrackAggregateMetric(object metricTelemetry)
        {
            PutMetricDataRequest putMetricDataRequest = new PutMetricDataRequest()
            {
                MetricData = new List<MetricDatum>()
                {
                    new MetricDatum()
                    {
                        MetricName = (string) metricTelemetry.GetType().GetProperty("Name").GetValue(metricTelemetry),
                        Value = (double) metricTelemetry.GetType().GetProperty("Sum").GetValue(metricTelemetry),
                        Timestamp = DateTime.Now,
                        Unit = "aggregated"
                    }
                },

                Namespace = "AMAW/Logs"
            };

            //Client send to AWS a metric for be registred.
            this._amazonCloud.PutMetricDataAsync(putMetricDataRequest);
        }

        //Check if object is have value in some index position.
        //Otherwise, will fault a complement of objects. On the microservices must be adjusted for write in the correct way
        private bool IsAllowedSet(object[] parameters, int position)
        {
            return parameters.Length > position;
        }

        //This feature doesn't exist on AWS CloudWatch
        public override void EnqueueContext(string parentID, object value = null, string operationID = "")
        {
            throw new NotImplementedException();
        }

        //This feature doesn't exist on AWS CloudWatch
        public override void DequeueContext()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                var x = _amazonCloud.ListMetricsAsync();
                return LightHealth.HealthCheck.Healthy;
            }catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }                
        }
    }
}
