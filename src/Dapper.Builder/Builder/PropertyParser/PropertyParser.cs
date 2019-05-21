using FastMember;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Dapper.Builder.Attributes;
using Dapper.Builder.Extensions;

namespace Dapper.Builder.Services.DAL.Builder.PropertyParser
{
    public class PropertyParser : IPropertyParser
    {
        public IEnumerable<string> Parse<TEntity>(Expression<Func<TEntity, object>> expression, bool validate = true) where TEntity : new()
        {
            if (expression == null) yield break;
            if (expression.Body is MemberExpression memExp)
            {
                if (Validate<TEntity>(memExp.Member.Name))
                {
                    yield return memExp.Member.Name;
                }
            }
            if ((expression.Body is UnaryExpression unarExp))
            {
                if (unarExp.Operand is MemberExpression omemExp)
                {
                    if (Validate<TEntity>(omemExp.Member.Name))
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
        }

        private bool Validate<T>(string property)
        {
            var accessor = TypeAccessor.Create(typeof(T));
            var members = accessor.GetMembers();
            return members.Any(m => m.Name.ToLower() == property.ToLower());
        }

        private IEnumerable<string> GetRelevantProperties<T>(Type type)
        {
            var accessor = TypeAccessor.Create(typeof(T));
            var members = accessor.GetMembers()
                .Where(member => member.GetAttribute(typeof(IgnoreInsert), false) == null
                                 && !typeof(T).IsAssignableFrom(member.Type) && !(member.Type.IsEnumerable() && member.Type.IsGenericType));
            members = members.Where(member => !string.Equals(member.Name, "id", StringComparison.CurrentCultureIgnoreCase));
            return members
            .Select(member => member.Name);
        }
    }
}
