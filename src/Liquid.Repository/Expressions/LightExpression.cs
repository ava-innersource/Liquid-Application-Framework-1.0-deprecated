using JetBrains.Annotations;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Liquid.Repository
{
    /// <summary>
    /// LightExpression{T} which abstracts a LINQ Expression and allows the user to build complex queries by extending less complex ones.
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    public class LightExpression<T>
    {
        /// <summary>
        /// This constructor stands for the simplest Expression, such f => true.
        /// </summary>
        internal LightExpression()
        {
            DefaultExpression = (f => true);
        }

        /// <summary>
        /// Initializes a LightExpression from a given LINQ Expression
        /// </summary>
        /// <param name="exp"></param>
        internal LightExpression(Expression<Func<T, bool>> exp) : this()
        {
            _predicate = exp;
        }

        /// <summary> Start an expression </summary>        
        /// <param name="expr"></param>
        /// <returns></returns>
        public static LightExpression<T> New(Expression<Func<T, bool>> expr = null) { return new LightExpression<T>(expr); }

        /// <summary>The actual Predicate. It can only be set by calling Start.</summary>
        private Expression<Func<T, bool>> Predicate => IsStarted ? _predicate : DefaultExpression;

        private Expression<Func<T, bool>> _predicate;

        /// <summary>
        /// Determines if the predicate is started.
        /// </summary>
        public bool IsStarted => _predicate != null;

        /// <summary> 
        /// A default expression to use only when the expression is null 
        /// </summary>
        public bool UseDefaultExpression => DefaultExpression != null;

        /// <summary>
        /// The default expression
        /// </summary>
        public Expression<Func<T, bool>> DefaultExpression { get; set; }

        /// <summary>
        /// And
        /// </summary>
        public Expression<Func<T, bool>> Where(Expression<Func<T, bool>> expr2)
        {
            if (IsStarted)
                _predicate = Predicate.Where(expr2);
            else
                _predicate = expr2;

            return _predicate;
        }
        /// <summary>
        /// Or
        /// </summary>
        public Expression<Func<T, bool>> WhereOr(Expression<Func<T, bool>> expr2)
        {
            if (IsStarted)
                _predicate = Predicate.WhereOr(expr2);
            else
                _predicate = expr2;

            return _predicate;
        }

        /// <summary> 
        /// Show predicate string 
        /// </summary>
        public override string ToString()
        {
            return Predicate?.ToString();
        }

        #region Implicit Operators
        /// <summary>
        /// Allows this object to be implicitely converted to an Expression{Func{T, bool}}.
        /// </summary>
        /// <param name="right"></param>
        public static implicit operator Expression<Func<T, bool>>(LightExpression<T> right)
        {
            return right?.Predicate;
        }

        /// <summary>
        /// Allows this object to be implicitely converted to an Expression{Func{T, bool}}.
        /// </summary>
        /// <param name="right"></param>
        public static implicit operator Func<T, bool>(LightExpression<T> right)
        {
            if (right == null)
                return null;

            return (right.IsStarted || right.UseDefaultExpression) ? right.Predicate.Compile() : null;
        }

        /// <summary>
        /// Allows this object to be implicitely converted to an Expression{Func{T, bool}}.
        /// </summary>
        /// <param name="right"></param>
        public static implicit operator LightExpression<T>(Expression<Func<T, bool>> right)
        {
            return right == null ? null : new LightExpression<T>(right);
        }
        #endregion

        #region Implement LamdaExpression methods and properties

        /// <summary>
        /// Body
        /// </summary>
        public Expression Body => Predicate.Body;

        /// <summary>
        /// Node Type
        /// </summary>
        public ExpressionType NodeType => Predicate.NodeType;

        /// <summary>
        /// Parameters
        /// </summary>
        public ReadOnlyCollection<ParameterExpression> Parameters => Predicate.Parameters;

        /// <summary>
        /// Type
        /// </summary>
        public Type Type => Predicate.Type;

        #endregion
    }
}
