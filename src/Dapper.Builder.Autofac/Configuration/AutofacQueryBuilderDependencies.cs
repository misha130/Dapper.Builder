using System;
using System.Data;
using Autofac;
using Dapper.Builder.Builder;
using Dapper.Builder.Builder.NamingStrategyService;
using Dapper.Builder.Builder.Processes.Configuration;
using Dapper.Builder.Services.DAL.Builder.FilterParser;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;
using Dapper.Builder.Services.DAL.Builder.PropertyParser;
using Dapper.Builder.Services.DAL.Builder.SortHandler;
using Dapper.Builder.Shared;

namespace Dapper.Builder.Dependencies_Configuration.Aggregates {
    /// <summary>
    /// Autofac implementation for dependencies aggregations
    /// </summary>
    public class AutofacQueryBuilderDependencies<T> : QueryBuilderDependencies<T>, IQueryBuilderDependencies<T> where T : new () {
       
        private Lazy<ILifetimeScope> Scope { get; set; }
      
        public AutofacQueryBuilderDependencies (
            IProcessHandler processHandler,
            IDbConnection context,
            Lazy<ILifetimeScope> scope,
            INamingStrategyService namingStrategyService,
            IFilterParser<T> filterParser,
            Lazy<IJoinHandler> joinHandler,
            Lazy<ISortHandler> sortHandler,
            Lazy<IPropertyParser> propertyParser
        ) : base (
            processHandler,
            context,
            namingStrategyService,
            filterParser,
            joinHandler,
            sortHandler,
            propertyParser) {
            Scope = scope;
        }
        /// <summary>
        /// Resolves the services using Autofac
        /// </summary>
        /// <typeparam name="TService">The service that needs to be resolved</typeparam>
        public TService ResolveService<TService> () {
            return Scope.Value.Resolve<TService> ();
        }
    }
}
