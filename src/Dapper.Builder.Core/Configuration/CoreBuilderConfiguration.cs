using Dapper.Builder.Builder.Processes.Interfaces;
using Dapper.Builder.Extensions;
using Dapper.Builder.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Builder.Core.Configuration
{
    /// <summary>
    /// Builder configuration for .net core
    /// </summary>
    public class CoreBuilderConfiguration : IBuilderConfiguration
    {
        /// <summary>
        /// The type of database so to create queries for them
        /// </summary>
        public DatabaseType DatabaseType { get; set; }
        /// <summary>
        /// The Function that provides a connection to the database.
        /// </summary>
        public Func<IServiceProvider, IDbConnection> DbConnectionFactory { get; set; }
    }
}