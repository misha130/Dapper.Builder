using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dapper.Builder.Services
{
    /// <summary>
    /// Services that parses objects to column names
    /// </summary>
    public interface IPropertyParser
    {
        /// <summary>
        /// Parses an expression that is an object into column names
        /// </summary>
        IEnumerable<string> Parse<TEntity>(Expression<Func<TEntity, object>> expression, bool validate = true) where TEntity : new();

        IEnumerable<string> Parse<TEntity, UEntity>(Expression<Func<TEntity, UEntity>> expression,
            bool validate = true) where TEntity : new();

    }
}