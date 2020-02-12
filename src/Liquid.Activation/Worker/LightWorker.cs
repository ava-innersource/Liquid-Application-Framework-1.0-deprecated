using Liquid.Base;
using Liquid.Base.Domain;
using Liquid.Domain;
using Liquid.Domain.Base;
using Liquid.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Liquid.Activation
{
    /// <summary>
    /// Implementation of the communication component between queues and topics, 
    /// to carry out the good practice of communication
    /// between micro services. In order to use this feature it is necessary 
    /// to implement the inheritance of this class.
    /// </summary>
    public abstract class LightWorker : LightBackgroundTask, ILightWorker
    {
        protected readonly static Dictionary<MethodInfo, QueueAttribute> _queues = new Dictionary<MethodInfo, QueueAttribute>();
        protected readonly static Dictionary<MethodInfo, TopicAttribute> _topics = new Dictionary<MethodInfo, TopicAttribute>();
        private readonly List<string> _inputValidationErrors = new List<string>();
        protected ILightTelemetry Telemetry { get; } = Workbench.Instance.Telemetry != null ? (ILightTelemetry)Workbench.Instance.Telemetry.CloneService() : null;
        protected ILightCache Cache => Workbench.Instance.Cache;
        //Instance of CriticHandler to inject on the others classes
        private readonly CriticHandler _criticHandler = new CriticHandler();

        /// <summary>
        /// Discovery the key connection defined on the implementation of the LightWorker
        /// </summary>
        /// <param name="method">Method related the queue or topic</param>
        /// <returns>String key connection defined on the implementation of the LightWorker</returns>
        protected string GetKeyConnection(MethodInfo method)
        {
            var attributes = method.ReflectedType.CustomAttributes;
            string connectionKey = "";
            if (attributes.Any())
                connectionKey = attributes.ToArray()[0].ConstructorArguments[0].Value.ToString();
            return connectionKey;
        }

        /// <summary>
        /// Check if it was declared attribute of the Key Connection on the implementation of the LightWorker
        /// </summary>
        /// <param name="method">Method related the queue or topic</param>
        /// <returns>Will true, if there is it</returns>
        private bool isDeclaredConnection(MethodInfo method)
        {
            return string.IsNullOrEmpty(GetKeyConnection(method));
        }

        /// <summary>
        /// Get the method related the queue or topic
        /// </summary>
        /// <typeparam name="T">Type of the queue or topic</typeparam>
        /// <param name="item">Item related dictionary of queue or topic</param>
        /// <returns>Method related the queue or topic</returns>
        protected virtual MethodInfo GetMethod<T>(KeyValuePair<MethodInfo, T> item)
        {
            return item.Key;
        }

        /// <summary>
        /// Implementation of the start process to discovery by reflection the Worker
        /// </summary>
        public virtual void Initialize()
        {
            Discovery();
        }

        /// <summary>
        /// Method for discovery all methods that use a LightQueue or LightTopic.
        /// </summary>
        private void Discovery()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<MethodInfo[]> _methodsSigned = (from assembly in assemblies
                                                 where !assembly.IsDynamic
                                                 from type in assembly.ExportedTypes
                                                 where type.BaseType != null && type.BaseType == typeof(LightWorker)
                                                 select type.GetMethods()).ToList();

            foreach (var methods in _methodsSigned)
            {
                foreach (var method in methods)
                {
                    foreach (TopicAttribute topic in method.GetCustomAttributes(typeof(TopicAttribute), false))
                    {
                        if (!isDeclaredConnection(method))
                        {
                            if (_topics.Values.FirstOrDefault(x => x.TopicName == topic.TopicName && x.Subscription == topic.Subscription) == null)
                            {
                                _topics.Add(method, topic);
                            }
                            else
                            {
                                throw new LightException($"Duplicated worker: there's already a worker for the same topic (\"{topic.TopicName}\") and subscription(\"{topic.Subscription}\")");
                            }
                        }
                        else
                        {
                            // if there isn't Custom Attribute with string connection, will be throw exception.
                            throw new LightException($"No Attribute MessageBus with a configuration string has been informed on the worker \"{method.DeclaringType}\".");
                        }
                    }
                    foreach (QueueAttribute queue in method.GetCustomAttributes(typeof(QueueAttribute), false))
                    {
                        if (!isDeclaredConnection(method))
                        {
                            if (_queues.Values.FirstOrDefault(x => x.QueueName == queue.QueueName) == null)
                                _queues.Add(method, queue);
                            else
                                throw new LightException($"There is already Queue defined with the name \"{queue.QueueName}\".");
                        }
                        else
                        {
                            //If there isn't Custom Attribute with string connection, will be throw exception.
                            throw new LightException($"No Attribute MessageBus with a configuration string has been informed on the worker \"{method.DeclaringType}\".");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method created to process by reflection the Workers declared
        /// </summary>
        /// <returns>object</returns>
        public static object InvokeProcess(MethodInfo method, byte[] message)
        {
            object result = null;
            if (method != null)
            {
                ParameterInfo[] parameters = method.GetParameters();
                object classInstance = Activator.CreateInstance(method.ReflectedType, null);

                if (parameters.Length == 0)
                {
                    result = method.Invoke(classInstance, null);
                }
                else
                {
                    dynamic lightMessage = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message), parameters[0].ParameterType);

                    object[] parametersArray = new object[] { lightMessage };
                    result = method.Invoke(classInstance, parametersArray);

                    var resultTask = (result as System.Threading.Tasks.Task);
                    if (resultTask != null && resultTask.IsFaulted)
                    {
                        throw resultTask.Exception;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Method for create a instance of LightDomain objects
        /// </summary>
        /// <typeparam name="T">Type of LightDomain</typeparam>
        /// <returns></returns>
        protected T Factory<T>() where T : LightDomain, new()
        {
            ///Verify if there's erros
            if (_inputValidationErrors.Count > 0)
            {
                /// Throws the error code from errors list of input validation to View Model
                throw new InvalidInputException(_inputValidationErrors);
            }
            var domain = LightDomain.FactoryDomain<T>();
            domain.Telemetry = Telemetry;
            domain.Cache = Cache;
            domain.CritictHandler = _criticHandler;
            return domain;
        }

        /// <summary>
        /// The method receive the error code to add on errors list of input validation.
        /// </summary>
        /// <param name="error">The code error</param>
        protected void AddInputValidationErrorCode(string error)
        {
            _inputValidationErrors.Add(error);
        }

        /// <summary>
        /// The method receive the ViewModel to input validation and add on errors list
        /// (if there are errors after validation ViewModel.)
        /// </summary>
        /// <param name="viewModel">The ViewModel to input validation</param>
        protected void ValidateInput<T>(T viewModel) where T : LightViewModel<T>, new()
        {
            viewModel.InputErrors = _inputValidationErrors;
            viewModel.Validate();
            InputResultValidation result = viewModel.Validator.Validate(viewModel);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ///receive the error code to add on errors list of input validation.
                    AddInputValidationErrorCode(error);
                }
            }
            //By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (PropertyInfo fieldInfo in viewModel.GetType().GetProperties())
            {
                dynamic child = fieldInfo.GetValue(viewModel);
                //When the child is a list, validate each of its members  
                if (child is IList)
                {
                    var children = (IList)fieldInfo.GetValue(viewModel);
                    foreach (var item in children)
                    {

                        //Check, if the property is a Light ViewModel, only they will validation Lights ViewModel
                        if ((item.GetType().BaseType != typeof(object))
                             && (item.GetType().BaseType != typeof(System.ValueType))
                                && (item.GetType().BaseType.GetGenericTypeDefinition() == typeof(LightViewModel<>)))
                        {
                            dynamic obj = item;
                            //Check, if the attribute is null for verification of the type.
                            if (obj != null)
                                ValidateInput(obj);
                        }
                    }
                }
                else
                {
                    //Otherwise, validate the very child once. 
                    if (child != null)
                    {

                        //Check, if the property is a Light ViewModel, only they will validation Lights ViewModel
                        if ((child.GetType().BaseType != typeof(object))
                           && (child.GetType().BaseType != typeof(System.ValueType))
                            && (child.GetType().BaseType.IsGenericType &&
                            child.GetType().BaseType.GetGenericTypeDefinition() == typeof(LightViewModel<>)))
                        {
                            ValidateInput(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method for send response.
        /// </summary>
        /// <param name="response">the domain response to be terminated</param>
        /// <returns>response</returns>
        protected object Terminate(DomainResponse response)
        {
            ///Verify if there's erros
            if (_criticHandler.Critics.Count > 0)
            {
                /// Throws the error code from errors list of input validation to View Model
                throw new BusinessValidationException(_criticHandler.Critics.Select(x => x.Code).ToList());
            }

            return response;
        }

    }
}