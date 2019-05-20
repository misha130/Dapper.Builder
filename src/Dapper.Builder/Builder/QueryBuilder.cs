// ***********************************************************************
// Assembly         : Dapper.Builder
// Author           : micha
// Created          : 01-28-2019
//
// Last Modified By : micha
// Last Modified On : 02-26-2019
// ***********************************************************************
// <copyright file="QueryBuilder.cs" company="Dapper.Builder">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Reflection;
using Dapper.Builder.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Dapper.Builder.Builder.Processes.Configuration;
using Dapper.Builder.Builder.NamingStrategyService;
using Dapper.Builder.Builder.SortHandler;
using Dapper.Builder.Builder;
using Dapper;
namespace Dapper.Builder.Services.DAL.Builder
{
    /// <summary>
    /// Class QueryBuilder.
    /// </summary>
    public class QueryBuilder<T> : IQueryBuilder<T> where T : new()
    {
        protected readonly IQueryBuilderDependencies<T> dependencies;
        public QueryBuilder(IQueryBuilderDependencies<T> dependencies)
        {

        }
        protected QueryBuilderOptions<T> Options = new QueryBuilderOptions<T>();

        protected virtual string parameterBinding => "@";


        public IQueryBuilder<T> Where(string filter, Dictionary<string, object> parameter)
        {
            Options.WhereStrings.Add(filter);
            Options.Parameters.Merge(parameter);
            return this;
        }

        public IQueryBuilder<T> Where(Expression<Func<T, bool>> condition)
        {
            var whereResult = dependencies.FilterParser.Parse(condition, ref Options.ParamCount);
            Options.WhereStrings.Add(whereResult.Query);
            Options.Parameters.Merge(whereResult.Parameters);
            return this;
        }

        public IQueryBuilder<T> ParamCount(int count)
        {
            Options.ParamCount += count;
            return this;
        }
        public IQueryBuilder<T> Where(Expression<Func<T, object>> condition)
        {
            var whereResult = dependencies.FilterParser.Parse(condition, ref Options.ParamCount);
            Options.WhereStrings.Add(whereResult.Query);
            Options.Parameters.Merge(whereResult.Parameters);
            return this;
        }

        public IQueryBuilder<T> Where<U>(Expression<Func<T, U, bool>> condition) where U : new()
        {
            var whereResult = dependencies.FilterParser.Parse(condition, ref Options.ParamCount);
            Options.WhereStrings.Add(whereResult.Query);
            Options.Parameters.Merge(whereResult.Parameters);
            return this;
        }

        public IQueryBuilder<T> Json()
        {
            Options.Json = true;
            return this;
        }

        public IQueryBuilder<T> Distinct()
        {
            Options.Distinct = true;
            return this;
        }

        public IQueryBuilder<T> Columns(params string[] columns)
        {
            Options.SelectColumns.AddRange(columns);
            return this;
        }
        public virtual IQueryBuilder<T> Columns<U>(params string[] columns) where U : new()
        {
            Options.SelectColumns.AddRange(columns.Select(col => $"{typeof(U)}.{col}"));
            return this;
        }

        public IQueryBuilder<T> Columns(Expression<Func<T, object>> properties)
        {
            Options.SelectColumns.AddRange(dependencies.PropertyParser.Value.Parse(properties));
            return this;
        }

        public virtual IQueryBuilder<T> SubQuery<U>(Func<IQueryBuilder<U>, IQueryBuilder<U>> query, string alias) where U : new()
        {
            var queryBuilder = dependencies.ServiceProvider.Value
                .GetRequiredService<IQueryBuilder<U>>();
            var result = query(queryBuilder.ParamCount(Options.ParamCount)).GetQueryString();
            ParamCount(result.Count);
            Options.Subqueries.Add($"({result.Query}) as {alias}");
            if (result.Parameters != null)
            {
                Options.Parameters.Merge(result.Parameters);
            }
            return this;
        }

        public IQueryBuilder<T> Count()
        {
            Options.Count = true;
            return this;
        }

        public IQueryBuilder<T> GroupBy(Expression<Func<T, object>> columns)
        {
            Options.GroupingColumns.AddRange(dependencies.PropertyParser.Value.Parse(columns, false));
            return this;
        }
        public IQueryBuilder<T> GroupBy(params string[] columns)
        {
            Options.GroupingColumns.AddRange(columns);
            return this;
        }


        public IQueryBuilder<T> SortAscending(Expression<Func<T, object>> columns)
        {
            Options.SortColumns.AddRange(dependencies.PropertyParser.Value.Parse(columns, true)
                .Select(sort => dependencies.SortHandler.Value
                    .Produce<T>(new SortColumn { Field = sort, Dir = SortType.Asc }, Options.Alias)
                    )
                );
            return this;
        }
        public IQueryBuilder<T> SortDescending(Expression<Func<T, object>> columns)
        {
            Options.SortColumns.AddRange(dependencies.PropertyParser.Value.Parse(columns, true)
                .Select(sort => dependencies.SortHandler.Value
                    .Produce<T>(new SortColumn { Field = sort, Dir = SortType.Desc }, Options.Alias)
                    )
                );
            return this;
        }

        public IQueryBuilder<T> SortAscending<U>(Expression<Func<U, object>> sortProperty) where U : new()
        {
            Options.SortColumns.AddRange(dependencies.PropertyParser.Value.Parse(sortProperty, true)
               .Select(sort => dependencies.SortHandler.Value
                   .Produce<U>(new SortColumn { Field = sort, Dir = SortType.Desc })
                   )
               );
            return this;
        }

        public IQueryBuilder<T> SortDescending<U>(Expression<Func<U, object>> sortProperty) where U : new()
        {
            Options.SortColumns.AddRange(dependencies.PropertyParser.Value.Parse(sortProperty, true)
                  .Select(sort => dependencies.SortHandler.Value
                      .Produce<U>(new SortColumn { Field = sort, Dir = SortType.Desc })
                      )
                  );
            return this;
        }

        public IQueryBuilder<T> Sort(params SortColumn[] sortColumns)
        {
            if (sortColumns != null)
            {
                Options.SortColumns.AddRange(sortColumns.Where(sc => sc != null).Select(sort => dependencies.SortHandler.Value.Produce<T>(sort)));
            }
            return this;
        }

        public IQueryBuilder<T> Top(int? top)
        {
            Options.Top = top;
            return this;
        }

        public IQueryBuilder<T> Skip(int? skip)
        {
            Options.Skip = skip;
            return this;
        }

        public IQueryBuilder<T> Join<U>(Expression<Func<T, U, bool>> foreignKeyProperty, JoinType type = JoinType.Inner) where U : new()
        {

            var joinResult = dependencies.FilterParser.Parse(foreignKeyProperty, ref Options.ParamCount);
            Options.JoinQueries.Add(
                dependencies.JoinHandler.Value.Produce(
                    GetTableName<U>(),
                   joinResult.Query,
                    type)
                );
            Options.Parameters.Merge(joinResult.Parameters);
            return this;
        }

        public IQueryBuilder<T> Join<U, W>(Expression<Func<U, W, bool>> foreignKeyProperty, JoinType type = JoinType.Inner)
            where U : new()
            where W : new()
        {
            var joinResult = dependencies.FilterParser.Parse(foreignKeyProperty, ref Options.ParamCount);
            Options.JoinQueries.Add(
                dependencies.JoinHandler.Value.Produce(
                    GetTableName<U>(),
                   joinResult.Query,
                    type)
                );
            Options.Parameters.Merge(joinResult.Parameters);
            return this;
        }

        public IQueryBuilder<T> ProcessConfig(Action<IProcessConfig> config)
        {
            config(dependencies.ProcessHandler);
            return this;
        }

        public virtual QueryResult GetQueryString()
        {
            // pipes!
            dependencies.ProcessHandler.PipeThrough(this);

            StringBuilder query = new StringBuilder();
            query.Append("SELECT ");
            if (Options.Distinct)
            {
                query.Append("DISTINCT ");
            }
            if (Options.Count)
            {
                query.Append("COUNT(*) ");
            }
            else
            {
                if (Options.Top.HasValue && !Options.Skip.HasValue)
                {
                    query.Append($"TOP {Options.Top.Value} ");
                }
                if (Options.SelectColumns.Any())
                {
                    query.Append(string.Join(",", Options.SelectColumns.Select(sc => GetColumnName(sc))) + " ");
                }
                else
                {
                    query.Append("* ");
                }
                if (Options.Subqueries.Any())
                {
                    query.Append(", ");
                    query.Append(string.Join(",", Options.Subqueries + " "));
                }
            }

            query.AppendLine($"FROM {GetTableName<T>()} {Options.Alias}");

            if (Options.JoinQueries.Any())
            {
                foreach (var joinPart in Options.JoinQueries)
                {
                    query.AppendLine(joinPart.ToString());
                }
            }

            if (Options.WhereStrings.Any())
            {
                query.AppendLine($" WHERE {string.Join(" AND ", Options.WhereStrings)}");
            }

            if (Options.GroupingColumns.Any())
            {
                query.AppendLine(" GROUP BY ");
                query.AppendLine($" {string.Join(",", Options.GroupingColumns.Select(GetColumnName))}");

            }

            if (Options.SortColumns.Any())
            {
                query.AppendLine(" ORDER BY ");
                query.AppendLine($" {string.Join(",", Options.SortColumns)}");
            }

            if (Options.Skip.HasValue)
            {
                query.AppendLine($"OFFSET {Options.Skip.Value} ROWS");
            }
            if (Options.Skip.HasValue && Options.Top.HasValue)
            {
                query.AppendLine($"FETCH NEXT {Options.Top.Value} ROWS ONLY");
            }

            if (Options.Json)
            {
                query.AppendLine("FOR JSON PATH");
            }

            // query.Append(";");
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public virtual async Task<T> ExecuteSingleAsync()
        {
            Top(1);
            var built = GetQueryString();
            if (Options.Json)
            {
                var jsonResult = await dependencies.Context.QueryAsync<T>(built.Query, built.Parameters);
                return JsonConvert.DeserializeObject<T>(string.Join("", jsonResult));
            }
            return await dependencies.Context.QueryFirstOrDefaultAsync<T>(built.Query, built.Parameters);
        }


        public virtual async Task<U> ExecuteSingleAsync<U>()
        {
            Top(1);
            var built = GetQueryString();
            if (Options.Json)
            {
                var jsonResult = await dependencies.Context.QueryAsync<U>(built.Query, built.Parameters);
                return JsonConvert.DeserializeObject<U[]>(string.Join("", jsonResult)).FirstOrDefault();
            }
            return await dependencies.Context.QueryFirstOrDefaultAsync<U>(built.Query, built.Parameters);
        }

        public virtual async Task<IEnumerable<T>> ExecuteAsync()
        {
            var built = GetQueryString();
            if (Options.Json)
            {
                var jsonResult = string.Join("", (await dependencies.Context.QueryAsync<string>(built.Query, built.Parameters)).Where(j => j != null));
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return new List<T>();
                }
                return JsonConvert.DeserializeObject<T[]>(jsonResult);
            }
            if (Options.Action != null)
            {
                return await Options.Action(dependencies.Context, built.Query, built.Parameters);
            }
            else
            {
                return await dependencies.Context.QueryAsync<T>(built.Query, built.Parameters);
            }


        }

        public IQueryBuilder<T> Map(Func<IDbConnection, string, object, Task<IEnumerable<T>>> action)
        {
            Options.Action = action;
            return this;
        }

        public virtual QueryResult GetInsertString(T entity)
        {
            // processes!
            entity = dependencies.ProcessHandler.RunThroughProcessesForInsert(entity);

            // here should be implemented joins and select on insert
            StringBuilder query = new StringBuilder();
            query.Append($"INSERT INTO {GetTableName<T>()} ");
            IEnumerable<string> columns = Options.SelectColumns.Any() ? Options.SelectColumns : dependencies.PropertyParser.Value.Parse<T>(e => e);

            query.AppendLine($"({string.Join(", ", columns.Select(d => GetColumnName(d)))})");

            query.AppendLine($"VALUES({string.Join(", ", columns.Select(p => $"@{Options.ParamCount++}"))})");

            query.Append(";");

            query.Append($"SELECT @@IDENTITY from {GetTableName<T>()}");
            Options.Parameters.Merge(ToDictionary(entity, ref Options.ParamCount, columns));
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public QueryResult GetInsertString(IEnumerable<T> entities)
        {
            if (!entities.Any())
            {
                throw new ArgumentException("Need atleast one entity to update");
            }
            return new QueryResult
            {
                Query = GetInsertString(entities.First()).ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public virtual QueryResult GetUpdateString(T entity)
        {
            // processes!
            entity = dependencies.ProcessHandler.RunThroughProcessesForUpdate(entity);

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {GetTableName<T>()} ");
            int innerCount = Options.ParamCount;
            IEnumerable<string> columns = Options.SelectColumns.Any() ? Options.SelectColumns : dependencies.PropertyParser.Value.Parse<T>(e => e);
            query.AppendLine("SET ");
            query.AppendLine(string.Join(", ", columns.Select(column => $"{GetColumnName(column)} = {parameterBinding}{innerCount++}")));

            if (Options.JoinQueries.Any())
            {
                var firstJoin = Options.JoinQueries.First();
                query.AppendLine($"FROM {firstJoin.Table}");
                Options.WhereStrings.Add(firstJoin.Condition);
                foreach (var join in Options.JoinQueries.Skip(1))
                {
                    query.AppendLine(join.ToString());
                }
            }

            if (Options.WhereStrings.Any())
            {
                query.AppendLine(" WHERE ");
                query.AppendLine(string.Join(" AND ", Options.WhereStrings));
            }
            Options.Parameters.Merge(ToDictionary(entity, ref Options.ParamCount, columns));
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public QueryResult GetDeleteString()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine($"DELETE FROM {GetTableName<T>()}");

            if (Options.JoinQueries.Any())
            {
                foreach (var joinPart in Options.JoinQueries)
                {
                    query.AppendLine(joinPart.ToString());
                }
            }

            if (Options.WhereStrings.Any())
            {
                query.AppendLine(" WHERE ");
                foreach (var wherePart in Options.WhereStrings)
                {
                    query.AppendLine(wherePart);
                }
            }

            query.Append(";");
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public IQueryBuilder<T> ParentAlias<U>(string alias) where U : new()
        {
            dependencies.FilterParser.SetParentAlias<U>(alias);
            Options.ParentAlias = alias;
            return this;
        }

        public IQueryBuilder<T> Alias(string alias)
        {
            dependencies.FilterParser.SetAlias<T>(alias);
            Options.Alias = alias;
            return this;
        }

        protected Dictionary<string, object> ToDictionary(T obj, ref int id, IEnumerable<string> columns)
        {
            int count = id;
            columns = columns.OrderBy(col => col);
            var dictionary = obj.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(prop => prop.Name)
            .Where(prop => columns.Any(col => col.ToLower() == prop.Name.ToLower()))
            .ToDictionary(prop =>
                (count++).ToString(),
            prop => prop.GetValue(obj, null)
            );
            id = count;
            return dictionary;
        }

        protected virtual string GetTableName<U>()
        {
            return $"[{dependencies.NamingStrategy.GetTableName(typeof(U))}]";
        }

        protected virtual string GetColumnName(string property)
        {
            if (property.Contains(".")) return property;
            return $"{Options.Alias ?? GetTableName<T>()}.[{property}]";
        }

        public int GetParamCount()
        {
            return Options.ParamCount;
        }
    }

    public class QueryResult
    {
        public QueryResult()
        {

        }

        public QueryResult(string query, Dictionary<string, object> parameters)
        {
            Query = query;
            Parameters = parameters;
        }

        public string Query { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public int Count { get; set; }
    }

    public class QueryBuilderOptions<T>
    {
        public bool Distinct = false;
        public bool Count = false;
        public bool Json = false;
        public int ParamCount = 1;
        public int? Top;
        public string Alias;
        public string ParentAlias;
        public int? Skip;
        public List<string> SelectColumns = new List<string>();
        public List<string> Subqueries = new List<string>();
        public List<string> GroupingColumns = new List<string>();
        public List<string> SortColumns = new List<string>();
        public List<string> WhereStrings = new List<string>();
        public List<JoinQuery> JoinQueries = new List<JoinQuery>();
        public Func<IDbConnection, string, object, Task<IEnumerable<T>>> Action;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
    }

    public class JoinQuery
    {
        public string Table { get; set; }
        public string Condition { get; set; }
        public string JoinType { get; set; }
        public string ToString() => $" {JoinType} {Table} ON {Condition}";
    }
}
