using System;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Dapper.Builder.Services;
using Dapper.Builder.Processes;

namespace Dapper.Builder.Core
{
    /// <summary>
    /// Core implementation for dependencies aggregations
    /// </summary>
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

        /// <summary>
        /// Resolves the services using .net core
        /// </summary>
        /// <typeparam name="TService">The service that needs to be resolved</typeparam>
        public TService ResolveService<TService>()
        {
            return ServiceProvider.Value.GetRequiredService<TService>();
        }
    }
}
