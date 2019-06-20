using System;
using System.Data;
using Autofac;

namespace Dapper.Builder.Autofac.Configuration
{
    /// <summary>
    /// Builder configuration for Autofac
    /// </summary>
    public class AutofacBuilderConfiguration : BaseBuilderConfiguration, IBuilderConfiguration

        /// <summary>
        /// The type of database so to create queries for them
        /// </summary>
    public DatabaseType DatabaseType { get; set; }
        
    public class AutofacBuilderConfiguration : BaseBuilderConfiguration, IBuilderConfiguration
    {
        /// <summary>
        /// The Function that provides a connection to the database.
        /// </summary>
        public Func<IComponentContext, IDbConnection> DbConnectionFactory { get; set; }


    }
}
