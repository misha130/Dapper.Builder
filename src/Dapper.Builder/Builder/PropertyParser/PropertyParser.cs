using Dapper.Builder.Attributes;
using Dapper.Builder.Extensions;
using FastMember;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dapper.Builder.Services
{
    public class PropertyParser : IPropertyParser
    {
        public IEnumerable<string> Parse<TEntity>(Expression<Func<TEntity, object>> expression, bool validate = true) where TEntity : new()
        {
            return ParseProperty<TEntity>(expression.Body, validate);
        }
        public IEnumerable<string> Parse<TEntity,UEntity>(Expression<Func<TEntity, UEntity>> expression, bool validate = true) where TEntity : new()
        {
            return ParseProperty<TEntity>(expression.Body, validate);
        }

        private IEnumerable<string> ParseProperty<TEntity>(Expression expression, bool validate) where TEntity : new()
        {
            if (expression == null) yield break;
            if (expression is MemberExpression memExp)
            {
                if (Validate<TEntity>(memExp.Member.Name) && validate)
                {
                    yield return memExp.Member.Name;
                }
            }
            if ((expression is UnaryExpression unarExp))
            {
                if (unarExp.Operand is NewExpression newUExp)
                {
                    var accessor = TypeAccessor.Create(newUExp.Type);
                    foreach (var member in accessor.GetMembers())
                    {
                        if (Validate<TEntity>(member.Name) && validate)
                        {
                            yield return member.Name;
                        }
                    }
                }
                if (unarExp.Operand is MemberExpression omemExp)
                {
                    if (Validate<TEntity>(omemExp.Member.Name)&& validate)
                    {
                        yield return omemExp.Member.Name;
                    }
                }
                if ((unarExp.Operand) is ParameterExpression paramExp)
                {
                    foreach (var name in GetRelevantProperties<TEntity>(paramExp.Type))
                    {
                        yield return name;
                    }
                }
            }
            if (expression is NewExpression newExp)
            {
                var accessor = TypeAccessor.Create(newExp.Type);
                foreach (var member in accessor.GetMembers())
                {
                    if (Validate<TEntity>(member.Name) && validate)
                    {
                        yield return member.Name;
                    }
                }
            }
        }
        private bool Validate<TEntity>(string property)
        {
            var accessor = TypeAccessor.Create(typeof(TEntity));
            var members = accessor.GetMembers();
            return members.Any(m => m.Name.ToLower() == property.ToLower());
        }
        private IEnumerable<string> GetRelevantProperties<TEntity>(Type type)
        {
            var accessor = TypeAccessor.Create(typeof(TEntity));
            var members = accessor.GetMembers()
                .Where(member => member.GetAttribute(typeof(IgnoreInsert), false) == null
                                 && !typeof(TEntity).IsAssignableFrom(member.Type) && !(member.Type.IsEnumerable() && member.Type.IsGenericType));
            members = members.Where(member => !string.Equals(member.Name, "id", StringComparison.CurrentCultureIgnoreCase));
            return members
            .Select(member => member.Name);
        }
    }
}
