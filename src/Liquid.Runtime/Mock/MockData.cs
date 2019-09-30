using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Liquid.Runtime
{
    public class MockData
    {
        private static readonly string _folder = "Mock";

        /// <summary>
        /// Get Mock files on MockDatabase folder by environment
        /// </summary> 
        /// <returns>Strings with mock files</returns>
        public static string[] GetMockFiles()
        { 
            string sufix = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");   
            if (Directory.Exists($"{Directory.GetCurrentDirectory()}/{_folder}/reseed"))
            { 
                return Directory.GetFiles($"{Directory.GetCurrentDirectory()}/{_folder}/reseed")
                    .Select(file => Path.GetFileName(file)).ToArray(); 
            }  
            return new string[0]; 
        }
        /// <summary>
        /// Get Mock files on MockDatabase folder by environment
        /// </summary>
        /// <param name="name">name of file</param>
        /// <returns>Generic type sended</returns>
        public T GetMockData<T>(string name)
        {
            string json = "";
            string sufix = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (File.Exists($"{Directory.GetCurrentDirectory()}/{_folder}/{name}.{sufix}.json"))
                json = File.ReadAllText($"{Directory.GetCurrentDirectory()}/{_folder}/{name}.{sufix}.json");
            else if (File.Exists($"{Directory.GetCurrentDirectory()}/{_folder}/{name}.json"))
                json = File.ReadAllText($"{Directory.GetCurrentDirectory()}/{_folder}/{name}.json");

            return JsonConvert.DeserializeObject<T>(ConvertDynamicTags(json));

        }

        /// <summary>
        /// Get Mock files on MockDatabase folder by environment
        /// </summary>
        /// <param name="name">File Name</param>
        /// <param name="environmentName">Environment ame</param>
        /// <returns>Generic type sended</returns>
        public T GetSpecificData<T>(string name, string environmentName)
        {
            string json = "";

            if (File.Exists($"{Directory.GetCurrentDirectory()}/{_folder}/{name}.{environmentName}.json"))
                json = File.ReadAllText($"{Directory.GetCurrentDirectory()}/{_folder}/{name}.{environmentName}.json");
            else if (File.Exists($"{Directory.GetCurrentDirectory()}/{_folder}/{name}.json"))
                json = File.ReadAllText($"{Directory.GetCurrentDirectory()}/{_folder}/{name}.json");

            return JsonConvert.DeserializeObject<T>(ConvertDynamicTags(json));
        }

        /// <summary>
        /// Convert the tags on dynamic data {{TODAY-n days}} and {{NOW -n minutes}} to current value
        /// </summary>
        /// <param name="jsonString">string contained the tags</param>
        /// <returns>Return the string with replace Tags</returns>
        private static string ConvertDynamicTags(string jsonString)
        {
            DateTime today = DateTime.Today;
            DateTime now = DateTime.UtcNow;

            jsonString = jsonString.Replace("{{TODAY}}", today.ToString("yyyy-MM-dd"));
            jsonString = jsonString.Replace("{{NOW}}", now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            MatchCollection todays = Regex.Matches(jsonString, @"\{{TODAY[\-|\+][0-9]+}}", RegexOptions.None);
            foreach (Match item in todays)
            {
                DateTime newDate = today.AddDays(Double.Parse(Regex.Match(item.Value, @"[\-|\+][0-9]+").Value));
                jsonString = jsonString.Replace(item.Value, newDate.ToString("yyyy-MM-dd"));
            }

            MatchCollection nows = Regex.Matches(jsonString, @"\{{NOW[\-|\+][0-9]+}}", RegexOptions.None);
            foreach (Match item in nows)
            {
                DateTime newDate = now.AddMinutes(Double.Parse(Regex.Match(item.Value, @"[\-|\+][0-9]+").Value));
                jsonString = jsonString.Replace(item.Value, newDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }

            return jsonString;
        }
    }
}
