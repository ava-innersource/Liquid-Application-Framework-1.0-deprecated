using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Liquid.Domain
{
    /// <summary>
    /// This class is responsible to provide localization capability over json files
    /// </summary>
    internal class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, Lazy<JObject>> _resourceObjectCache =
            new ConcurrentDictionary<string, Lazy<JObject>>();

        private readonly string _baseName = "Resources";
        private readonly string _fileName = "localization";

        private readonly IEnumerable<string> _resourceFileLocations;

        private JsonStringLocalizer()
        {
            var baseName = $"{_baseName}{Path.DirectorySeparatorChar}{_fileName}";
            // Get a list of possible resource file locations.
            _resourceFileLocations = LocalizerUtil.ExpandPaths(baseName, string.Empty).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string Localize(string code)
        {
            var localizer = new JsonStringLocalizer();
            return localizer[code];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Localize(string code, object[] args)
        {
            return string.Format(Localize(code), args);
        }

        public virtual LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetLocalizedString(name, CultureInfo.CurrentUICulture);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public virtual LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var format = GetLocalizedString(name, CultureInfo.CurrentUICulture);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }

        public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

        protected IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            if (culture == null)
            {
                return new JsonStringLocalizer();
            }
            throw new NotImplementedException();
        }

        protected string GetLocalizedString(string name, CultureInfo culture)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            // Attempt to get resource with the given name from the resource object. if not found, try parent
            // resource object until parent begets himself.
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo previousCulture = null;
            do
            {
                var resourceObject = GetResourceObject(currentCulture);
                if (resourceObject != null)
                {
                    JToken value;
                    if (resourceObject.TryGetValue(name, out value))
                    {
                        var localizedString = value.ToString();
                        return localizedString;
                    }
                }

                // Consult parent culture.
                previousCulture = currentCulture;
                currentCulture = currentCulture.Parent;

            } while (previousCulture != currentCulture);

            return null;
        }

        private JObject GetResourceObject(CultureInfo currentCulture)
        {
            if (currentCulture == null)
            {
                throw new ArgumentNullException(nameof(currentCulture));
            }

            var cultureSuffix = "." + currentCulture.Name;
            cultureSuffix = cultureSuffix == "." ? "" : cultureSuffix;

            var lazyJObjectGetter = new Lazy<JObject>(() =>
            {
                // First attempt to find a resource file location that exists.
                string resourcePath = null;
                foreach (var resourceFileLocation in _resourceFileLocations)
                {
                    resourcePath = resourceFileLocation + cultureSuffix + ".json";
                    if (File.Exists(resourcePath))
                    {
                        break;
                    }
                    else
                    {
                        resourcePath = null;
                    }
                }

                if (resourcePath == null)
                {
                    return null;
                }

                // Found a resource file path: attempt to parse it into a JObject.
                try
                {
                    var resourceFileStream =
                        new FileStream(resourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    using (resourceFileStream)
                    {
                        var resourceReader =
                            new JsonTextReader(new StreamReader(resourceFileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true));
                        using (resourceReader)
                        {
                            return JObject.Load(resourceReader);
                        }
                    }
                }
                catch
                {
                    return null;
                }

            }, LazyThreadSafetyMode.ExecutionAndPublication);

            lazyJObjectGetter = _resourceObjectCache.GetOrAdd(cultureSuffix, lazyJObjectGetter);
            var resourceObject = lazyJObjectGetter.Value;
            return resourceObject;
        }
    }

    static class LocalizerUtil
    {
        public static string TrimPrefix(string name, string prefix)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name.Substring(prefix.Length);
            }
            return name;
        }

        public static IEnumerable<string> ExpandPaths(string name, string baseName)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (baseName == null) throw new ArgumentNullException(nameof(baseName));

            return ExpandPathIterator(name, baseName);
        }


        private static IEnumerable<string> ExpandPathIterator(string name, string baseName)
        {
            StringBuilder expansion = new StringBuilder();

            // Start replacing periods, starting at the beginning.
            var components = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < components.Length; i++)
            {
                for (var j = 0; j < components.Length; j++)
                {
                    expansion.Append(components[j]);

                    AppendSeparator(expansion, i, j);
                }
                // Remove trailing period.
                yield return expansion.Remove(expansion.Length - 1, 1).ToString();
                expansion.Clear();
            }

            // Do the same with the name where baseName prefix is removed.
            var nameWithoutPrefix = TrimPrefix(name, baseName);
            if (nameWithoutPrefix != string.Empty && nameWithoutPrefix != name)
            {
                nameWithoutPrefix = nameWithoutPrefix.Substring(1);
                var componentsWithoutPrefix = nameWithoutPrefix.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < componentsWithoutPrefix.Length; i++)
                {
                    for (var j = 0; j < componentsWithoutPrefix.Length; j++)
                    {
                        expansion.Append(componentsWithoutPrefix[j]);

                        AppendSeparator(expansion, i, j);
                    }
                    // Remove trailing period.
                    yield return expansion.Remove(expansion.Length - 1, 1).ToString();
                    expansion.Clear();
                }
            }
        }

        private static void AppendSeparator(StringBuilder expansion, int i, int j)
        {
            var separator = j < i ? Path.DirectorySeparatorChar : '.';
            expansion.Append(separator);
        }
    }
}
