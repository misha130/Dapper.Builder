using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Dapper.Builder.Extensions;
using Newtonsoft.Json;
using Dapper.Builder.Services;
using Dapper.Builder.Dependencies_Configuration.Aggregates;
using Dapper.Builder.Processes;

namespace Dapper.Builder
{
    /// <summary>
    /// Class QueryBuilder.
    /// </summary>
    public class QueryBuilder<TEntity> : IInternalQueryBuilder, IQueryBuilder<TEntity> where TEntity : new()
    {
        protected readonly IQueryBuilderDependencies<TEntity> dependencies;
        public QueryBuilder(IQueryBuilderDependencies<TEntity> dependencies)
        {
            this.dependencies = dependencies;
        }
        protected QueryBuilderOptions<TEntity> Options = new QueryBuilderOptions<TEntity>();

        protected virtual string parameterBinding => "@";


        public IQueryBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            var whereResult = dependencies.FilterParser.Parse(predicate, ref Options.ParamCount);
            Options.WhereStrings.Add(whereResult.Query);
            Options.Parameters.Merge(whereResult.Parameters);
            return this;
        }

        public void ParamCount(int count)
        {
            Options.ParamCount += count;
        }

        public IQueryBuilder<TEntity> Where<UEntity>(Expression<Func<TEntity, UEntity, bool>> predicate) where UEntity : new()
        {
            var whereResult = dependencies.FilterParser.Parse<UEntity>(predicate, ref Options.ParamCount);
            Options.WhereStrings.Add(whereResult.Query);
            Options.Parameters.Merge(whereResult.Parameters);
            return this;

        }

        public IQueryBuilder<TEntity> Json()
        {
            Options.Json = true;
            return this;
        }

        public IQueryBuilder<TEntity> Distinct()
        {
            Options.Distinct = true;
            return this;
        }

        public IQueryBuilder<TEntity> Columns(params string[] columns)
        {
            Options.SelectColumns.AddRange(columns);
            return this;
        }
        public virtual IQueryBuilder<TEntity> Columns<UEntity>(params string[] columns) where UEntity : new()
        {
            Options.SelectColumns.AddRange(columns.Select(col => $"{typeof(UEntity)}.{col}"));
            return this;
        }

        public IQueryBuilder<TEntity> Columns(Expression<Func<TEntity, object>> properties)
        {
            Options.SelectColumns.AddRange(dependencies.PropertyParser.Value.Parse(properties));
            return this;
        }

        public virtual IQueryBuilder<TEntity> SubQuery<UEntity>(Func<IQueryBuilder<UEntity>, IQueryBuilder<UEntity>> query, string alias)
            where UEntity : new()
        {
            var queryBuilder = dependencies.ResolveService<IQueryBuilder<UEntity>>();
            if (queryBuilder is IInternalQueryBuilder internalQueryBuilder)
            {
                internalQueryBuilder.ParamCount(Options.ParamCount);
            }
            var result = query(queryBuilder).GetQueryString();
            ParamCount(result.Count);
            Options.Subqueries.Add($"({result.Query}) as {alias}");
            if (result.Parameters != null)
            {
                Options.Parameters.Merge(result.Parameters);
            }
            return this;
        }

        public IQueryBuilder<TEntity> Count()
        {
            Options.Count = true;
            return this;
        }

        public IQueryBuilder<TEntity> GroupBy(Expression<Func<TEntity, object>> columns)
        {
            Options.GroupingColumns.AddRange(dependencies.PropertyParser.Value.Parse(columns, false));
            return this;
        }
        public IQueryBuilder<TEntity> GroupBy(params string[] columns)
        {
            Options.GroupingColumns.AddRange(columns);
            return this;
        }


        public IQueryBuilder<TEntity> SortAscending(Expression<Func<TEntity, object>> columns)
        {
            Options.SortColumns.AddRange(dependencies.PropertyParser.Value.Parse(columns, true)
                .Select(sort => dependencies.SortHandler.Value
                    .Produce<TEntity>(new SortColumn { Field = sort, Dir = SortType.Asc }, Options.Alias)
                    )
                );
            return this;
        }
        public IQueryBuilder<TEntity> SortDescending(Expression<Func<TEntity, object>> columns)
        {
            Options.SortColumns.AddRange(dependencies.PropertyParser.Value.Parse(columns, true)
                .Select(sort => dependencies.SortHandler.Value
                    .Produce<TEntity>(new SortColumn { Field = sort, Dir = SortType.Desc }, Options.Alias)
                    )
                );
            return this;
        }

        public IQueryBuilder<TEntity> SortAscending<UEntity>(Expression<Func<UEntity, object>> sortProperty) where UEntity : new()
        {
            Options.SortColumns.AddRange(dependencies.PropertyParser.Value.Parse(sortProperty, true)
               .Select(sort => dependencies.SortHandler.Value
                   .Produce<UEntity>(new SortColumn { Field = sort, Dir = SortType.Desc })
                   )
               );
            return this;
        }

        public IQueryBuilder<TEntity> SortDescending<UEntity>(Expression<Func<UEntity, object>> sortProperty) where UEntity : new()
        {
            Options.SortColumns.AddRange(dependencies.PropertyParser.Value.Parse(sortProperty, true)
                  .Select(sort => dependencies.SortHandler.Value
                      .Produce<UEntity>(new SortColumn { Field = sort, Dir = SortType.Desc })
                      )
                  );
            return this;
        }

        public IQueryBuilder<TEntity> Sort(params SortColumn[] sortColumns)
        {
            if (sortColumns != null)
            {
                Options.SortColumns.AddRange(sortColumns.Where(sc => sc != null).Select(sort => dependencies.SortHandler.Value.Produce<TEntity>(sort)));
            }
            return this;
        }

        public IQueryBuilder<TEntity> Top(int? top)
        {
            Options.Top = top;
            return this;
        }

        public IQueryBuilder<TEntity> Skip(int? skip)
        {
            Options.Skip = skip;
            return this;
        }

        public IQueryBuilder<TEntity> Join<UEntity>(Expression<Func<TEntity, UEntity, bool>> predicate, JoinType type = JoinType.Inner)
            where UEntity : new()
        {

            var joinResult = dependencies.FilterParser.Parse<UEntity>(predicate, ref Options.ParamCount);
            Options.JoinQueries.Add(
                dependencies.JoinHandler.Value.Produce(
                  dependencies.NamingStrategy.GetTableName<UEntity>(),
                   joinResult.Query,
                    type)
                );
            Options.Parameters.Merge(joinResult.Parameters);
            return this;
        }

        public IQueryBuilder<TEntity> Join<UEntity, WEntity>(Expression<Func<UEntity, WEntity, bool>> predicate, JoinType type = JoinType.Inner)
            where UEntity : new()
            where WEntity : new()
        {
            var joinResult = dependencies.FilterParser.Parse(predicate, ref Options.ParamCount);
            Options.JoinQueries.Add(
                dependencies.JoinHandler.Value.Produce(
                   dependencies.NamingStrategy.GetTableName<UEntity>(),
                   joinResult.Query,
                    type)
                );
            Options.Parameters.Merge(joinResult.Parameters);
            return this;
        }

        public IQueryBuilder<TEntity> ProcessConfig(Action<IProcessConfig> config)
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
                    query.Append(string.Join(",", Options.SelectColumns.Select(sc =>
                   dependencies.NamingStrategy.GetColumnName<TEntity>(sc))) + " ");
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

            query.AppendLine($"FROM {dependencies.NamingStrategy.GetTableName<TEntity>()} {Options.Alias}");

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
                query.AppendLine($@"{string.Join(",", Options.GroupingColumns
                    .Select(x => dependencies.NamingStrategy.GetColumnName<TEntity>(x)))}
                ");

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

        public virtual async Task<TEntity> ExecuteSingleAsync()
        {
            Top(1);
            var built = GetQueryString();
            if (Options.Json)
            {
                var jsonResult = await dependencies.Context.QueryAsync<TEntity>(built.Query, built.Parameters);
                return JsonConvert.DeserializeObject<TEntity>(string.Join("", jsonResult));
            }
            return await dependencies.Context.QueryFirstOrDefaultAsync<TEntity>(built.Query, built.Parameters);
        }


        public virtual async Task<UEntity> ExecuteSingleAsync<UEntity>()
        {
            Top(1);
            var built = GetQueryString();
            if (Options.Json)
            {
                var jsonResult = await dependencies.Context.QueryAsync<UEntity>(built.Query, built.Parameters);
                return JsonConvert.DeserializeObject<UEntity[]>(string.Join("", jsonResult)).FirstOrDefault();
            }
            return await dependencies.Context.QueryFirstOrDefaultAsync<UEntity>(built.Query, built.Parameters);
        }

        public virtual async Task<IEnumerable<TEntity>> ExecuteAsync()
        {
            var built = GetQueryString();
            if (Options.Json)
            {
                var jsonResult = string.Join("", (await dependencies.Context.QueryAsync<string>(built.Query, built.Parameters)).Where(j => j != null));
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return new List<TEntity>();
                }
                return JsonConvert.DeserializeObject<TEntity[]>(jsonResult);
            }
            if (Options.Action != null)
            {
                return await Options.Action(dependencies.Context, built.Query, built.Parameters);
            }
            else
            {
                return await dependencies.Context.QueryAsync<TEntity>(built.Query, built.Parameters);
            }


        }

        public IQueryBuilder<TEntity> Map(Func<IDbConnection, string, object, Task<IEnumerable<TEntity>>> action)
        {
            Options.Action = action;
            return this;
        }

        public virtual QueryResult GetInsertString(TEntity entity)
        {
            // processes!
            entity = dependencies.ProcessHandler.RunThroughProcessesForInsert(entity);

            // here should be implemented joins and select on insert
            StringBuilder query = new StringBuilder();
            query.Append($"INSERT INTO {dependencies.NamingStrategy.GetTableName<TEntity>()} ");
            IEnumerable<string> columns = Options.SelectColumns.Any() ? Options.SelectColumns : dependencies.PropertyParser.Value.Parse<TEntity>(e => e);

            query.AppendLine($"({string.Join(", ", columns.Select(d => dependencies.NamingStrategy.GetColumnName<TEntity>(d)))})");

            query.AppendLine($"VALUES({string.Join(", ", columns.Select(p => $"@{Options.ParamCount++}"))})");

            query.Append(";");

            query.Append($"SELECT @@IDENTITY from {dependencies.NamingStrategy.GetTableName<TEntity>()}");
            Options.Parameters.Merge(entity.ToDictionary(ref Options.ParamCount, columns));
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public QueryResult GetInsertString(IEnumerable<TEntity> entities)
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

        public virtual QueryResult GetUpdateString(TEntity entity)
        {
            // processes!
            entity = dependencies.ProcessHandler.RunThroughProcessesForUpdate(entity);

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {dependencies.NamingStrategy.GetTableName<TEntity>()} ");
            int innerCount = Options.ParamCount;
            IEnumerable<string> columns = Options.SelectColumns.Any() ? Options.SelectColumns : dependencies.PropertyParser.Value.Parse<TEntity>(e => e);
            query.AppendLine("SET ");
            query.AppendLine(string.Join(", ", columns.Select(column => $"{dependencies.NamingStrategy.GetColumnName<TEntity>(column)} = {parameterBinding}{innerCount++}")));

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
            Options.Parameters.Merge(entity.ToDictionary(ref Options.ParamCount, columns));
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
            query.AppendLine($"DELETE FROM {dependencies.NamingStrategy.GetTableName<TEntity>()}");

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

        public IQueryBuilder<TEntity> ParentAlias<UEntity>(string alias) where UEntity : new()
        {
            dependencies.FilterParser.SetParentAlias<UEntity>(alias);
            Options.ParentAlias = alias;
            return this;
        }

        public IQueryBuilder<TEntity> Alias(string alias)
        {
            dependencies.FilterParser.SetAlias(alias);
            Options.Alias = alias;
            return this;
        }




        public int GetParamCount()
        {
            return Options.ParamCount;
        }

        public async Task<int> ExecuteUpdateAsync(TEntity entity)
        {
            var query = GetUpdateString(entity);
            return await dependencies.Context.ExecuteAsync(query.Query, query.Parameters);
        }


        public async Task<int> ExecuteDeleteAsync()
        {
            var query = GetDeleteString();
            return await dependencies.Context.ExecuteAsync(query.Query, query.Parameters);
        }

        public async Task<long> ExecuteInsertAsync(TEntity entity)
        {
            var query = GetInsertString(entity);
            return await dependencies.Context.QueryFirstOrDefaultAsync<long>(query.Query, query.Parameters);
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

    public class QueryBuilderOptions<TEntity>
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
        public Func<IDbConnection, string, object, Task<IEnumerable<TEntity>>> Action;
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
