using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper.Builder.Builder.Processes.Configuration;
using Dapper.Builder.Builder.SortHandler;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;

namespace Dapper.Builder.Services.DAL.Builder {
    /// <summary>
    /// A builder to create a relational database query & execute it.
    /// </summary>
    /// <typeparam name="T">The entity that will be used in this query</typeparam>
    public interface IQueryBuilder<T> where T : new () {

        /// <summary>
        /// Add a condition to the query 
        /// </summary>
        /// <param name="Predicate">An expression that describes the condition</param>
        IQueryBuilder<T> Where (Expression<Func<T, bool>> predicate);
        IQueryBuilder<T> Where<U> (Expression<Func<T, U, bool>> condition) where U : new ();

        IQueryBuilder<T> ParamCount (int count);

        IQueryBuilder<T> Distinct ();

        IQueryBuilder<T> Alias (string alias);

        IQueryBuilder<T> ParentAlias<U> (string alias) where U : new ();

        IQueryBuilder<T> Columns (params string[] columns);

        IQueryBuilder<T> Columns (Expression<Func<T, object>> columns);
        IQueryBuilder<T> Columns<U> (params string[] columns) where U : new ();
        IQueryBuilder<T> SubQuery<U> (Func<IQueryBuilder<U>, IQueryBuilder<U>> query, string alias) where U : new ();

        IQueryBuilder<T> ProcessConfig (Action<IProcessConfig> config);

        IQueryBuilder<T> Count ();
        IQueryBuilder<T> Json ();
        IQueryBuilder<T> GroupBy (Expression<Func<T, object>> columns);

        IQueryBuilder<T> SortAscending (Expression<Func<T, object>> sortProperty);
        IQueryBuilder<T> SortDescending (Expression<Func<T, object>> sortProperty);

        IQueryBuilder<T> SortAscending<U> (Expression<Func<U, object>> sortProperty) where U : new ();
        IQueryBuilder<T> SortDescending<U> (Expression<Func<U, object>> sortProperty) where U : new ();
        IQueryBuilder<T> Sort (params SortColumn[] columns);

        IQueryBuilder<T> Top (int? top);

        IQueryBuilder<T> Skip (int? skip);

        IQueryBuilder<T> Join<U> (Expression<Func<T, U, bool>> foreignKeyProperty, JoinType type = JoinType.Inner) where U : new ();

        IQueryBuilder<T> Join<U, W> (Expression<Func<U, W, bool>> foreignKeyProperty, JoinType type = JoinType.Inner) where U : new () where W : new ();

        IQueryBuilder<T> Map (Func<IDbConnection, string, object, Task<IEnumerable<T>>> action);

        QueryResult GetQueryString ();
        QueryResult GetInsertString (T entity);
        QueryResult GetInsertString (IEnumerable<T> entity);
        QueryResult GetUpdateString (T entity);
        QueryResult GetDeleteString ();

        Task<T> ExecuteSingleAsync ();

        Task<IEnumerable<T>> ExecuteAsync ();

        Task<U> ExecuteSingleAsync<U> ();

        Task<int> ExecuteUpdateAsync (T entity);

        Task<long> ExecuteInsertAsync (T entity);

        Task<int> ExecuteDeleteAsync (T entity);

        int GetParamCount ();
        IQueryBuilder<T> GroupBy (params string[] columns);

    }
}