using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Dapper.Builder.Builder;
using Dapper.Builder.Builder.NamingStrategyService;

namespace Dapper.Builder.Services.DAL.Builder.FilterParser {
    public class FilterParser<TEntity> : IFilterParser<TEntity> where TEntity : new () {

        protected INamingStrategyService _namingService;
        public FilterParser (INamingStrategyService namingService) {
            _namingService = namingService;
        }
        protected virtual string parameterBinding => "@";
        protected string _alias;
        protected Dictionary<Type, string> _parentAliases = new Dictionary<Type, string> ();

        public void SetAlias (string alias) {
            _alias = alias;
        }

        public void SetParentAlias<UEntity> (string alias) where UEntity : new () {
            _parentAliases.Add (typeof (TEntity), alias);
        }
        public QueryResult Parse (Expression<Func<TEntity, bool>> expression, ref int i) {
            return Recurse<TEntity> (ref i, expression.Body, isUnary : true);
        }

        public QueryResult Parse<UEntity> (Expression<Func<TEntity, UEntity, bool>> expression, ref int i)
        where UEntity : new () {
            return Recurse<UEntity> (ref i, expression.Body, isUnary : true);
        }

        public QueryResult Parse<UEntity, WEntity> (Expression<Func<UEntity, WEntity, bool>> expression, ref int i)
        where UEntity : new ()
        where WEntity : new () {
            return Recurse<UEntity> (ref i, expression.Body, isUnary : true);
        }
        protected virtual QueryResult Recurse<UEntity> (ref int i, Expression expression, bool isUnary = false, string prefix = null, string postfix = null)
        where UEntity : new () {
            if (expression is UnaryExpression) {
                var unary = (UnaryExpression) expression;
                return Concat (NodeTypeToString (unary.NodeType), Recurse<UEntity> (ref i, unary.Operand, true));
            }
            if (expression is BinaryExpression) {
                var body = (BinaryExpression) expression;
                return Concat (Recurse<UEntity> (ref i, body.Left), NodeTypeToString (body.NodeType), Recurse<UEntity> (ref i, body.Right));
            }
            if (expression is ConstantExpression) {
                var constant = (ConstantExpression) expression;
                var value = constant.Value;
                if (value is int) {
                    return IsSql (value.ToString ());
                }
                if (value is string) {
                    value = prefix + (string) value + postfix;
                }
                if (value is bool && isUnary) {
                    return Concat (IsParameter (i++, value), "=", IsSql ("1"));
                }
                return IsParameter (i++, value);
            }
            if (expression is MemberExpression) {

                var member = (MemberExpression) expression;
                try {
                    var value = Expression.Lambda (expression).Compile ().DynamicInvoke ();
                    return IsParameter (i++, value);
                } catch {

                }

                if (member.Member is PropertyInfo) {
                    var property = (PropertyInfo) member.Member;
                    var colName = property.Name;
                    if (isUnary && member.Type == typeof (bool)) {
                        return Concat (Recurse<UEntity> (ref i, expression), "=", IsParameter (i++, true));
                    }
                    var type = member.Expression.Type;
                    if (type.IsInterface) {
                        type = typeof (UEntity);
                    }
                    string tableName = string.Empty;
                    if (type == typeof (TEntity) && !string.IsNullOrEmpty (_alias)) {
                        tableName = _alias;
                    } else if (_parentAliases.ContainsKey (typeof (UEntity))) {
                        tableName = _parentAliases[typeof (UEntity)];
                    } else {
                         tableName = ($"[{_namingService.GetTableName(type)}]");
                    }

                    return IsSql ($"{tableName}.[{colName}]");
                }
                if (member.Member is FieldInfo) {
                    var value = GetValue (member);
                    if (value is string) {
                        value = prefix + (string) value + postfix;
                    }
                    return IsParameter (i++, value);
                }
                throw new Exception ($"Expression does not refer to a property or field: {expression}");
            }
            if (expression is MethodCallExpression) {
                var methodCall = (MethodCallExpression) expression;
                // LIKE queries:
                if (methodCall.Method == typeof (string).GetMethod ("Contains", new [] { typeof (string) })) {
                    return Concat (Recurse<UEntity> (ref i, methodCall.Object), "LIKE", Recurse<UEntity> (ref i, methodCall.Arguments[0], prefix: "%", postfix: "%"));
                }
                if (methodCall.Method == typeof (string).GetMethod ("StartsWith", new [] { typeof (string) })) {
                    return Concat (Recurse<UEntity> (ref i, methodCall.Object), "LIKE", Recurse<UEntity> (ref i, methodCall.Arguments[0], postfix: "%"));
                }
                if (methodCall.Method == typeof (string).GetMethod ("EndsWith", new [] { typeof (string) })) {
                    return Concat (Recurse<UEntity> (ref i, methodCall.Object), "LIKE", Recurse<UEntity> (ref i, methodCall.Arguments[0], prefix: "%"));
                }
                // IN queries:
                if (methodCall.Method.Name == "Contains") {
                    Expression collection;
                    Expression property;
                    if (methodCall.Method.IsDefined (typeof (ExtensionAttribute)) && methodCall.Arguments.Count == 2) {
                        collection = methodCall.Arguments[0];
                        property = methodCall.Arguments[1];
                    } else if (!methodCall.Method.IsDefined (typeof (ExtensionAttribute)) && methodCall.Arguments.Count == 1) {
                        collection = methodCall.Object;
                        property = methodCall.Arguments[0];
                    } else {
                        throw new Exception ("Unsupported method call: " + methodCall.Method.Name);
                    }
                    var values = (IEnumerable) GetValue (collection);
                    return Concat (Recurse<UEntity> (ref i, property), "IN", IsCollection (ref i, values));
                }
                try {
                    var value = Expression.Lambda (expression).Compile ().DynamicInvoke ();
                    return IsParameter (i++, value);
                } finally {

                }
                throw new Exception ("Unsupported method call: " + methodCall.Method.Name);
            }
            throw new Exception ("Unsupported expression: " + expression.GetType ().Name);
        }

        public string ValueToString (object value, bool isUnary, bool quote) {
            if (value is bool) {
                if (isUnary) {
                    return (bool) value ? "(1=1)" : "(1=0)";
                }
                return (bool) value ? "1" : "0";
            }
            return value.ToString ();
        }

        protected static object GetValue (Expression member) {
            // source: http://stackoverflow.com/a/2616980/291955
            var objectMember = Expression.Convert (member, typeof (object));
            var getterLambda = Expression.Lambda<Func<object>> (objectMember);
            var getter = getterLambda.Compile ();
            return getter ();
        }

        protected static string NodeTypeToString (ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Not:
                    return "NOT";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Convert:
                    return string.Empty;
            }
            throw new Exception ($"Unsupported node type: {nodeType}");
        }
        protected virtual QueryResult IsSql (string sql) {
            return new QueryResult (sql, new Dictionary<string, object> ());
        }

        protected virtual QueryResult IsParameter (int count, object value) {
            return
            new QueryResult {
                Query = $"{parameterBinding}{count}",
                    Parameters = new Dictionary<string, object> { { count.ToString (), value } }
            };
        }

        protected virtual QueryResult IsCollection (ref int countStart, IEnumerable values) {
            var parameters = new Dictionary<string, object> ();
            var sql = new StringBuilder ("(");
            foreach (var value in values) {
                parameters.Add ((countStart).ToString (), value);
                sql.Append ($"{parameterBinding}{countStart},");
                countStart++;
            }
            if (sql.Length == 1) {
                sql.Append ("null,");
            }
            sql[sql.Length - 1] = ')';
            return
            new QueryResult (
                sql.ToString (),
                parameters
            );
        }

        protected virtual QueryResult
        Concat (string @operator, QueryResult operand) {
            return
            new QueryResult {
                Query = $"({@operator} {operand.Query})",
                    Parameters = operand.Parameters,
            };

        }

        public QueryResult Concat (QueryResult left,
            string @operator, QueryResult right) {
            string eqOperator;

            if (left.Query == right.Query && @operator == "=") {
                // there is a check for the same type on an inner join, should change one alias to a parent alias
                if (_parentAliases.ContainsKey (typeof (TEntity))) {
                    right.Query = right.Query.Replace ($"{_alias}.", $"{_parentAliases[typeof(TEntity)]}.");
                }
            }
            if (right.Query.ToLower () == "null" && @operator == "=") {
                eqOperator = "is";
            } else {
                eqOperator = @operator;
            }
            return
            new QueryResult {
                Query = $"({left.Query} {eqOperator} {right.Query})",
                    Parameters = left.Parameters.Union (right.Parameters).ToDictionary (kvp => kvp.Key, kvp => kvp.Value)
            };
        }
    }

}