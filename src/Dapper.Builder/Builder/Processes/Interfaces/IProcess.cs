namespace Dapper.Builder.Builder.Processes.Interfaces
{
    /// <summary>
    /// Interface IProcess
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// Processes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        void Process<TEntity>(TEntity entity) where TEntity : new();
    }
}
