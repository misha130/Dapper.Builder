﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Dapper.Builder.Builder.NamingStrategyService
{
    public class NamingStrategyService : INamingStrategyService
    {
        public string GetColumnName<TEntity>(string property, string alias = null)
        where TEntity : new()
        {
            if (property.Contains(".")) return property;
            return $"{alias ?? GetTableName(typeof(TEntity))}.[{property}]";
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
            return $"[{name}]";
        }
    }
}