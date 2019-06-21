using System;
using System.Data;
using Autofac;

namespace Dapper.Builder.Autofac
{
    /// <summary>
    /// Builder configuration for Autofac
    /// </summary>
    public class AutofacBuilderConfiguration : BaseBuilderConfiguration, IBuilderConfiguration
    {

        /// <summary>
        /// The Function that provides a connection to the database.
        /// </summary>
        public Func<IComponentContext, IDbConnection> DbConnectionFactory { get; set; }


    }
}
