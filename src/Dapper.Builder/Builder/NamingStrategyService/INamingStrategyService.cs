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
        /// <param name="name">The name of</param>
        string GetTableName(string name);

        /// <summary>
        /// Gets the name of the column and the table
        /// </summary>
        /// <param name="name"></param>
        /// <param name="alias"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns>The column and table name adjusted to the data source</returns>
        string GetTableAndColumnName<TEntity>(string name, string alias = null) where TEntity : new();


        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="name">The column name.</param>
        /// <param name="alias">The alias.</param>
        /// <returns>The column adjusted to the data source</returns>
        string GetColumnName<TEntity>(string name, string alias = null) where TEntity : new();
    }
}