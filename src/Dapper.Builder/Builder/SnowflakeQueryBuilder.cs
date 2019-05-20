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
using System.Collections;
using Dapper;
using Dapper.Builder.Services.DAL.Builder.FilterParser;
using Dapper.Builder.Services.DAL.Builder.PropertyParser;
using Dapper.Builder.Services.DAL.Builder.SortHandler;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Dapper.Builder.Builder;

namespace Dapper.Builder.Services.DAL.Builder
{
    /// <summary>
    /// Class QueryBuilder.
    /// </summary>
    public class SnowflakeQueryBuilder<T> : QueryBuilder<T> where T : new()
    {
        public SnowflakeQueryBuilder(IQueryBuilderDependencies<T> depenedencies) : base(depenedencies)
        {

        }
        public override QueryResult GetQueryString()
        {
            return base.GetQueryString();
        }
    }
}
