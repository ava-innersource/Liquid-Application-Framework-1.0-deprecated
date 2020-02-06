using Liquid.Base;
using Liquid.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Liquid.Activation
{
    /// <summary>
    /// Implementation of the communication component between hub, 
    /// to carry out the good practice of sensibilize a big data strategy.
    /// </summary>
    public abstract class LightEvent : ILightEvent
    {
        protected readonly static Dictionary<TypeInfo, HubAttribute> _eventCache = new Dictionary<TypeInfo, HubAttribute>();
        //Cloning TLightelemetry service singleton because it services multiple LightDomain instances from multiple threads with instance variables
        private readonly ILightTelemetry _telemetry = Workbench.Instance.Telemetry != null ? (ILightTelemetry)Workbench.Instance.Telemetry.CloneService() : null;
        protected ILightTelemetry Telemetry => _telemetry;


        /// <summary>
        /// Implementation of the start process to discovery by reflection the LightEvent
        /// </summary>
        public virtual void Initialize()
        {
            Discovery();
        }

        /// <summary>
        /// Method for discovery all methods that use a Hub.
        /// </summary>
        private void Discovery()
        {
            IEnumerable<Type> _classesSigned = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(ILightModel).IsAssignableFrom(p) && !p.IsInterface);

            foreach (Type t in _classesSigned)
            {
                foreach (HubAttribute hub in t.GetCustomAttributes(typeof(HubAttribute), false))
                {
                    if (hub.ConfigTagName == null || string.IsNullOrEmpty(hub.HubName))
                    {
                        ///If there isn't Custom Attribute with key connection, will be throw exception.
                        throw new LightException($"Wrong implementation of HubAttribute on the model \"{t.Name}\".");
                    }
                    else
                    {
                        if (_eventCache.Values.FirstOrDefault(x => x.HubName == hub.HubName) == null)
                            _eventCache.Add(t.GetTypeInfo(), hub);
                        else
                            throw new LightException($"There is already Hub defined with the name \"{hub.HubName}\".");
                    }
                }
            }
        }

        /// <summary>
        /// Check if a model is stored in event cache, check Discovery()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        protected static KeyValuePair<TypeInfo, HubAttribute> CheckEventCache<T>(T model)
        {
            KeyValuePair<TypeInfo, HubAttribute> keyPair = new KeyValuePair<TypeInfo, HubAttribute>(null, null);

            Type modelType = model.GetType();
            HubAttribute hub = _eventCache.GetValueOrDefault(modelType.GetTypeInfo());

            if (hub != null)
            {
                keyPair = new KeyValuePair<TypeInfo, HubAttribute>(modelType.GetTypeInfo(), hub);
            }

            return keyPair;
        }

        public abstract Task<T> SendToHub<T>(T model, string dataOperation);
    }
}
