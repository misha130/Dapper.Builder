using System;
using System.Linq.Expressions;

namespace Dapper.Builder.Services
{
    /// <summary>
    /// Class Responsible for parsing condition
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    public interface IFilterParser<TEntity> where TEntity : new()
    {

        /// <summary>
        /// Parses an expression to an database query condition
        /// </summary>
        /// <param name="predicate">The expression</param>
        /// <param name="count">Current count of the parameters</param>

        QueryResult Parse(Expression<Func<TEntity, bool>> predicate, ref int count);
        /// <summary>
        /// Parses an expression to a database query condition
        /// In the case that one of the entities is not part of the original source (from)
        /// </summary>
        /// <param name="predicate">The expression</param>
        /// <param name="count">Current count of the parameters</param>
        /// <typeparam name="UEntity">The other entity</typeparam>

        QueryResult Parse<UEntity>(Expression<Func<TEntity, UEntity, bool>> predicate, ref int count)
            where UEntity : new();

        /// <summary>
        /// Parses an expressions to a database query condition
        /// </summary>
        /// <typeparam name="UEntity"></typeparam>
        /// <typeparam name="WEntity"></typeparam>
        /// <param name="expression"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        QueryResult Parse<UEntity, WEntity>(Expression<Func<UEntity, WEntity, TEntity, bool>> expression, ref int count)
            where UEntity : new()
            where WEntity : new();

        /// <summary>
        /// Parses an expression to an database query condition.
        /// In the case when the two entities are not part of the original source (from)
        /// </summary>
        /// <param name="predicate">The expression</param>
        /// <param name="count">Current count of the parameters</param>
        /// <typeparam name="UEntity">First entity</typeparam>
        /// <typeparam name="WEntity">Second Entity</typeparam>
        QueryResult Parse<UEntity, WEntity>(Expression<Func<UEntity, WEntity, bool>> predicate, ref int count) where UEntity : new() where WEntity : new();

        /// <summary>
        /// Sets the alias for TEntity to be used in the condition
        /// </summary>
        /// <param name="alias"></param>
        void SetAlias(string alias);

        /// <summary>
        /// Sets the alias for the other entities, even if they are the same type as TEntity
        /// </summary>
        /// <param name="alias"></param>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        void SetParentAlias<UEntity>(string alias) where UEntity : new();
    }
}