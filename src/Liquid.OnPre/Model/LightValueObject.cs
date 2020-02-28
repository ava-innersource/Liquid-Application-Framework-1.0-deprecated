using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Liquid.Domain;
using Liquid.Interfaces;
using Liquid.Runtime;
using Newtonsoft.Json;

namespace Liquid.Repository
{
    public abstract class LightValueObject<T> : ILightValueObject where T : LightValueObject<T>, ILightValueObject, new()
    {
        private List<string> _inputErrors;

        protected LightValueObject()
        {
            ValidateInstances();
        }

        [JsonIgnore]
        public List<string> InputErrors
        {
            get { return _inputErrors; }
            set { _inputErrors = value; }
        }

        /// <summary>
        /// The method receive the error code to add on errors list of validation.
        /// </summary>
        /// <param name="error">The code error</param>
        protected void AddModelValidationErrorCode(string error)
        {
            _inputErrors.Add(error);
        }

        
        /// <summary>
        /// The properties used to return the InputValidator.
        /// </summary>
        [JsonIgnore]
        public Validator<T> Validator { get; } = new Validator<T>();

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
        /// Method used for mapping between Model to ViewModel 
        /// </summary>
        /// <param name="data"></param>
        public void MapFrom(ILightViewModel data)
        {
            this.DynamicHelperData(data);
        }

        /// <summary>
        /// Method used to map data from Model to ViewModel.
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
                    FieldInfo filed = this.GetFieldByNameAndType(this, fieldInfo.Name, fieldInfo.FieldType.Name);
                    if (filed != null)
                        filed.SetValue(this, value);
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
        /// <summary>
        /// Method used to create new ViewModel object from a Model.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public LightViewModel<U> MapTo<U>() where U : LightViewModel<U>, ILightViewModel, new()
        {
            U viewModel = new U();

            ///By reflection, browse viewModel by identifying all attributes and lists for validation.  
            foreach (FieldInfo fieldInfo in this.GetType().GetFields())
            {
                dynamic value = fieldInfo.GetValue(this);
                if (value != null)
                {
                    FieldInfo field = this.GetFieldByNameAndType(viewModel, fieldInfo.Name, fieldInfo.FieldType.Name);
                    if (field != null)
                        field.SetValue(viewModel, value);
                }
            }

            foreach (PropertyInfo fieldInfo in this.GetType().GetProperties())
            {
                dynamic value = fieldInfo.GetValue(this);
                if (value != null)
                {
                    PropertyInfo field = this.GetPropertyByNameAndType(viewModel, fieldInfo.Name, fieldInfo.PropertyType.Name);
                    if (field != null)
                        field.SetValue(viewModel, value);
                }
            }
            return viewModel;
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
