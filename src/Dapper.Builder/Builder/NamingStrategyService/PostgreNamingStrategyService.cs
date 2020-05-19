using Dapper.Builder.Attributes;
using Dapper.Builder.Extensions;
using System;
using System.Linq;

namespace Dapper.Builder.Services
{
    public class PostgreNamingStrategyService : INamingStrategyService
    {
        public string GetTableAndColumnName<TEntity>(string property, string alias = null)
        where TEntity : new()
        {
            if (property.Contains('(') || property.Contains(')')) return property;
            if (property.Contains("."))
            {
                var splitted = property.Split('.');
                string manipulatedProperty = string.Empty;
                foreach (var prop in splitted)
                {
                    if (splitted.Last() == prop)
                    {
                        manipulatedProperty += prop;
                    }
                    else
                    {
                        manipulatedProperty += prop;
                    }

                }

                return manipulatedProperty;
            };
            return $"{alias ?? GetTableName<TEntity>()}.{property}";
        }

        public string GetColumnName<TEntity>(string property, string alias = null)
      where TEntity : new()
        {
            if (property.Contains(".")) return property;
            return alias ?? property;
        }


        public string GetTableName<TEntity>() where TEntity : new()
        {
            return GetTableName(typeof(TEntity));
        }

        public virtual string GetTableName(Type type)
        {
            TableAttribute tableAttribute = null;
            if (type.CustomAttributes.Any())
            {
                tableAttribute =
                    (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault();
            }
            return GetTableName(
                (tableAttribute != null) ?
                tableAttribute.Name :
                type.Name);
        }

        public virtual string GetTableName(string name)
        {
            return $"\"{name}\"";
        }
    }
}