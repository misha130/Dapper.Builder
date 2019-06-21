using System;

namespace Dapper.Builder.Services
{
    /// <summary>
    /// The strategy how to name columns and tables
    /// </summary>
    public interface INamingStrategyService
    {

        /// <summary>
        /// Gets the table name by Type
        /// </summary>
        /// <param name="type">The type to get the name from</param>
        string GetTableName(Type type);

        /// <summary>
        /// Gets the table name by Generic Type
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        string GetTableName<TEntity>() where TEntity : new();

        /// <summary>
        /// Gets the table name on how it would be called in the database
        /// </summary>
        /// <param name="type">The type to get the name from</param>
        string GetTableName(string name);

        /// <summary>
        /// Gets the name of the column
        /// </summary>
        /// <param name="name"></param>
        /// <param name="alias"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        string GetTableAndColumnName<TEntity>(string name, string alias = null) where TEntity : new();

        string GetColumnName<TEntity>(string name, string alias = null) where TEntity : new();
    }
}