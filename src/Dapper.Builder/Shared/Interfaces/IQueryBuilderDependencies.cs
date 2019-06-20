using Dapper.Builder.Processes;
using Dapper.Builder.Services;
using System;
using System.Data;

namespace Dapper.Builder.Dependencies_Configuration.Aggregates
{
    public interface IQueryBuilderDependencies<T> where T : new()
    {
        Lazy<IPropertyParser> PropertyParser { get; }
        Lazy<ISortHandler> SortHandler { get; }
        Lazy<IJoinHandler> JoinHandler { get; }
        IFilterParser<T> FilterParser { get; }
        INamingStrategyService NamingStrategy { get; }
        IDbConnection Context { get; }
        IProcessHandler ProcessHandler { get; }
        TService ResolveService<TService>();
    }
}
