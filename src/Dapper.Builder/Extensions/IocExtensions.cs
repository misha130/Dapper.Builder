using Dapper.Builder.Processes;
using System;
using System.Collections.Generic;

namespace Dapper.Builder.Extensions
{
    public static class IocExtensions
    {
        public
            static BuilderProcessesAndPipes
            GetProcessAndPipes(this IBuilderConfiguration configuration)
        {
            var processAndPipes = new BuilderProcessesAndPipes
            {
                InsertProcesses = configuration.InsertProcesses.AsList() ?? new List<Type>(),
                UpdateProcesses = configuration.UpdateProcesses.AsList() ?? new List<Type>(),
                SelectPipes = configuration.SelectPipes.AsList() ?? new List<Type>()
            };
            if (configuration.ProcessScanAssembly != null)
            {
                var types = configuration.ProcessScanAssembly
                .GetTypes();
                foreach (var type in types)
                {
                    if (type.IsInterface)
                    {
                        continue;
                    }
                    if (typeof(ISelectPipe).IsAssignableFrom(type))
                    {
                        processAndPipes.SelectPipes.Add(type);
                    }
                    if (typeof(IUpdateProcess).IsAssignableFrom(type))
                    {
                        processAndPipes.UpdateProcesses.Add(type);
                    }
                    if (typeof(IInsertProcess).IsAssignableFrom(type))
                    {
                        processAndPipes.InsertProcesses.Add(type);
                    }
                }
            }
            return processAndPipes;
        }
    }

    public class BuilderProcessesAndPipes
    {
        public List<Type> SelectPipes { get; set; }

        public List<Type> UpdateProcesses { get; set; }

        public List<Type> InsertProcesses { get; set; }
    }
}
