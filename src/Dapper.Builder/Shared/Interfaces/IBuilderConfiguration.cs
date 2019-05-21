using Dapper.Builder.Extensions;

namespace Dapper.Builder.Shared.Interfaces
{
    public interface IBuilderConfiguration
    {
        DatabaseType DatabaseType { get; }
    }
}
