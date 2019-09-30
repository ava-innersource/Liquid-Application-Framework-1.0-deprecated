using Liquid.Base.Domain;
using Liquid.Domain;
using Liquid.Activation;
using Liquid.Interfaces;
using Liquid.Repository;
using Liquid.Runtime.Configuration.Base;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.OnAzure
{
    public class AzureEventHub : LightEvent
    {
        HubConfiguration config = null;
        /// <summary>
        /// Inicialize the class with set Config Name and Queue Name, must called the parent method
        /// </summary>
        /// <param name="tagConfigName"> Config Name </param>
        /// <param name="name">Queue Name</param> 
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Get connection settings
        /// </summary>
        /// <param name="TagConfigName"></param>
        private string GetConnection(string TagConfigName)
        {
            if (string.IsNullOrEmpty(TagConfigName)) // Load specific settings if provided
                config = LightConfigurator.Config<HubConfiguration>("EventHub");
            else
                config = LightConfigurator.Config<HubConfiguration>($"EventHub_{TagConfigName}");

            return config.ConnectionString;
        }

        /// <summary>
        /// To serialize models ans send to configured Hub 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="dataOperation"></param>
        /// <returns></returns>
        public override async Task<T> SendToHub<T>(T model, string dataOperation)
        {
            EventHubClient eventHubClient;

            var eventMessage = new LightEventMessage()
            {
                MessageType = dataOperation,
                PayLoad = (model != null) ? model.ToJsonCamelCase() : null
            };

            var message = JsonConvert.SerializeObject(eventMessage);

            KeyValuePair<TypeInfo, HubAttribute> keyPair = CheckEventCache(model);

            if (keyPair.Key != null && !string.IsNullOrEmpty(message))
            {
                string ConnectionString = this.GetConnection(keyPair.Value.ConfigTagName);

                var connectionStringBuilder = new EventHubsConnectionStringBuilder(ConnectionString)
                {
                    EntityPath = keyPair.Value.HubName
                };

                eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
                await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                await eventHubClient.CloseAsync();
            }

            return Task.FromResult<T>(default(T)).Result;
        }

        /// <summary>
        /// Method to run Health Check for Azure Event hub 
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
				if (config == null)
					this.GetConnection(string.Empty);

				foreach (var item in _eventCache)
				{
					if(!string.IsNullOrEmpty(item.Value.HubName))
					{
						var connectionStringBuilder = new EventHubsConnectionStringBuilder(config.ConnectionString)
						{
							EntityPath = item.Value.HubName
						};

						EventHubClient eventHub = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
						var a = eventHub.GetRuntimeInformationAsync();
						eventHub.Close();
					}				
				}
             
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }

        }
    }
}
