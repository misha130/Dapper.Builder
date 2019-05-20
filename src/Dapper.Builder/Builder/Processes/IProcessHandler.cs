using Dapper.Builder.Services.DAL.Builder;

namespace Dapper.Builder.Builder.Processes.Configuration
{
    public interface IProcessHandler : IProcessConfig
    {
        void PipeThrough<T>(IQueryBuilder<T> queryBuilder) where T : new();
        T RunThroughProcessesForUpdate<T>(T entity) where T : new();
        T RunThroughProcessesForInsert<T>(T entity) where T : new();
    }
}