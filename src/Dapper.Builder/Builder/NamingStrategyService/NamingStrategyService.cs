using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Dapper.Builder.Builder.NamingStrategyService
{
    public class NamingStrategyService : INamingStrategyService
    {
        public string GetColumnName<T>(string property, string alias = null)
        {
            if (property.Contains(".")) return property;
            return $"{alias ?? GetTableName(typeof(T))}.[{property}]";
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
            return $"[{name}]";
        }
    }
}
