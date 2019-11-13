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
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Builder.Services
{
    /// <summary>
    /// Class QueryBuilder.
    /// </summary>
    public class SqliteQueryBuilder<TEntity> : QueryBuilder<TEntity> where TEntity : new()
    {
        public SqliteQueryBuilder(IQueryBuilderDependencies<TEntity> depenedencies) : base(depenedencies)
        {
        }

        public override QueryResult GetQueryString()
        {
            // pipes!
            Dependencies.ProcessHandler.PipeThrough(this);

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
                    columns = columns.Where(col =>
                        !Options.ExcludeColumns.Any(ec => string.Equals(ec, col, StringComparison.OrdinalIgnoreCase)));
                    query.Append(string.Join(",", columns.Select(sc =>
                        Dependencies.NamingStrategy.GetTableAndColumnName<TEntity>(sc))));
                    query.Append(" ");
                }
                else
                {
                    if (Options.ExcludeColumns.Any())
                    {
                        columns = Dependencies.PropertyParser.Value.Parse<TEntity>(e => new TEntity());
                        columns = columns.Where(col =>
                            !Options.ExcludeColumns.Any(
                                ec => string.Equals(ec, col, StringComparison.OrdinalIgnoreCase)));
                        query.Append(string.Join(",", columns.Select(sc =>
                            Dependencies.NamingStrategy.GetTableAndColumnName<TEntity>(sc))));
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
                }
            }

            query.AppendLine($"FROM {Dependencies.NamingStrategy.GetTableName<TEntity>()} {Options.Alias}");

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
                    .Select(x => Dependencies.NamingStrategy.GetTableAndColumnName<TEntity>(x)))}
                ");
            }

            if (Options.SortColumns.Any())
            {
                query.AppendLine(" ORDER BY ");
                query.AppendLine($" {string.Join(",", Options.SortColumns)}");
            }


            if (Options.Skip.HasValue && !Options.Top.HasValue)
            {
                query.AppendLine($"LIMIT -1 OFFSET {Options.Skip.Value}");
            }
            else
            {
                if (Options.Top.HasValue)
                {
                    query.AppendLine($"LIMIT {Options.Top.Value}");
                }

                if (Options.Skip.HasValue)
                {
                    query.Append($" OFFSET {Options.Skip.Value}");
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
            entity = Dependencies.ProcessHandler.RunThroughProcessesForInsert(entity);
            // here should be implemented joins and select on insert
            var query = new StringBuilder();
            var insertQueryPart = GetInsertQueryInsetPart(query);
            query = insertQueryPart.quert;
            var columns = insertQueryPart.columns;
            query = GetInsertQueryValuesPart(query, columns);
            query.Append($"SELECT LAST_INSERT_ROWID()");
            return GetQueryResult(entity, query, columns);
        }

        public override QueryResult GetUpdateString(TEntity entity)
        {
            // processes!
            entity = Dependencies.ProcessHandler.RunThroughProcessesForUpdate(entity);

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {Dependencies.NamingStrategy.GetTableName<TEntity>()} ");
            int innerCount = Options.ParamCount;
            IEnumerable<string> columns = Options.SelectColumns.Any() ? Options.SelectColumns : Dependencies.PropertyParser.Value.Parse<TEntity>(e => e);
            columns = columns.Where(col => !Options.ExcludeColumns.Any(ec => string.Equals(ec, col, StringComparison.OrdinalIgnoreCase)));
            query.AppendLine("SET ");
            query.AppendLine(string.Join(", ", columns.Select(column => $"{Dependencies.NamingStrategy.GetColumnName<TEntity>(column)} = {parameterBinding}{innerCount++}")));

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


        public override QueryResult GetInsertString<UEntity>(TEntity entity, Expression<Func<TEntity, UEntity>> property)
        {
            entity = Dependencies.ProcessHandler.RunThroughProcessesForInsert(entity);
            var query = new StringBuilder();
            var insertQueryPart = GetInsertQueryInsetPart(query);
            query = insertQueryPart.quert;
            var columns = insertQueryPart.columns;
            query.Append($" SELECT {Dependencies.PropertyParser.Value.Parse(property)} FROM {Dependencies.NamingStrategy.GetTableName<TEntity>()} ORDER BY row_id DESC LIMIT 1;");
            query = GetInsertQueryValuesPart(query, columns);
            return GetQueryResult(entity, query, columns);
        }
        public override (StringBuilder quert, List<string> columns) GetInsertQueryInsetPart(StringBuilder query)
        {
            query.Append($"INSERT INTO {Dependencies.NamingStrategy.GetTableName<TEntity>()} ");
            var columns = Options.SelectColumns.Any() ? Options.SelectColumns : Dependencies.PropertyParser.Value.Parse<TEntity>(e => e).ToList();
            columns = columns.Where(col => !Options.ExcludeColumns.Any(ec => string.Equals(ec, col, StringComparison.OrdinalIgnoreCase))).ToList();

            //int innerCount = Options.Parameters.Count + 1;
            query.AppendLine($"({string.Join(", ", columns.Select(d => Dependencies.NamingStrategy.GetColumnName<TEntity>(d)))})");
            return (query, columns);
        }

        protected override StringBuilder GetInsertQueryValuesPart(StringBuilder query, List<string> columns)
        {
            query.AppendLine($"VALUES({string.Join(", ", columns.Select(p => $"@{Options.ParamCount++}"))});");
            return query;
        }

        public override IQueryBuilder<TEntity> Json()
        {
            throw new NotSupportedException("Json data is not supported currently in SQLite");
        }
    }
}