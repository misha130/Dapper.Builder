namespace Dapper.Builder.Processes {
    /// <summary>
    /// Classes that implement this interface are added to the pipe collection
    /// </summary>
    public interface IPipe {
        /// <summary>
        /// Executes a pipe on the entity, which adds a condition query
        /// </summary>
        void Pipe<TEntity> (IQueryBuilder<TEntity> builder) where TEntity : new ();
    }
}