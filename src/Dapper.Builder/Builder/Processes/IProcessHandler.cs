using Dapper.Builder.Services.DAL.Builder;

namespace Dapper.Builder.Builder.Processes.Configuration
{
    /// <summary>
    /// Process Handling Service that runs the pipes and processes
    /// </summary>
    public interface IProcessHandler : IProcessConfig
    {
        /// <summary>
        /// Goes through all the pipes with the query builder
        /// </summary>
        void PipeThrough<TEntity>(IQueryBuilder<TEntity> queryBuilder) where TEntity : new();

        /// <summary>
        /// Goes through all the update processes and puts the data on the entity
        /// </summary>
        TEntity RunThroughProcessesForUpdate<TEntity>(TEntity entity) where TEntity : new();

        /// <summary>
        /// Goes through all the insert processes and puts the data on the entity
        /// </summary>
        TEntity RunThroughProcessesForInsert<TEntity>(TEntity entity) where TEntity : new();
    }
}