using System;
using System.Data;

namespace Dapper.Builder.Core
{
    /// <summary>
    /// Builder configuration for .net core
    /// </summary>
    public class CoreBuilderConfiguration : BaseBuilderConfiguration, IBuilderConfiguration
    {
        /// <summary>
        /// The Function that provides a connection to the database.
        /// </summary>
        public Func<IServiceProvider, IDbConnection> DbConnectionFactory { get; set; }
    }
}