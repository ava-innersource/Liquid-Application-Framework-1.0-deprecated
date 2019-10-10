using Liquid.Base;
using Liquid.Base.Domain;
using Liquid.Domain;
using Liquid.Domain.Base;
using Liquid.Interfaces;
using Liquid.Microservices.Interfaces;
using Liquid.Runtime.Configuration.Base;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Liquid.Activation 
{
    /// <summary>
    /// Scheduler
    /// </summary>
    public abstract class LightScheduler : LightBackgroundTask, ILightScheduler
    {
        #region Properties  
        private Task _executingTask;
        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();
        protected readonly static Dictionary<MethodInfo, SchedulerAttribute> _scheduler = new Dictionary<MethodInfo, SchedulerAttribute>();
        private readonly List<string> _inputValidationErrors = new List<string>();
        protected ILightTelemetry Telemetry { get; } = WorkBench.Telemetry != null ? (ILightTelemetry)WorkBench.Telemetry.CloneService() : null;
        protected ILightCache Cache => WorkBench.Cache;
        protected ILightLogger Logger => WorkBench.Logger;

        //Instance of CriticHandler to inject on the others classes
        private readonly CriticHandler _criticHandler = new CriticHandler();
        #endregion

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
        /// Start a background task async
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public virtual Task StartAsync(CancellationToken cancellationToken, string suffixName, MethodInfo method)
        {
            LightSchedulerConfiguration config;
            if (string.IsNullOrEmpty(suffixName)) // Load specific settings if provided
                config = LightConfigurator.Config<LightSchedulerConfiguration>($"{nameof(Scheduler)}");
            else
                config = LightConfigurator.Config<LightSchedulerConfiguration>($"{nameof(Scheduler)}_{suffixName}");

            _executingTask = ExecuteAsync(cancellationToken, config.Interval, config.RandonAdditional, method);
            if (_executingTask.IsCompleted)
                return _executingTask;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop a background task async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
                return;

            try
            {
                _cancelToken.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        /// <summary>
        /// Execute a background Task async
        /// </summary>
        /// <param name="stoppingToken">Cancellation Token</param>
        /// <returns></returns>
        protected virtual async Task ExecuteAsync(CancellationToken stoppingToken, int interval, int RandonAdditional, MethodInfo method)
        {
            do
            {
                await ProcessAsync(method);
                await Task.Delay((interval + RandonAdditional) * 1000, stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
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
                                                 where type.BaseType != null && type.BaseType == typeof(LightScheduler)
                                                 select type.GetMethods()).ToList();

            foreach (var methods in _methodsSigned)
            {
                foreach (var method in methods)
                {

                    foreach (SchedulerAttribute scheduler in method.GetCustomAttributes(typeof(SchedulerAttribute), false))
                    {
                        if (_scheduler.Values.FirstOrDefault(x => x.Name == scheduler.Name) == null)
                            _scheduler.Add(method, scheduler);
                        else
                            throw new LightException($"There is already scheduler defined with the name \"{scheduler.Name}\".");
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
                    //Check if it needs authorization, unless that there isn't AuthorizeAttribute
                    foreach (AuthorizeAttribute authorize in method.GetCustomAttributes(typeof(AuthorizeAttribute), false))
                    {
                        if ((lightMessage.Context == null) || ((lightMessage.Context != null) && (lightMessage.Context.User == null)))
                        {
                            //If there isn't Context, will be throw exception.
                            throw new LightException($"No TokenJwt has been informed on the message sent to the scheduler.");
                        }
                        if ((authorize.Policy != null) && (lightMessage.Context.User.FindFirst(authorize.Policy) == null))
                        {
                            throw new LightException($"No Policy \"{authorize.Policy}\" has been informed on the message sent to the scheduler.");
                        }
                        if ((authorize.Roles != null) && (!lightMessage.Context.User.IsInRole(authorize.Roles)))
                        {
                            throw new LightException($"No Roles \"{authorize.Roles}\" has been informed on the message sent to the scheduler.");
                        }
                    }

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
            domain.Logger = Logger;
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



        /// <summary>
        /// Process a brackground task async.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ProcessAsync(MethodInfo method) { }

    }
}