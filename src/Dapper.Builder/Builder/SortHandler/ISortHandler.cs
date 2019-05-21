using Dapper.Builder.Builder.SortHandler;

namespace Dapper.Builder.Services.DAL.Builder.SortHandler
{
    /// <summary>
    /// Services that creates order by queries
    /// </summary>
    public interface ISortHandler
    {
        /// <summary>
        /// Parses SortColumn into query strings of order by
        /// </summary>
        /// <param name="sort">Sort Data</param>
        /// <param name="alias">Alias for the column used</param>
        string Produce<TEntity>(SortColumn sort, string alias = null) where TEntity : new();

        // TODO: Create a sort not by TEntity but UEntity
    }
}
