
namespace Dapper.Builder
{
    public interface IInternalQueryBuilder
    {
        int GetParamCount();

        void ParamCount(int count);
    }
}
