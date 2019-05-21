using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dapper.Builder.Builder;
using Dapper.Builder.Builder.NamingStrategyService;

namespace Dapper.Builder.Services.DAL.Builder.FilterParser {
    public class PostgreFilterParser<TEntity> : FilterParser<TEntity> where TEntity : new () {

        public PostgreFilterParser (INamingStrategyService namingService) : base (namingService) {

        }
        protected override string parameterBinding => ":";
        protected override QueryResult Recurse<UEntity> (ref int i, Expression expression, bool isUnary = false, string prefix = null, string postfix = null) {
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
                if (value == null) {
                    return IsSql ("null");
                }
                return IsParameter (i++, value);
            }
            if (expression is MemberExpression) {
                var member = (MemberExpression) expression;

                if (member.Member is FieldInfo) {
                    var value = GetValue (member);
                    if (value is string) {
                        value = prefix + (string) value + postfix;
                    }
                    return IsParameter (i++, value);
                }
                try {
                    var value = Expression.Lambda (expression).Compile ().DynamicInvoke ();
                    return IsParameter (i++, value);
                } catch {

                }
                if (member.Member is PropertyInfo) {
                    var property = (PropertyInfo) member.Member;
                    var colName = property.Name;
                    if (isUnary && (member.Type == typeof (bool) || member.Type == typeof (bool?))) {
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
                        tableName = ($"\"{_namingService.GetTableName(type)}\"");
                    }
                    return IsSql (_namingService.GetColumnName<TEntity> ($"{tableName}.{colName}"));
                }

                throw new Exception ($"Expression does not refer to a property or field: { expression }");
            }

            if (expression is MethodCallExpression) {
                var methodCall = (MethodCallExpression) expression;
                // LIKE queries:
                if (methodCall.Method == typeof (string).GetMethod ("Contains ", new [] { typeof (string) })) {
                    return Concat (Recurse<UEntity> (ref i, methodCall.Object), "LIKE ", Recurse<UEntity> (ref i, methodCall.Arguments[0], prefix: " % ", postfix: " % "));
                }
                if (methodCall.Method == typeof (string).GetMethod ("StartsWith ", new [] { typeof (string) })) {
                    return Concat (Recurse<UEntity> (ref i, methodCall.Object), "LIKE ", Recurse<UEntity> (ref i, methodCall.Arguments[0], postfix: " % "));
                }
                if (methodCall.Method == typeof (string).GetMethod ("EndsWith ", new [] { typeof (string) })) {
                    return Concat (Recurse<UEntity> (ref i, methodCall.Object), "LIKE ", Recurse<UEntity> (ref i, methodCall.Arguments[0], prefix: " % "));
                }
                // IN queries:
                if (methodCall.Method.Name == "Contains ") {
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
                    return Concat (Recurse<UEntity> (ref i, property), "IN ", IsCollection (ref i, values));
                }

                //Date queries
                if (methodCall.Method == typeof (DateTime).GetMethod ("ToShortDateString ")) {
                    var dateInfo = Recurse<UEntity> (ref i, methodCall.Object);
                    return new QueryResult ($"CAST ({ dateInfo.Query } as date)", dateInfo.Parameters);
                }
                if (methodCall.Method == typeof (DateTime).GetMethod ("ToString ", new [] { typeof (string) })) {
                    var concatDate = Concat (Recurse<UEntity> (ref i, methodCall.Object), ", ", Recurse<UEntity> (ref i, methodCall.Arguments[0]));
                    return new QueryResult ($"TO_DATE ({ concatDate.Query })", concatDate.Parameters);
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

    }

}