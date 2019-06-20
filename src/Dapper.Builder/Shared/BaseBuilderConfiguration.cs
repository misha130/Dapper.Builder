using System;
using System.Collections.Generic;

namespace Dapper.Builder
{
    public class BaseBuilderConfiguration
    {
        /// <summary>
        /// The type of database so to create queries for them
        /// </summary>
        public DatabaseType DatabaseType { get; set; }

        public System.Reflection.Assembly ProcessScanAssembly { get; set; }

        public IEnumerable<Type> SelectPipes { get; set; }

        public IEnumerable<Type> UpdateProcesses { get; set; }

        public IEnumerable<Type> InsertProcesses { get; set; }
    }
}
