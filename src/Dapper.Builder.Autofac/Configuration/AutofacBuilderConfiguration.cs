using Autofac;
using Dapper.Builder.Builder.Processes.Interfaces;
using Dapper.Builder.Extensions;
using Dapper.Builder.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Builder.Autofac.Configuration
{
    public class AutofacBuilderConfiguration : IBuilderConfiguration
    {
        public DatabaseType DatabaseType { get; set; }
        public Func<IComponentContext, IDbConnection> DbConnectionFactory { get; set; }
    }
}