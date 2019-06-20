using System;
using System.Collections.Generic;

namespace Dapper.Builder
{
    public interface IBuilderConfiguration
    {
        DatabaseType DatabaseType { get; }

        System.Reflection.Assembly ProcessScanAssembly { get; set; }

        IEnumerable<Type> SelectPipes { get; set; }

        IEnumerable<Type> UpdateProcesses { get; set; }

        IEnumerable<Type> InsertProcesses { get; set; }
    }
}
