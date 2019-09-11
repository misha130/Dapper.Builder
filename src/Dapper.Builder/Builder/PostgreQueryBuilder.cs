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
using Dapper.Builder.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Builder.Services
{
    /// <summary>
    /// Class QueryBuilder.
    /// </summary>
    public class PostgreQueryBuilder<TEntity> : QueryBuilder<TEntity> where TEntity : new()
    {

        public PostgreQueryBuilder(IQueryBuilderDependencies<TEntity> dependencies) : base(dependencies)
        {
        }
        protected override string parameterBinding => ":";
        public override QueryResult GetQueryString()
        {
            // pipes!
            dependencies.ProcessHandler.PipeThrough(this);

            StringBuilder query = new StringBuilder();
            IEnumerable<string> columns = Options.SelectColumns;
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
                if (columns.Any())
                {
                    columns = columns.Where(col => !Options.ExcludeColumns.Any(ec => string.Equals(ec, col, StringComparison.OrdinalIgnoreCase)));
                    query.Append(string.Join(",", columns.Select(sc =>
                    dependencies.NamingStrategy.GetTableAndColumnName<TEntity>(sc))));
                    query.Append(" ");
                }
                else
                {
                    if (Options.ExcludeColumns.Any())
                    {
                        columns = dependencies.PropertyParser.Value.Parse<TEntity>(e => new TEntity());
                        columns = columns.Where(col => !Options.ExcludeColumns.Any(ec => string.Equals(ec, col, StringComparison.OrdinalIgnoreCase)));
                        query.Append(string.Join(",", columns.Select(sc =>
                        dependencies.NamingStrategy.GetTableAndColumnName<TEntity>(sc))));
                        query.Append(" ");
                    }
                    else
                    {
                        query.Append("* ");
                    }
                }
                if (Options.Subqueries.Any())
                {
                    query.Append(", ");
                    query.Append(string.Join(", ", Options.Subqueries));
                    query.AppendLine();
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
                query.AppendLine($" {string.Join(",", Options.GroupingColumns.Select(col => dependencies.NamingStrategy.GetTableAndColumnName<TEntity>(col)))}");
            }

            if (!Options.Count)
            {
                if (Options.SortColumns.Any())
                {
                    query.AppendLine(" ORDER BY ");
                    query.AppendLine($" {string.Join(",", Options.SortColumns)}");
                }

                if (Options.Top.HasValue)
                {
                    query.AppendLine($"OFFSET {Options.Skip ?? 0} ROWS");
                    query.AppendLine($"FETCH FIRST {Options.Top.Value} ROWS ONLY");
                }

                if (Options.Json)
                {
                    if (Options.JsonPrimitive)
                    {
                        if (columns.Count() > 1) throw new ArgumentException("Json primitive support only one column");
                        var forArrayJsonQuery = $@"select array_to_json(array_agg({dependencies.NamingStrategy.GetTableAndColumnName<TEntity>($"t.{columns.FirstOrDefault()}")})) from ( {query})  t";
                        query = new StringBuilder(forArrayJsonQuery);
                    }
                    else
                    {
                        if (Options.Top == 1 && (!Options.Skip.HasValue || Options.Skip.Value == 0))
                        {
                            var forJsonQuery = $"select row_to_json(t) from ( {query})  t";
                            query = new StringBuilder(forJsonQuery);
                        }
                        else
                        {
                            var forArrayJsonQuery = $"select array_to_json(array_agg(row_to_json(t))) from ( {query})  t";
                            query = new StringBuilder(forArrayJsonQuery);
                        }
                    }
                }
            }
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }
        public override QueryResult GetInsertString(TEntity entity)
        {
            // processes!
            entity = dependencies.ProcessHandler.RunThroughProcessesForInsert(entity);

            // here should be implemented joins and select on insert
            StringBuilder query = new StringBuilder();
            query.Append($"INSERT INTO {dependencies.NamingStrategy.GetTableName<TEntity>()} ");
            IEnumerable<string> columns = Options.SelectColumns.Any() ?
                                          Options.SelectColumns :
                                          dependencies.PropertyParser.Value.Parse<TEntity>(e => e);
            columns = columns.Where(col => !Options.ExcludeColumns.Any(ec => string.Equals(ec, col, StringComparison.OrdinalIgnoreCase)));

            int innerCount = Options.Parameters.Count + 1;
            query.AppendLine($"({string.Join(", ", columns.Select(d => $"\"{d.ToCamelCase()}\""))})");
            query.AppendLine($"VALUES({string.Join(", ", columns.Select(p => $":{innerCount++}"))})");
            query.Append("RETURNING  Id");
            Options.ParamCount = Options.Parameters.Count + 1;
            Options.Parameters.Merge(entity.ToDictionary(ref Options.ParamCount, columns));
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public override QueryResult GetUpdateString(TEntity entity)
        {
            // processes!
            entity = dependencies.ProcessHandler.RunThroughProcessesForUpdate(entity);

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {dependencies.NamingStrategy.GetTableName<TEntity>()} ");
            int innerCount = Options.Parameters.Count + 1;

            IEnumerable<string> columns = Options.SelectColumns.Any() ?
                                          Options.SelectColumns :
                                         dependencies.PropertyParser.Value.Parse<TEntity>(e => e);
            columns = columns.Where(col => !Options.ExcludeColumns.Any(ec => string.Equals(ec, col, StringComparison.OrdinalIgnoreCase)));
            query.AppendLine("SET ");
            query.AppendLine(string.Join(", ", columns.Select(column => $"{dependencies.NamingStrategy.GetColumnName<TEntity>(column)} = { parameterBinding }{ innerCount++}")));

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
            query.AppendLine("RETURNING Id");
            Options.ParamCount = Options.Parameters.Count + 1;
            Options.Parameters.Merge(entity.ToDictionary(ref Options.ParamCount, columns));
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public override IQueryBuilder<TEntity> Columns<U>(params string[] columns)
        {
            Options.SelectColumns.AddRange(columns.Select(col => dependencies.NamingStrategy.GetTableAndColumnName<TEntity>(col)));
            return this;
        }

        //TODO: do upsert
        //public override async Task<long> ExecuteUpsertAsync(TEntity entity, Expression<Func<TEntity, object>> conflict)
        //{
        //    StringBuilder query = new StringBuilder();
        //    var insertQuery = GetInsertString(entity);
        //    var updateQuery = GetUpdateString(entity);

        //    var properties = dependencies.PropertyParser.Value.Parse(conflict);
        //    query.AppendLine(insertQuery.Query);
        //    query.AppendLine($"ON CONFLICT({string.Join(",", properties)}) DO UPDATE");
        //    query.AppendLine(updateQuery.Query);
        //    query.AppendLine("RETURNING id");

        //    return await dependencies.Context.QueryFirstOrDefaultAsync<long>(query.ToString(), Options.Parameters);
        //}
    }
}