
using Liquid.Base;
using Liquid.Base.Interfaces.Polly;
using Liquid.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Liquid
{
    public static class WorkBench
    {
        /// <summary>
        /// _singletonCache exposed to be used by Health Check
        /// </summary>
        public static Dictionary<WorkBenchServiceType, IWorkBenchService> _singletonCache = new Dictionary<WorkBenchServiceType, IWorkBenchService>();
        public static ILightRepository Repository => GetService<ILightRepository>(WorkBenchServiceType.Repository);
		public static ILightMediaStorage MediaStorage => GetService<ILightMediaStorage>(WorkBenchServiceType.MediaStorage);
        public static ILightTelemetry Telemetry => GetService<ILightTelemetry>(WorkBenchServiceType.Telemetry);
        public static ILightCache Cache => GetService<ILightCache>(WorkBenchServiceType.Cache, false);
        public static ILightEvent Event => GetService<ILightEvent>(WorkBenchServiceType.EventHandler, false);
        public static ILightLogger Logger => GetService<ILightLogger>(WorkBenchServiceType.Logger, false);

        private static IConfiguration _configuration = null;
        public static IConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
            set
            {
                _configuration = value;

                // Trigger initializations over Workbench Services as soon 
                // as the configuration is available.
                //InitializeServices();
            }
        }

        // Indicates if the Workbench Services are initialized.
        private static bool _isServicesUp = false;

        public static ILightPolly Polly { get; set; }

        public static void AddToCache(WorkBenchServiceType singletonType, IWorkBenchService singleton)
        {
            if (_singletonCache.ContainsKey(singletonType))
                throw new ArgumentException("type", $"The SingletonType '{singletonType.ToString()}' has been already set. Only one Workbench service of a given type is allowed.");

            _singletonCache.Add(singletonType, singleton);
        }

        public static void UseMediaStorage<T>() where T : ILightMediaStorage, new()
        {
            AddToCache(WorkBenchServiceType.MediaStorage, new T());
        }

        public static void UseRepository<T>() where T : ILightRepository, new()
        {
            AddToCache(WorkBenchServiceType.Repository, new T());
        }

        public static void UseMessageBus<T>() where T : ILightWorker, new()
        {
            AddToCache(WorkBenchServiceType.Worker, new T());
        }

        public static void UseTelemetry<T>() where T : ILightTelemetry, new()
        {
            AddToCache(WorkBenchServiceType.Telemetry, new T());
        }

        public static void UseEnventHandler<T>() where T : ILightEvent, new()
        {
            AddToCache(WorkBenchServiceType.EventHandler, new T());
        }

        public static void UseCache<T>() where T : ILightCache, new()
        {
            AddToCache(WorkBenchServiceType.Cache, new T());
        }

        public static void UseLogger<T>() where T : ILightLogger, new()
        {
            AddToCache(WorkBenchServiceType.Logger, new T());
        }

        public static object GetRegisteredService(WorkBenchServiceType serviceType)
        {
            if (!_isServicesUp)
            {
                // Trigger initializations over Workbench Services on demand
                InitializeServices();
            }

            IWorkBenchService IWorkBenchService;

            _singletonCache.TryGetValue(serviceType, out IWorkBenchService);

            return IWorkBenchService != null ? IWorkBenchService : null;
        }

        internal static T GetService<T>(WorkBenchServiceType singletonType, Boolean mandatoryParam = true)
        {
            IWorkBenchService service;
            if (!_singletonCache.TryGetValue(singletonType, out service))
            {
                if (mandatoryParam)
                    throw new ArgumentException($"No Workbench service of type '{singletonType.ToString()}' was injected on Startup.");
            }

            return (T)service;
        }

        private static void InitializeServices()
        {
            // Foreach service registered on WorkBench cache, the Initialize method should be called to apply specific configurations
            foreach (WorkBenchServiceType serviceType in _singletonCache.Keys)
            {
                IWorkBenchService IWorkBenchService;
                _singletonCache.TryGetValue(serviceType, out IWorkBenchService);
                IWorkBenchService.Initialize();
            }

            //prevent from discoveries to run twice or more
            _isServicesUp = true;
        }

        /// <summary>
        /// Prepare Workbench to start Unit Test
        /// </summary>
        /// <param name="settingsFileName">Name of settings file in Unit Test Project in current directory</param>
        public static void PrepareUnitTestMode(string settingsFileName = "appsettings.json")
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(settingsFileName)
               .Build();

            Configuration = config;
        }

        /// <summary>
        /// Initializes cartridges before run test
        /// </summary>
        public static void RunUnitTestMode()
        {
            InitializeServices();
        }

        /// <summary>
        /// Resets the service cache by removing all cached services.
        /// </summary>
        public static void Reset()
        {
            _singletonCache.Clear();
            _isServicesUp = false;
        }
    }
}