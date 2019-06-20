using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.Builder.Processes
{
    public class ProcessHandler : IProcessConfig, IProcessHandler
    {
        private List<Type> _excludedTypes = new List<Type>();

        private readonly IEnumerable<IUpdateProcess> _updateProcesses;
        private readonly IEnumerable<IInsertProcess> _insertProcesses;
        private readonly IEnumerable<ISelectPipe> _selectPipes;

        public ProcessHandler(
            IEnumerable<ISelectPipe> selectPipes = null,
            IEnumerable<IInsertProcess> insertProcesses = null,
            IEnumerable<IUpdateProcess> updateProcesses = null
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
            foreach (var pipe in _selectPipes)
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
            foreach (var process in _updateProcesses)
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
            foreach (var process in _insertProcesses)
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