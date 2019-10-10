using Liquid.Base.Interfaces;
using Liquid.Runtime.Telemetry;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Liquid.Activation
{
    /// <summary>
    /// Implementation of the communication component  of Scheduler 
    /// </summary>
    public class Scheduler : LightScheduler, IWorkBenchHealthCheck
    {
        /// <summary>
        /// Implementation of the start process Scheduler. It must be called  parent before start processes.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            StartWorkerAsync();
        }

        protected async Task StartWorkerAsync()
        {
            try
            {
                foreach (var scheduling in _scheduler)
                {
                    MethodInfo method = GetMethod(scheduling);
                    string name = scheduling.Value.Name;
                    CancellationToken _cancelToken = new CancellationToken(); 
                    await StartAsync(_cancelToken, name, method);
                }
            }
            catch (Exception exception)
            {
                Exception moreInfo = new Exception($"Error setting up queue consumption from scheduler. See inner exception for details. Message={exception.Message}", exception);
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                ((LightTelemetry)WorkBench.Telemetry).TrackException(moreInfo);
            }

        }


        /// <summary>
        /// Process 
        /// </summary>
        /// <returns></returns>
        protected override async Task ProcessAsync(MethodInfo method)
        {
            try
            {
                InvokeProcess(method, null);
            }
            catch (Exception exRegister)
            {
                Exception moreInfo = new Exception($"Exception reading message from scheduler. See inner exception for details. Message={exRegister.Message}", exRegister);
                //Use the class instead of interface because tracking exceptions directly is not supposed to be done outside AMAW (i.e. by the business code)
                ((LightTelemetry)WorkBench.Telemetry).TrackException(moreInfo);
            }

        }


        /// <summary>
        /// Method to run Health Check for Service Bus
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }
        }
    }
}
