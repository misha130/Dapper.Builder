using System;
using System.Collections.Generic;
using System.Data;
using Autofac;
using Dapper.Builder.Builder.Processes.Interfaces;
using Dapper.Builder.Extensions;
using Dapper.Builder.Shared.Interfaces;

namespace Dapper.Builder.Autofac.Configuration {
    /// <summary>
    /// Builder configuration for Autofac
    /// </summary>
    public class AutofacBuilderConfiguration : IBuilderConfiguration {
        /// <summary>
        /// The type of database so to create queries for them
        /// </summary>
        public DatabaseType DatabaseType { get; set; }
        /// <summary>
        /// The Function that provides a connection to the database.
        /// </summary>
        public Func<IComponentContext, IDbConnection> DbConnectionFactory { get; set; }
    }
}