using Dapper.Builder.Processes;
using Dapper.Builder.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.Builder
{
    /// <summary>
    /// A builder to create a relational database query & execute it.
    /// </summary>
    /// <TEntityEntityypeparam name="TEntity">The entity that will be used in this query</typeparam>
    public interface IQueryBuilder<TEntity> where TEntity : new()
    {

        /// <summary>
        /// Adds a where condition to the query on the main entity
        /// </summary>
        /// <param name="predicate">The expression that represents the condition</param>
        IQueryBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Adds a where condition to the query on not the main entity
        /// </summary>
        /// <typeparam name="UEntity">Some entity that was joined</typeparam>
        /// <param name="predicate">The expression that represents the condition</param>
        IQueryBuilder<TEntity> Where<UEntity>(Expression<Func<TEntity, UEntity, bool>> predicate) where UEntity : new();
        /// <summary>
        /// Adds a where condition to the query to two unrelated entities
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IQueryBuilder<TEntity> Where<UEntity, WEntity>(Expression<Func<UEntity, WEntity, TEntity, bool>> predicate)
        where UEntity : new()
        where WEntity : new();
        /// <summary>
        /// Tells to add a distinct to the query
        /// </summary>
        /// <returns></returns>
        IQueryBuilder<TEntity> Distinct();

        /// <summary>
        /// Adds an alias on the main query entity
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        IQueryBuilder<TEntity> Alias(string alias);

        /// <summary>
        /// Adds an alias on the parent/joined entity
        /// </summary>

        IQueryBuilder<TEntity> ParentAlias<UEntity>(string alias) where UEntity : new();

        /// <summary>
        /// The columns that should be in the query
        /// </summary>
        IQueryBuilder<TEntity> Columns(params string[] columns);

        /// <summary>
        /// An expression that represents the columns, usually an anonymous object
        /// </summary>
        /// <param name="columns"></param>
        IQueryBuilder<TEntity> Columns(Expression<Func<TEntity, object>> columns);


        /// <summary>
        /// An expression that represents the columns to exclude, usually an anonymous object
        /// </summary>
        /// <param name="columns"></param>
        IQueryBuilder<TEntity> ExcludeColumns<UEntity>(Expression<Func<UEntity, object>> columns) where UEntity : new();


        /// <summary>
        /// An expression that represents the columns to exclude, usually an anonymous object
        /// </summary>
        /// <param name="columns"></param>
        IQueryBuilder<TEntity> ExcludeColumns(Expression<Func<TEntity, object>> columns);

        /// <summary>
        /// The columns that should be in the query, not from the main entity
        /// </summary>
        /// <typeparam name="UEntity">The entity to take the name from</typeparam>
        IQueryBuilder<TEntity> Columns<UEntity>(params string[] columns) where UEntity : new();

        /// <summary>
        /// Creates a sub query in the query
        /// </summary>
        /// <typeparam name="UEntity">The main sub query entity</typeparam>
        /// <param name="query">A query builder that represents the sub query</param>
        /// <param name="alias">The name of the column for the sub query</param>
        /// <returns></returns>
        IQueryBuilder<TEntity> SubQuery<UEntity>(Func<IQueryBuilder<UEntity>, IQueryBuilder<UEntity>> query, string alias)
        where UEntity : new();

        /// <summary>
        /// Processes configuration
        /// </summary>
        /// <param name="config">The services for configurating processes</param>
        IQueryBuilder<TEntity> ProcessConfig(Action<IProcessConfig> config);

        /// <summary>
        /// If this is stated the query will only return the number of rows.
        /// It does not add another count column to the previous columns
        /// </summary>
        IQueryBuilder<TEntity> Count();

        /// <summary>
        /// Whether the query should go through a json serialization in the database and then deserialization
        /// Used for getting multiple rows inside of subqueries
        /// </summary>
        IQueryBuilder<TEntity> Json();

        /// <summary>
        /// Creates a group part of the query
        /// </summary>
        /// <param name="columns">The object that will be translated into a columns</param>
        IQueryBuilder<TEntity> GroupBy(Expression<Func<TEntity, object>> columns);

        /// <summary>
        /// Creates a group part of the query
        /// </summary>
        /// <param name="columns">Names of columns that will be used</param>
        /// <returns></returns>
        IQueryBuilder<TEntity> GroupBy(params string[] columns);

        /// <summary>
        /// Adds an order by asc to the query 
        /// </summary>
        /// <param name="sortProperty">Expression that determines the column for sort</param>
        IQueryBuilder<TEntity> SortAscending(Expression<Func<TEntity, object>> sortProperty);

        /// <summary>
        /// Adds an order by desc to the query 
        /// </summary>
        /// <param name="sortProperty">Expression that determines the column for sort</param>
        IQueryBuilder<TEntity> SortDescending(Expression<Func<TEntity, object>> sortProperty);

        /// <summary>
        /// Adds an order by asc to the query for not the main entity
        /// </summary>
        /// <param name="sortProperty">Expression that determines the column for sort</param>
        IQueryBuilder<TEntity> SortAscending<UEntity>(Expression<Func<UEntity, object>> sortProperty) where UEntity : new();

        /// <summary>
        /// Adds an order by desc to the query for not the main entity
        /// </summary>
        /// <param name="sortProperty">Expression that determines the column for sort</param>
        IQueryBuilder<TEntity> SortDescending<UEntity>(Expression<Func<UEntity, object>> sortProperty) where UEntity : new();

        /// <summary>
        /// Adds order by parts to the query 
        /// </summary>
        /// <param name="columns">The sort data that will be used</param>
        /// <returns></returns>
        IQueryBuilder<TEntity> Sort(params SortColumn[] columns);

        /// <summary>
        /// Determines how many rows to get
        /// </summary>
        /// <param name="top">Number of rows to get</param>
        /// <returns></returns>
        IQueryBuilder<TEntity> Top(int? top);

        /// <summary>
        /// Determines how many rows to skip
        /// </summary>
        /// <param name="skip">Number of rows to skip</param>
        /// <returns></returns>

        IQueryBuilder<TEntity> Skip(int? skip);

        /// <summary>
        /// Creates a join query part
        /// </summary>
        /// <typeparam name="UEntity">The entity to join with</typeparam>
        /// <param name="predicate">The condition for the join</param>
        /// <param name="type">The type of join</param>
        /// <returns></returns>
        IQueryBuilder<TEntity> Join<UEntity>(Expression<Func<TEntity, UEntity, bool>> predicate, JoinType type = JoinType.Inner)
        where UEntity : new();

        /// <summary>
        /// Creates a join query part
        /// </summary>
        /// <typeparam name="UEntity">The entity to join with</typeparam>
        /// <typeparam name="WEntity">The entity to join with</typeparam>
        /// <param name="predicate">The condition for the join</param>
        /// <param name="type">The type of join</param>
        /// <returns></returns>
        IQueryBuilder<TEntity> Join<UEntity, WEntity>(Expression<Func<UEntity, WEntity, bool>> predicate, JoinType type = JoinType.Inner)
        where UEntity : new() where WEntity : new();

        /// <summary>
        /// Utilizes the mapping ability of dapper after query has been executed.
        /// Enables the developer to execute the query themselves
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IQueryBuilder<TEntity> Map(Func<IDbConnection, string, object, Task<IEnumerable<TEntity>>> action);

        /// <summary>
        /// Returns the select query
        /// </summary>
        QueryResult GetQueryString();

        /// <summary>
        /// Returns the insert query
        /// </summary>
        QueryResult GetInsertString(TEntity entity);

        /// <summary>
        /// Returns the insert query
        /// </summary>
        QueryResult GetInsertString(IEnumerable<TEntity> entity);

        /// <summary>
        /// Returns the update query
        /// </summary>
        QueryResult GetUpdateString(TEntity entity);

        /// <summary>
        /// Returns the delete query
        /// </summary>
        QueryResult GetDeleteString();

        /// <summary>
        /// Executes a query to the database that returns only one row
        /// </summary>
        Task<TEntity> ExecuteSingleAsync();

        /// <summary>
        /// Executes a query to the database
        /// </summary>
        Task<IEnumerable<TEntity>> ExecuteAsync();

        /// <summary>
        /// Executes a query to the database that returns only one row
        /// Maybe return numbers
        /// </summary>
        Task<UEntity> ExecuteSingleAsync<UEntity>();


        /// <summary>
        /// Executes updated query
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>Number of rows affected</returns>
        Task<int> ExecuteUpdateAsync(TEntity entity);

        /// <summary>
        /// Executes delete query
        /// </summary>
        /// <returns>Number of rows affected</returns>
        Task<int> ExecuteDeleteAsync();

        /// <summary>
        /// Executes insert query
        /// </summary>
        /// <returns>Returns id of inserted entity</returns>
        Task<long> ExecuteInsertAsync(TEntity entity);

        /// <summary>
        /// Clones an instancee of the current builder with the options
        /// </summary>
        /// <returns></returns>
        IQueryBuilder<TEntity> CloneInstance();
    }

}