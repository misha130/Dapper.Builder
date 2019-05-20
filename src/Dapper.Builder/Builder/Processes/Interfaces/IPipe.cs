// ***********************************************************************
// Assembly         : Dapper.Builder
// Author           : micha
// Created          : 01-28-2019
//
// Last Modified By : micha
// Last Modified On : 01-28-2019
// ***********************************************************************
// <copyright file="IPipe.cs" company="Dapper.Builder">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Dapper.Builder.Services.DAL.Builder;

namespace Dapper.Builder.Builder.Processes.Interfaces {
    /// <summary>
    /// Interface IPipe
    /// </summary>
    public interface IPipe {
        /// <summary>
        /// Pipes this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.String.</returns>
        void Pipe<T> (IQueryBuilder<T> builder) where T : new ();
    }
}