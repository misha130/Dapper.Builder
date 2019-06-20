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
using Dapper.Builder.Dependencies_Configuration.Aggregates;

namespace Dapper.Builder.Services
{
    /// <summary>
    /// Class QueryBuilder.
    /// </summary>
    public class SqlQueryBuilder<TEntity> : QueryBuilder<TEntity> where TEntity : new()
    {
        public SqlQueryBuilder(IQueryBuilderDependencies<TEntity> depenedencies) : base(depenedencies)
        {

        }
        public override QueryResult GetQueryString()
        {
            return base.GetQueryString();
        }
    }
}
