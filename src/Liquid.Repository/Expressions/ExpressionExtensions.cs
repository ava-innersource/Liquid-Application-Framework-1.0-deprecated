using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Liquid.Repository
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Class responsible to update parameters
        /// </summary>
        private class RebindParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="oldParameter"></param>
            /// <param name="newParameter"></param>
            public RebindParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            /// <summary>
            /// Verify parameter
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == _oldParameter)
                    return _newParameter;

                return base.VisitParameter(node);
            }
        }

        /// <summary>
        /// Where clause to aggregate lambda expressions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Where<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var expr2Body = new RebindParameterVisitor(expr2.Parameters[0], expr1.Parameters[0]).Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, expr2Body), expr1.Parameters);
        }

        /// <summary>
        /// Where clause to aggregate lambda expressions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereOr<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var expr2Body = new RebindParameterVisitor(expr2.Parameters[0], expr1.Parameters[0]).Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.Or(expr1.Body, expr2Body), expr1.Parameters);
        }
    }
}
