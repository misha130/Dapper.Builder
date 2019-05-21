using Dapper.Builder.Builder.Processes.Interfaces;
using Dapper.Builder.Extensions;
using Dapper.Builder.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Builder.Core.Configuration
{
    public class CoreBuilderConfiguration : IBuilderConfiguration
    {
        public DatabaseType DatabaseType { get; set; }
        public Func<IServiceProvider, IDbConnection> DbConnectionFactory { get; set; }
    }
}