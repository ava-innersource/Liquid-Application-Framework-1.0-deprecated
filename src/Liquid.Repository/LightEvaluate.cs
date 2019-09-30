using Liquid.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Liquid.Repository
{
    public abstract class LightEvaluate : ILightEvaluate
    {
        /// <summary>
        /// Method responsible for take a expression and return a list with all operations for can be used on AWS and Google Cloud.
        /// </summary>
        /// <typeparam name="T">Model for be analysed</typeparam>
        /// <param name="expression">expression for transform all operations</param>
        /// <returns></returns>
        public abstract IEnumerable<dynamic> Evaluate<T>(Expression<Func<T, bool>> expression);
    }
}
