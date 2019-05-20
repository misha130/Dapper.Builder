using Dapper.Builder.Builder.Processes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Builder.Extensions.Configuration
{
    public class BuilderConfiguration
    {
        public DatabaseType DatabaseType { get; set; }
        public Func<IServiceProvider, IDbConnection> DbConnectionFactory { get; set; }
    }
}