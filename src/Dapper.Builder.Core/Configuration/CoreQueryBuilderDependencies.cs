using Dapper.Builder.Builder.NamingStrategyService;
using Dapper.Builder.Builder.Processes.Configuration;
using Dapper.Builder.Services.DAL.Builder.FilterParser;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;
using Dapper.Builder.Services.DAL.Builder.PropertyParser;
using Dapper.Builder.Services.DAL.Builder.SortHandler;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Dapper.Builder.Shared;

namespace Dapper.Builder.Dependencies_Configuration.Aggregates
{
    public class CoreQueryBuilderDependencies<T> : QueryBuilderDependencies<T>, IQueryBuilderDependencies<T> where T : new()
    {
        private Lazy<IServiceProvider> ServiceProvider { get; set; }



        public CoreQueryBuilderDependencies(
            IProcessHandler processHandler,
            IDbConnection context,
            Lazy<IServiceProvider> serviceProvider,
            INamingStrategyService namingStrategyService,
            IFilterParser<T> filterParser,
            Lazy<IJoinHandler> joinHandler,
            Lazy<ISortHandler> sortHandler,
            Lazy<IPropertyParser> propertyParser
            ) : base(
                processHandler,
                context,
                namingStrategyService,
                filterParser,
                joinHandler,
                sortHandler,
                propertyParser)
        {
            ServiceProvider = serviceProvider;
        }

        public TService ResolveService<TService>()
        {
            return ServiceProvider.Value.GetRequiredService<TService>();
        }
    }
}
