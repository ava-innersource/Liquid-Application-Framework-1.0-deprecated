using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Activation
{
    /// <summary>
    /// Attribute used for definy the domain model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ModelSpecificationAttribute : Attribute
    {

        /// <summary>
        /// Constructor used to Domain Model Specification.
        /// </summary>
        public ModelSpecificationAttribute()
        {
        }

		/// <summary>
		/// Property for put the description of property 
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Another overhead for when the user need put some description on 
		/// </summary>
		/// <param name="description"></param>
		public ModelSpecificationAttribute(string description)
		{
			Description = description;
		}

	}
}
