using Dapper.Builder.Builder.NamingStrategyService;
using Dapper.Builder.Builder.Processes.Configuration;
using Dapper.Builder.Services.DAL.Builder.FilterParser;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;
using Dapper.Builder.Services.DAL.Builder.PropertyParser;
using Dapper.Builder.Services.DAL.Builder.SortHandler;
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
