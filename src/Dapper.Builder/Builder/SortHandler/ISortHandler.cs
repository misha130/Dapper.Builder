using Dapper.Builder.Builder.SortHandler;

namespace Dapper.Builder.Services.DAL.Builder.SortHandler
{
    public interface ISortHandler
    {
        string Produce<T>(SortColumn sort, string alias = null);
    }
}
