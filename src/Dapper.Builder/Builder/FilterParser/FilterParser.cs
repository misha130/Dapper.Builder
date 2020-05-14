using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dapper.Builder.Services
{
    public class FilterParser<TEntity> : IFilterParser<TEntity> where TEntity : new()
    {

        protected INamingStrategyService _namingService;
        public FilterParser(INamingStrategyService namingService)
        {
            _namingService = namingService;
        }
        protected virtual string parameterBinding => "@";
        protected string _alias;
        protected Dictionary<Type, string> _parentAliases = new Dictionary<Type, string>();

        public void SetAlias(string alias)
        {
            _alias = alias;
        }

        public void SetParentAlias<UEntity>(string alias) where UEntity : new()
        {
            _parentAliases.Add(typeof(TEntity), alias);
        }
        public QueryResult Parse(Expression<Func<TEntity, bool>> expression, ref int i)
        {
            return Recurse<TEntity>(ref i, expression.Body, true);
        }

        public QueryResult Parse<UEntity>(Expression<Func<TEntity, UEntity, bool>> expression, ref int i)
        where UEntity : new()
        {
            return Recurse<UEntity>(ref i, expression.Body, true);
        }

        public QueryResult Parse<UEntity, WEntity>(Expression<Func<UEntity, WEntity, bool>> expression, ref int i)
        where UEntity : new()
        where WEntity : new()
        {
            return Recurse<UEntity>(ref i, expression.Body, true);
        }

        public QueryResult Parse<UEntity, WEntity>(Expression<Func<UEntity, WEntity, TEntity, bool>> expression, ref int i)
            where UEntity : new()
            where WEntity : new()
        {
            return Recurse<UEntity>(ref i, expression.Body, isUnary: true);
        }


        protected virtual QueryResult Recurse<UEntity>(ref int i, Expression expression, bool isUnary = false, string prefix = null, string postfix = null)
        where UEntity : new()
        {
            if (expression is UnaryExpression)
            {
                var unary = (UnaryExpression)expression;
                return Concat(NodeTypeToString(unary.NodeType), Recurse<UEntity>(ref i, unary.Operand, true));
            }
            if (expression is BinaryExpression)
            {
                var body = (BinaryExpression)expression;
                if (body.NodeType == ExpressionType.ArrayIndex)
                {
                    var arrayValue = Expression.Lambda(body).Compile().DynamicInvoke();
                    return IsParameter(i++, arrayValue);
                }
                else
                {
                    return Concat(Recurse<UEntity>(ref i, body.Left), NodeTypeToString(body.NodeType), Recurse<UEntity>(ref i, body.Right));
                }
            }
            if (expression is ConstantExpression)
            {
                var constant = (ConstantExpression)expression;
                var value = constant.Value;
                if (value is int)
                {
                    return IsSql(value.ToString());
                }
                if (value is string valString)
                {
                    if (prefix == "%")
                    {
                        valString = $"'{prefix}{valString}";
                    }
                    else
                    {
                        valString = $"{prefix}'{valString}";
                    }
                    if (postfix == "%")
                    {
                        valString = $"{valString}{postfix}'";
                    }
                    else
                    {
                        valString = $"{valString}'{postfix}";
                    }
                    return IsSql(valString);
                }
                if (isUnary && value is bool)
                {
                    return Concat(IsParameter(i++, value), " = ", IsSql("1"));
                }
                if (value == null)
                {
                    return IsSql("null");
                }
                return IsParameter(i++, value);
            }
            if (expression is MemberExpression)
            {

                var member = (MemberExpression)expression;
                if (member.Member is FieldInfo)
                {
                    var value = GetValue(member);
                    var parameter = IsParameter(i++, value);
                    if (value is string)
                    {
                        parameter.Query = prefix + parameter.Query + postfix;
                    }
                    return parameter;
                }
                try
                {
                    var value = Expression.Lambda(expression).Compile().DynamicInvoke();
                    var parameter = IsParameter(i++, value);
                    parameter.Query = prefix + parameter.Query + postfix;
                    return parameter;
                }
                catch
                {

                }

                if (member.Member is PropertyInfo)
                {
                    var property = (PropertyInfo)member.Member;
                    var colName = property.Name;
                    if (isUnary && member.Type == typeof(bool))
                    {
                        return Concat(Recurse<UEntity>(ref i, expression), " = ", IsParameter(i++, true));
                    }
                    var type = member.Expression.Type;
                    if (type.IsInterface)
                    {
                        type = typeof(UEntity);
                    }
                    string tableName = string.Empty;
                    if (type == typeof(TEntity) && !string.IsNullOrEmpty(_alias))
                    {
                        tableName = _alias;
                    }
                    else if (_parentAliases.ContainsKey(typeof(UEntity)))
                    {
                        tableName = _parentAliases[typeof(UEntity)];
                    }
                    else
                    {
                        tableName = _namingService.GetTableName(type);
                    }

                    return IsSql($"{prefix}{tableName}.[{colName}]{postfix}");
                }
                if (member.Member is FieldInfo)
                {
                    var value = GetValue(member);
                    if (value is string)
                    {
                        value = prefix + (string)value + postfix;
                    }
                    return IsParameter(i++, value);
                }
                throw new Exception($"Expression does not refer to a property or field: {expression}");
            }
            if (expression is MethodCallExpression)
            {
                var methodCall = (MethodCallExpression)expression;
                // LIKE queries:
                if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                {
                    return Concat(Recurse<UEntity>(ref i, methodCall.Object), " LIKE ", Recurse<UEntity>(ref i, methodCall.Arguments[0], prefix: "%", postfix: "%"));
                }
                if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                {
                    return Concat(Recurse<UEntity>(ref i, methodCall.Object), " LIKE ", Recurse<UEntity>(ref i, methodCall.Arguments[0], postfix: "%"));
                }
                if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                {
                    return Concat(Recurse<UEntity>(ref i, methodCall.Object), " LIKE ", Recurse<UEntity>(ref i, methodCall.Arguments[0], prefix: "%"));
                }
                // IN queries:
                if (methodCall.Method.Name == "Contains")
                {
                    Expression collection;
                    Expression property;
                    if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                    {
                        collection = methodCall.Arguments[0];
                        property = methodCall.Arguments[1];
                    }
                    else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                    {
                        collection = methodCall.Object;
                        property = methodCall.Arguments[0];
                    }
                    else
                    {
                        throw new Exception("Unsupported method call: " + methodCall.Method.Name);
                    }
                    var values = (IEnumerable)GetValue(collection);
                    return Concat(Recurse<UEntity>(ref i, property), " IN ", IsCollection(ref i, values));
                }
                if (methodCall.Method == typeof(DateTime).GetMethod("ToString") && methodCall.Arguments.Count == 1)
                {
                    return Concat(IsSql("to_date"), string.Empty, Recurse<UEntity>(ref i, methodCall.Object, false, "(", $"'{methodCall.Arguments[0]}')"));
                }
                if (methodCall.Method.Name == nameof(string.ToLower)
                    || methodCall.Method.Name == nameof(string.ToLowerInvariant))
                {
                    return Concat(IsSql("LOWER"), string.Empty, Recurse<UEntity>(ref i, methodCall.Object, false, "(", ")"));
                }
                if (methodCall.Method.Name == nameof(string.ToUpper)
                   || methodCall.Method.Name == nameof(string.ToUpperInvariant))
                {
                    return Concat(IsSql("UPPER"), string.Empty, Recurse<UEntity>(ref i, methodCall.Object, false, "(", ")"));
                }
                try
                {
                    var value = Expression.Lambda(expression).Compile().DynamicInvoke();
                    if (value is string)
                    {
                        value = prefix + (string)value + postfix;
                    }
                    return IsParameter(i++, value);
                }
                catch
                {

                }
                finally
                {

                }
                throw new Exception("Unsupported method call: " + methodCall.Method.Name);
            }
            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        public string ValueToString(object value, bool isUnary, bool quote)
        {
            if (value is bool)
            {
                if (isUnary)
                {
                    return (bool)value ? "(1=1)" : "(1=0)";
                }
                return (bool)value ? "1" : "0";
            }
            return value.ToString();
        }

        protected static object GetValue(Expression member)
        {
            // source: http://stackoverflow.com/a/2616980/291955
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        protected static string NodeTypeToString(ExpressionType nodeType)
        {
            var @operator = string.Empty;
            switch (nodeType)
            {
                case ExpressionType.Add:
                    @operator = "+";
                    break;
                case ExpressionType.And:
                    @operator = "&";
                    break;
                case ExpressionType.AndAlso:
                    @operator = "AND";
                    break;
                case ExpressionType.Divide:
                    @operator = "/";
                    break;
                case ExpressionType.Equal:
                    @operator = "=";
                    break;
                case ExpressionType.ExclusiveOr:
                    @operator = "^";
                    break;
                case ExpressionType.GreaterThan:
                    @operator = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    @operator = ">=";
                    break;
                case ExpressionType.LessThan:
                    @operator = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    @operator = "<=";
                    break;
                case ExpressionType.Modulo:
                    @operator = "%";
                    break;
                case ExpressionType.Multiply:
                    @operator = "*";
                    break;
                case ExpressionType.Negate:
                    @operator = "-";
                    break;
                case ExpressionType.Not:
                    @operator = "NOT";
                    break;
                case ExpressionType.NotEqual:
                    @operator = "<>";
                    break;
                case ExpressionType.Or:
                    @operator = "|";
                    break;
                case ExpressionType.OrElse:
                    @operator = "OR";
                    break;
                case ExpressionType.Subtract:
                    @operator = "-";
                    break;
                case ExpressionType.Convert:
                    @operator = string.Empty;
                    break;
                default:
                    throw new Exception($"Unsupported node type: {nodeType}");
            }
            return $" {@operator} ";

        }
        protected virtual QueryResult IsSql(string sql)
        {
            return new QueryResult(sql, new Dictionary<string, object>());
        }

        protected virtual QueryResult IsParameter(int count, object value)
        {
            if (typeof(Guid) == value.GetType())
            {
                value = value.ToString();
            }
            return
            new QueryResult
            {
                Query = $"{parameterBinding}{count}",
                Parameters = new Dictionary<string, object> { { count.ToString(), value } }
            };
        }

        protected virtual QueryResult IsCollection(ref int countStart, IEnumerable values)
        {
            var parameters = new Dictionary<string, object>();
            var sql = new StringBuilder("(");
            foreach (var value in values)
            {
                if (typeof(Guid) == value.GetType())
                {
                    parameters.Add((countStart).ToString(), value.ToString());
                }
                else
                {
                    parameters.Add((countStart).ToString(), value);
                }
                sql.Append($"{parameterBinding}{countStart},");
                countStart++;
            }
            if (sql.Length == 1)
            {
                sql.Append("null,");
            }
            sql[sql.Length - 1] = ')';
            return
            new QueryResult(
                sql.ToString(),
                parameters
            );
        }

        protected virtual QueryResult
        Concat(string @operator, QueryResult operand)
        {
            return
            new QueryResult
            {
                Query = $"({@operator}{operand.Query})",
                Parameters = operand.Parameters,
            };

        }

        public QueryResult Concat(QueryResult left,
            string @operator, QueryResult right)
        {
            string eqOperator;

            if (left.Query == right.Query && @operator.Trim() == "=")
            {
                // there is a check for the same type on an inner join, should change one alias to a parent alias
                if (_parentAliases.ContainsKey(typeof(TEntity)))
                {
                    right.Query = right.Query.Replace($"{_alias}.", $"{_parentAliases[typeof(TEntity)]}.");
                }
            }
            if (right.Query.ToLower() == "null" && @operator.Trim() == "=")
            {
                eqOperator = "is";
            }
            else
            {
                eqOperator = @operator;
            }
            return
            new QueryResult
            {
                Query = $"({left.Query}{eqOperator}{right.Query})",
                Parameters = left.Parameters.Union(right.Parameters).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }
    }

}