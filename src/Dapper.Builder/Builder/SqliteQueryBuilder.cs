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

        public override IQueryBuilder<TEntity> Json()
        {
            throw new NotSupportedException("Json data is not supported currently in SQLite");
        }
    }
}