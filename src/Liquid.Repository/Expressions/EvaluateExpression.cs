using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Liquid.Repository
{
    public class ExpressionAnalyzed
    {
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }
        public string PropertyType { get; set; }
        public ExpressionProperty OperatorBetweenPropAndValue { get; set; }
        public ExpressionProperty LogicalOperator { get; set; }
    }

    public class ExpressionProperty
    {
        public string Operator { get; set; }
        public ExpressionType ExpressionType { get; set; }
    }

    public class EvaluateExpression : LightEvaluate
    {
        private readonly string _regexAnything = ".*";
        private readonly string _regexWithWhiteSpace = "\\s";
        private IEnumerable<ExpressionProperty> _knowExpressionsTypes => KnowOperator();
        private (int, int, ExpressionProperty, ExpressionProperty) GetOperator(string text)
        {
            ExpressionProperty operatorBetweenPropsAndValues = this._knowExpressionsTypes.Where(x => Regex.IsMatch(text, 
                $"{_regexAnything}{_regexWithWhiteSpace}{ Regex.Escape(x.Operator)}{_regexWithWhiteSpace}{_regexAnything}"))
                .FirstOrDefault();

            ExpressionProperty operatorExpression = default(ExpressionProperty);
            this._knowExpressionsTypes.ToList().ForEach(x => {

                if (Regex.IsMatch(text, $"{_regexAnything}{_regexWithWhiteSpace}{ Regex.Escape(x.Operator)}{_regexWithWhiteSpace}{_regexAnything}"))
                {
                    operatorExpression = new ExpressionProperty() { Operator = x.Operator, ExpressionType = x.ExpressionType };
                }
            });

            return (text.IndexOf(operatorBetweenPropsAndValues.Operator), operatorBetweenPropsAndValues.Operator.Length, operatorBetweenPropsAndValues, operatorExpression);
        }

        public override IEnumerable<dynamic> Evaluate<T>(Expression<Func<T, bool>> expression)
        {
            Type type = expression.Parameters.FirstOrDefault().Type;

            List<ExpressionAnalyzed> expressions = new List<ExpressionAnalyzed>();

            string resolvedMember = string.Empty;

            string[] arrayExpression = expression.ToString().Trim().Split('.');

            for (int i = 1; i < arrayExpression.Count(); i++)
            {
                (int, int, ExpressionProperty, ExpressionProperty) operators = GetOperator(arrayExpression[i]);

                string PropertyName = arrayExpression[i].Substring(0, operators.Item1).Replace(" ", "").Trim();

                PropertyInfo member = type.GetProperties().Where(x => x.Name == PropertyName).FirstOrDefault();

                if (member != null)
                {
                    if (member.PropertyType.IsPrimitive || member.PropertyType == typeof(Decimal) || member.PropertyType == typeof(String) || member.PropertyType == typeof(string))
                    {
                        resolvedMember = member.PropertyType.Name;
                    }
                }
                else
                {
                    FieldInfo fieldInfo = type.GetField(PropertyName);

                    if (fieldInfo != null)
                    {
                        if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType == typeof(Decimal) || fieldInfo.FieldType == typeof(String) || fieldInfo.FieldType == typeof(string))
                        {
                            resolvedMember = fieldInfo.FieldType.Name;
                        }
                    }
                }


                var PropretyValue = arrayExpression[i].Substring(PropertyName.Length + operators.Item2 + 1, arrayExpression[i].IndexOf(')') - PropertyName.Length - operators.Item2 - 1).Replace("\"", "").Trim();

                expressions.Add(new ExpressionAnalyzed() { PropertyName = PropertyName,
                                                           PropertyValue = PropretyValue,
                                                           PropertyType = resolvedMember,
                                                           OperatorBetweenPropAndValue = operators.Item3,
                                                           LogicalOperator = operators.Item4
                });
            }

            return expressions.AsEnumerable();
        }

        private IEnumerable<ExpressionProperty> KnowOperator()
        {
            List<ExpressionProperty> list = new List<ExpressionProperty>
            {
                new ExpressionProperty { Operator = "+", ExpressionType = ExpressionType.Add },
                new ExpressionProperty { Operator = "&", ExpressionType = ExpressionType.And },
                new ExpressionProperty { Operator = "&&", ExpressionType = ExpressionType.AndAlso },
                new ExpressionProperty { Operator = "/", ExpressionType = ExpressionType.Divide },
                new ExpressionProperty { Operator = "==", ExpressionType = ExpressionType.Equal },
                new ExpressionProperty { Operator = ">", ExpressionType = ExpressionType.GreaterThan },
                new ExpressionProperty { Operator = ">=", ExpressionType = ExpressionType.GreaterThanOrEqual },
                new ExpressionProperty { Operator = "<", ExpressionType = ExpressionType.LessThan },
                new ExpressionProperty { Operator = "<=", ExpressionType = ExpressionType.LessThanOrEqual },
                new ExpressionProperty { Operator = "%", ExpressionType = ExpressionType.Modulo },
                new ExpressionProperty { Operator = "*", ExpressionType = ExpressionType.Multiply },
                new ExpressionProperty { Operator = "-", ExpressionType = ExpressionType.Negate },
                new ExpressionProperty { Operator = "!", ExpressionType = ExpressionType.Not },
                new ExpressionProperty { Operator = "!=", ExpressionType = ExpressionType.NotEqual },
                new ExpressionProperty { Operator = "|", ExpressionType = ExpressionType.Or },
                new ExpressionProperty { Operator = "||", ExpressionType = ExpressionType.OrElse },
                new ExpressionProperty { Operator = "-", ExpressionType = ExpressionType.Subtract },
                new ExpressionProperty { Operator = "??", ExpressionType = ExpressionType.Coalesce },
                new ExpressionProperty { Operator = "^", ExpressionType = ExpressionType.ExclusiveOr }
            };

            return list.AsEnumerable();
        }

    }
}
