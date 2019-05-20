// ***********************************************************************
// Assembly         : Dapper.Builder
// Author           : micha
// Created          : 01-28-2019
//
// Last Modified By : micha
// Last Modified On : 01-28-2019
// ***********************************************************************
// <copyright file="IProcess.cs" company="Dapper.Builder">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace Dapper.Builder.Builder.Processes.Interfaces
{
    /// <summary>
    /// Interface IProcess
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// Processes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        void Process<T>(T entity) where T : new();
    }
}
