// Copyright (c) Avanade Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Liquid.Base.Interfaces.Polly;
using Liquid.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Liquid
{
    /// <summary>
    /// Provides a global way to configure a Liquid application.
    /// </summary>
    [Obsolete("Please use the correct spelled class, Liquid.Base.Workbench")]
    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:File may only contain a single type",
        Justification = "Obsolete class will be removed.")]
    public static class WorkBench
    {
        public static ILightRepository Repository => Workbench.Instance.Repository;

        public static ILightMediaStorage MediaStorage => Workbench.Instance.MediaStorage;

        public static ILightTelemetry Telemetry => Workbench.Instance.Telemetry;

        public static ILightCache Cache => Workbench.Instance.Cache;

        public static ILightEvent Event => Workbench.Instance.Event;

        public static ILightLogger Logger => Workbench.Instance.Logger;

        public static IConfiguration Configuration => Workbench.Instance.Configuration;

        public static ILightPolly Polly => Workbench.Instance.Polly;

        public static void AddToCache(WorkBenchServiceType singletonType, IWorkBenchService singleton) => Workbench.Instance.AddToCache((WorkbenchServiceType)singletonType, (IWorkbenchService)singleton);

        public static void UseMediaStorage<T>()
            where T : ILightMediaStorage, new()
            => Workbench.Instance.UseMediaStorage<T>();

        public static void UseRepository<T>()
            where T : ILightRepository, new()
            => Workbench.Instance.UseRepository<T>();

        public static void UseMessageBus<T>()
            where T : ILightWorker, new()
            => Workbench.Instance.UseMessageBus<T>();

        public static void UseTelemetry<T>()
            where T : ILightTelemetry, new()
            => Workbench.Instance.UseTelemetry<T>();

        public static void UseEnventHandler<T>()
            where T : ILightEvent, new()
            => Workbench.Instance.UseEventHandler<T>();

        public static void UseCache<T>()
            where T : ILightCache, new()
            => Workbench.Instance.UseCache<T>();

        public static void UseLogger<T>()
            where T : ILightLogger, new()
            => Workbench.Instance.UseLogger<T>();

        public static object GetRegisteredService(WorkBenchServiceType serviceType)
            => Workbench.Instance.GetRegisteredService((WorkbenchServiceType)serviceType);

        /// <summary>
        /// Prepare Workbench to start Unit Test.
        /// </summary>
        /// <param name="settingsFileName">Name of settings file in Unit Test Project in current directory.</param>
        public static void PrepareUnitTestMode(string settingsFileName = "appsettings.json") => Workbench.Instance.PrepareUnitTestMode(settingsFileName);

        /// <summary>
        /// Initializes cartridges before run test.
        /// </summary>
        public static void RunUnitTestMode() => Workbench.Instance.RunUnitTestMode();

        /// <summary>
        /// Resets the service cache by removing all cached services.
        /// </summary>
        public static void Reset() => Workbench.Instance.Reset();
    }

    /// <summary>
    /// Provides a global way to configure a Liquid application.
    /// </summary>
    public class Workbench
    {
        /// <summary>
        /// Holds the singleton services used by Workbench, indexed by their 'service types'.
        /// </summary>
        private Dictionary<WorkbenchServiceType, IWorkbenchService> _singletonCache = new Dictionary<WorkbenchServiceType, IWorkbenchService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Workbench"/> class.
        /// </summary>
        private Workbench()
        {
        }

        /// <summary>
        /// Gets the singleton instance of <see cref="Workbench"/>.
        /// </summary>
        public static Workbench Instance { get; } = new Workbench();

        /// <summary>
        /// Gets the default <see cref="ILightRepository"/> implementation.
        /// </summary>
        public ILightRepository Repository => GetService<ILightRepository>(WorkbenchServiceType.Repository);

        /// <summary>
        /// Gets the preconfigured <see cref="ILightMediaStorage"/> implementation.
        /// </summary>
        public ILightMediaStorage MediaStorage => GetService<ILightMediaStorage>(WorkbenchServiceType.MediaStorage);

        /// <summary>
        /// Gets the preconfigured <see cref="ILightTelemetry"/> implementation.
        /// </summary>
        public ILightTelemetry Telemetry => GetService<ILightTelemetry>(WorkbenchServiceType.Telemetry);

        /// <summary>
        /// Gets the preconfigured <see cref="ILightCache"/> implementation.
        /// </summary>
        public ILightCache Cache => GetService<ILightCache>(WorkbenchServiceType.Cache, false);

        /// <summary>
        /// Gets the preconfigured <see cref="ILightEvent"/> implementation.
        /// </summary>
        public ILightEvent Event => GetService<ILightEvent>(WorkbenchServiceType.EventHandler, false);

        /// <summary>
        /// Gets the preconfigured <see cref="ILightLogger"/> implementation.
        /// </summary>
        public ILightLogger Logger => GetService<ILightLogger>(WorkbenchServiceType.Logger, false);

        /// <summary>
        /// Gets the default <see cref="ILightPolly"/> configuration.
        /// </summary>
        public ILightPolly Polly { get; set; }

        /// <summary>
        /// Gets the <see cref="Workbench"/> configuration.
        /// </summary>
        public IConfiguration Configuration { get; set; } = null;

        // Indicates if the Workbench Services are initialized.
        private bool _isInitialized = false;

        /// <summary>
        /// Adds a <see cref="IWorkbenchService"/> to the cache.
        /// </summary>
        /// <param name="type">The type of the service being added.</param>
        /// <param name="service">The actual service instance.</param>
        public void AddToCache(WorkbenchServiceType type, IWorkbenchService service)
        {
            if (_singletonCache.ContainsKey(type))
            {
                var message = $"The SingletonType '{type}' has been already set. Only one Workbench service of a given type is allowed.";
                throw new ArgumentException(message, nameof(type));
            }

            _singletonCache.Add(type, service);
        }

        public void UseMediaStorage<T>()
            where T : ILightMediaStorage, new()
        {
            AddToCache(WorkbenchServiceType.MediaStorage, new T());
        }

        public void UseRepository<T>()
            where T : ILightRepository, new()
        {
            AddToCache(WorkbenchServiceType.Repository, new T());
        }

        public void UseMessageBus<T>()
            where T : ILightWorker, new()
        {
            AddToCache(WorkbenchServiceType.Worker, new T());
        }

        public void UseTelemetry<T>()
            where T : ILightTelemetry, new()
        {
            AddToCache(WorkbenchServiceType.Telemetry, new T());
        }

        public void UseEventHandler<T>()
            where T : ILightEvent, new()
        {
            AddToCache(WorkbenchServiceType.EventHandler, new T());
        }

        public void UseCache<T>()
            where T : ILightCache, new()
        {
            AddToCache(WorkbenchServiceType.Cache, new T());
        }

        public void UseLogger<T>()
            where T : ILightLogger, new()
        {
            AddToCache(WorkbenchServiceType.Logger, new T());
        }

        public IWorkbenchService GetRegisteredService(WorkbenchServiceType serviceType)
        {
            if (!_isInitialized)
            {
                // Trigger initializations over Workbench Services on demand
                InitializeServices();
            }

            _singletonCache.TryGetValue(serviceType, out var service);

            return service;
        }

        /// <summary>
        /// Prepare Workbench to start Unit Test.
        /// </summary>
        /// <param name="settingsFileName">Name of settings file in Unit Test Project in current directory.</param>
        public void PrepareUnitTestMode(string settingsFileName = "appsettings.json")
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(settingsFileName)
               .Build();

            Configuration = config;
        }

        /// <summary>
        /// Initializes cartridges before run test.
        /// </summary>
        public void RunUnitTestMode()
        {
            InitializeServices();
        }

        /// <summary>
        /// Resets the service cache by removing all cached services.
        /// </summary>
        public void Reset()
        {
            _singletonCache.Clear();
            _isInitialized = false;
        }

        private T GetService<T>(WorkbenchServiceType type, bool mandatory = true)
        {
            if (!_singletonCache.TryGetValue(type, out var service))
            {
                if (mandatory)
                {
                    throw new ArgumentException($"No Workbench service of type '{type.ToString()}' was injected on Startup.");
                }
            }

            return (T)service;
        }

        private void InitializeServices()
        {
            // Foreach service registered on WorkBench cache, the Initialize method should be called to apply specific configurations
            foreach (var serviceType in _singletonCache.Keys)
            {
                _singletonCache.TryGetValue(serviceType, out var service);
                service.Initialize();
            }

            // Prevent initialization to run twice
            _isInitialized = true;
        }
    }
}
