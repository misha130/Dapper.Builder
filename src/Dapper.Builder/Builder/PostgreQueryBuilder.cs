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
using Dapper.Builder.Builder;
using Dapper.Builder.Dependencies_Configuration.Aggregates;
using Dapper.Builder.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Builder.Services.DAL.Builder
{
    /// <summary>
    /// Class QueryBuilder.
    /// </summary>
    public class PostgreQueryBuilder<T> : QueryBuilder<T> where T : new()
    {
        protected override string parameterBinding => ":";
        public PostgreQueryBuilder(IQueryBuilderDependencies<T> dependencies) : base(dependencies)
        {

        }
        public override QueryResult GetQueryString()
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
                if (Options.SelectColumns.Any())
                {
                    query.Append(string.Join(",", Options.SelectColumns.Select(sc => GetColumnName(sc))));
                    query.Append(" ");
                }
                else
                {
                    query.Append("* ");
                }
                if (Options.Subqueries.Any())
                {
                    query.Append(", ");
                    query.Append(string.Join(", ", Options.Subqueries));
                    query.AppendLine();
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

            if (Options.Top.HasValue)
            {
                query.AppendLine($"OFFSET {Options.Skip ?? 0} ROWS");
                query.AppendLine($"FETCH FIRST {Options.Top.Value} ROWS ONLY");
            }

            if (Options.Json)
            {
                var forJsonQuery = $"select array_to_json(array_agg(row_to_json(t))) from ( {query})  t";
                query = new StringBuilder(forJsonQuery);
            }

            // query.Append(";");
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }
        public override QueryResult GetInsertString(T entity)
        {
            // processes!
            entity = dependencies.ProcessHandler.RunThroughProcessesForInsert(entity);

            // here should be implemented joins and select on insert
            StringBuilder query = new StringBuilder();
            query.Append($"INSERT INTO {GetTableName<T>()} ");
            IEnumerable<string> columns = Options.SelectColumns.Any() ? Options.SelectColumns : dependencies.PropertyParser.Value.Parse<T>(e => e);
            int innerCount = Options.Parameters.Count + 1;
            query.AppendLine($"({string.Join(", ", columns.Select(d => $"\"{d.ToCamelCase()}\""))})");

            query.AppendLine($"VALUES ({string.Join(", ", columns.Select(p => $":{innerCount++}"))})");
            query.Append("RETURNING  Id;");
            Options.ParamCount = Options.Parameters.Count + 1;
            Options.Parameters.Merge(ToDictionary(entity, ref Options.ParamCount, columns));
            return new QueryResult
            {
                Query = query.ToString(),
                Parameters = Options.Parameters,
                Count = Options.ParamCount
            };
        }

        public override QueryResult GetUpdateString(T entity)
        {
            // processes!
            entity = dependencies.ProcessHandler.RunThroughProcessesForUpdate(entity);

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {GetTableName<T>()} ");
            int innerCount = Options.Parameters.Count + 1;
            Options.ParamCount = Options.Parameters.Count + 1;
            IEnumerable<string> columns = Options.SelectColumns.Any() ? Options.SelectColumns :
             dependencies.PropertyParser.Value.Parse<T>(e => e);
            query.AppendLine("SET ");
            query.AppendLine(string.Join(", ", columns.Select(column => $"\"{column.ToCamelCase()}\" = {parameterBinding}{innerCount++}")));

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

        public override IQueryBuilder<T> Columns<U>(params string[] columns)
        {
            Options.SelectColumns.AddRange(columns.Select(col => $"{typeof(U)}.{col}"));
            return this;
        }
    }
}