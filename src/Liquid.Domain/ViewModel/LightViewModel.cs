using FluentValidation;
using Liquid.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Liquid.Runtime;

namespace Liquid.Domain
{
    public abstract class LightViewModel<T> : ILightViewModel where T : LightViewModel<T>, ILightViewModel, new()
    {
        protected LightViewModel()
        {
            ValidateInstances();
        }

        private List<string> _inputErrors;

        [JsonIgnore]
        public List<string> InputErrors
        {
            get { return _inputErrors; }
            set { _inputErrors = value; }
        }

        /// <summary>
        /// The method receive the error code to add on errors list of input validation.
        /// </summary>
        /// <param name="error">The code error</param>
        protected void AddInputValidationErrorCode(string error)
        {
            _inputErrors.Add(error);
        }


        /// <summary>
        /// The properties used to return the InputValidator.
        /// </summary>
        [JsonIgnore]
        public InputValidator<T> Validator { get; } = new InputValidator<T>();

        /// <summary>
        /// The method used to input validation of ViewModel.
        /// </summary>
        ///  <remarks>Must be implemented in each derived class.</remarks>
        public abstract void Validate();
    
        /// <summary>
        /// The method used to input validation of ViewModel on FluentValidation.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return Validator.RuleFor(expression);
        }

        /// <summary>
        ///  Method used to map data from ViewModel to Model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void MapFrom(ILightModel data)
        {
            this.DynamicHelperData(data);
        }

        /// <summary>
        /// Method used to map data from ViewModel to ViewModel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void MapFrom(ILightViewModel data)
        {
            this.DynamicHelperData(data);
        }

        /// <summary>
        /// Method used to map data from ViewModel to ViewModel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void MapFrom(ILightValueObject data)
        {
            this.DynamicHelperData(data);
        }

        /// <summary>
        ///  Method used to map data from ViewModel or ValueObject to Model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void MapTo(ILightValueObject data)
        {
            this.DynamicHelperData(data);
        }

        /// <summary>
        ///  Method used to map data from ViewModel or Model to Model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void MapTo(ILightModel data)
        {
            this.DynamicHelperData(data);
        }

        /// <summary>
        /// Method used to map data from ViewModel or Model to ViewModel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void MapTo(ILightViewModel data)
        {
            this.DynamicHelperData(data);
        }

        /// <summary>
        /// Method responsible for dynamic mapping between objects
        /// </summary>
        /// <param name="data"></param>
        private void DynamicHelperData(dynamic data)
        { 
            ///By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (FieldInfo fieldInfo in data.GetType().GetFields())
            {
                
                dynamic value = fieldInfo.GetValue(data);
                if (value != null)
                {
                    FieldInfo field = GetFieldByNameAndType(this, fieldInfo.Name, fieldInfo.FieldType.Name);
                    if (field != null)
                        field.SetValue(this, value);
                }
            } 
            ///By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (PropertyInfo propertyInfo in data.GetType().GetProperties())
            {
                dynamic value = propertyInfo.GetValue(data);
                if (value != null)
                {
                    PropertyInfo field = GetPropertyByNameAndType(this, propertyInfo.Name, propertyInfo.PropertyType.Name);
                    if (field != null)
                        field.SetValue(this, value);
                }
            }
        }
        /// <summary>
        /// From an object it verifies the parameter informed, it has the same name and data type.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private FieldInfo GetFieldByNameAndType(dynamic data, String name, String type)
        {
            FieldInfo retorno = null;
            ///By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (FieldInfo fieldInfo in data.GetType().GetFields())
            {
                if (fieldInfo.Name.Equals(name) && fieldInfo.FieldType.Name.Equals(type))
                {
                    retorno = fieldInfo;
                    break;
                }
            }
            return retorno;
        }
        /// <summary>
        /// From an object it verifies the parameter informed, it has the same name and data type.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private PropertyInfo GetPropertyByNameAndType(dynamic data, String name, String type)
        {
            PropertyInfo retorno = null;
            ///By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (PropertyInfo propertyInfo in data.GetType().GetProperties())
            {
                if (propertyInfo.Name.Equals(name) && propertyInfo.PropertyType.Name.Equals(type))
                {
                    retorno = propertyInfo;
                    break;
                }
            }
            return retorno;
        }
        private void ValidateInstances()
        {
            if (!typeof(T).IsInstanceOfType(this))
            {
                throw new TypeInitializationException(this.GetType().FullName, null);
            }
        }
    }
}
