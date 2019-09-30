using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Liquid.Runtime.Configuration
{
    class LiquidConfiguration
    {
        private static string _CONFIGPATH = "\\Config\\";

        public T GetConfiguration<T>(string configuration)
        {
            JObject objConfig = LoadConfigObject(configuration);

            return objConfig.ToObject<T>();
        }

        public JObject GetConfiguration(string configuration)
        {
            return LoadConfigObject(configuration);
        }

        private static JObject LoadConfigObject(string configuration)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            string configPath = $"{Directory.GetCurrentDirectory()}{_CONFIGPATH}{configuration}.{environment}.json";
            if (!File.Exists(configPath))
            {
                configPath = $"{Directory.GetCurrentDirectory()}{_CONFIGPATH}{configuration}.json";
            }

            JObject objConfig = JObject.Parse(File.ReadAllText(configPath));
            return objConfig;
        }

    }
}
