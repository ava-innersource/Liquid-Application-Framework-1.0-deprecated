using FluentValidation;
using System;
using System.Linq.Expressions;

namespace Liquid.Runtime.Configuration
{
    public abstract class LightConfig<T> where T : LightConfig<T>
    {
        /// <summary>
        /// The properties used to return the InputValidator.
        /// </summary>
        public ConfigValidator<T> Validator { get; } = new ConfigValidator<T>();

        /// <summary>
        /// The method used to validate Configuration Objects.
        /// </summary>
        ///  <remarks>Must be implemented in each derived class.</remarks>
        public abstract void Validate();

        /// <summary>
        /// The method used to define validation for settings retrieved from Configuration.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return Validator.RuleFor(expression);
        }
    }
}
