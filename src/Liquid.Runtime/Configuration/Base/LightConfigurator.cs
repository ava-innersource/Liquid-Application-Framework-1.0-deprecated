using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Liquid.Runtime.Configuration.Base
{
    /// <summary>
    /// Validates the Section Property from LightAPI
    /// </summary>
    public static class LightConfigurator
    {
        /// <summary>
        /// Is Responsible for get the section from the Workbench 
        /// and validate the configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <returns>The section configuration object</returns>
        public static T Config<T>(string section) where T : LightConfig<T>
        {
            /// Load given section from Workbench
            var config = WorkBench.Configuration.GetSection(section).Get<T>();

            if (config == null)
            {
                throw new ArgumentException($"Not found a valid configuration section with name '{section}'.", "section");
            }
            ///Verify if there's any errors from the configuration
            var validationErrors = ValidateConfig(config);

            if (validationErrors.Count > 0)
            {///if there's any exception trhows the errors
                throw new InvalidConfigurationException(validationErrors);
            }
            ///returns the section configuration
            return config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <returns>Exception List</returns>
        private static List<string> ValidateConfig<T>(T config) where T : LightConfig<T>
        {
            List<string> _inputValidationErrors = new List<string>();

            config.Validate();
            var result = config.Validator.Validate(config);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    /// The method receive the error code to add on errors list of input validation.
                    _inputValidationErrors.Add(error);
                }
            }
            ///By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (FieldInfo fieldInfo in config.GetType().GetFields())
            {
                var children = fieldInfo.GetValue(config) as IList;
                if(children != null)
                {
                    ///Validate each of its members 
                    foreach (var item in children)
                    {
                        if ((item.GetType().BaseType != typeof(object))
                                && (item.GetType().BaseType.GetGenericTypeDefinition() == typeof(LightConfig<>)))
                        {
                            dynamic obj = item;
                                /// The method receive the error code to add on errors list of input validation.
                                _inputValidationErrors.AddRange(ValidateConfig(obj));
                        }
                    }
                }
            }

            return _inputValidationErrors;
        }

    }
}
