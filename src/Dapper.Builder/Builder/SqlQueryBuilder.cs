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
using System.Collections.Generic;

namespace Dapper.Builder.Services.DAL.Builder
{
    /// <summary>
    /// Class QueryBuilder.
    /// </summary>
    public class SqlQueryBuilder<T> : QueryBuilder<T> where T : new()
    {
        public SqlQueryBuilder(IQueryBuilderDependencies<T> depenedencies) : base(depenedencies)
        {

        }
        public override QueryResult GetQueryString()
        {
            return base.GetQueryString();
        }
    }
}
