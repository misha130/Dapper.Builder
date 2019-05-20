using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Builder.Builder.Processes.Interfaces;
using Dapper.Builder.Services.DAL.Builder;

namespace Dapper.Builder.Builder.Processes.Configuration
{
    public class ProcessHandler : IProcessConfig, IProcessHandler
    {
        private List<Type> _excludedTypes = new List<Type>();

        private readonly Lazy<IEnumerable<IUpdateProcess>> _updateProcesses;
        private readonly Lazy<IEnumerable<IInsertProcess>> _insertProcesses;
        private readonly Lazy<IEnumerable<ISelectPipe>> _selectPipes;

        public ProcessHandler(
            Lazy<IEnumerable<ISelectPipe>> selectPipes = null,
            Lazy<IEnumerable<IInsertProcess>> insertProcesses = null,
            Lazy<IEnumerable<IUpdateProcess>> updateProcesses = null
            )
        {
            _selectPipes = selectPipes;
            _insertProcesses = insertProcesses;
            _updateProcesses = updateProcesses;
        }


        private bool IsExcluded(Type type)
        {
            return _excludedTypes.Any(ex => ex == type);
        }

        public void Exclude(params Type[] type)
        {
            _excludedTypes.AddRange(type);
        }

        public void PipeThrough<T>(IQueryBuilder<T> queryBuilder) where T : new()
        {
            if (_selectPipes == null) return;
            foreach (var pipe in _selectPipes.Value)
            {
                if (!IsExcluded(pipe.GetType()))
                {
                    pipe.Pipe(queryBuilder);
                }
            }
        }

        public T RunThroughProcessesForUpdate<T>(T entity) where T : new()
        {
            if (_updateProcesses == null) return entity;
            foreach (var process in _updateProcesses.Value)
            {
                if (!IsExcluded(process.GetType()))
                {
                    process.Process(entity);
                }
            }
            return entity;
        }
        public T RunThroughProcessesForInsert<T>(T entity) where T : new()
        {
            if (_insertProcesses == null) return entity;
            foreach (var process in _insertProcesses.Value)
            {
                if (!IsExcluded(process.GetType()))
                {
                    process.Process(entity);
                }
            }
            return entity;
        }
    }
}