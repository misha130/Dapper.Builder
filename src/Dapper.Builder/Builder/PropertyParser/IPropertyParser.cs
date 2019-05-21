using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Builder.Services.DAL.Builder.PropertyParser
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

    }
}