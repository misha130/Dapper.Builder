using FastMember;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Dapper.Builder.Attributes;
using Dapper.Builder.Extensions;

namespace Dapper.Builder.Services.DAL.Builder.PropertyParser
{
    public class PropertyParser : IPropertyParser
    {
        public IEnumerable<string> Parse<T>(Expression<Func<T, object>> expression, bool validate = true) where T :  new()
        {
            if (expression == null) yield break;
            if (expression.Body is MemberExpression memExp)
            {
                if (Validate<T>(memExp.Member.Name))
                {
                    yield return memExp.Member.Name;
                }
            }
            if ((expression.Body is UnaryExpression unarExp))
            {
                if (unarExp.Operand is MemberExpression omemExp)
                {
                    if (Validate<T>(omemExp.Member.Name))
                    {
                        yield return omemExp.Member.Name;
                    }
                }
                if ((unarExp.Operand) is ParameterExpression paramExp)
                {
                    foreach (var name in GetRelevantProperties<T>(paramExp.Type))
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

        private string ConstructName(MemberInfo member)
        {
            return string.Empty;
        }
    }
}
