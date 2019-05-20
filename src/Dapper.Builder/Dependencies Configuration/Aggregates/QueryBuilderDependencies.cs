using Dapper.Builder.Builder;
using Dapper.Builder.Builder.NamingStrategyService;
using Dapper.Builder.Builder.Processes.Configuration;
using Dapper.Builder.Extensions.Configuration;
using Dapper.Builder.Services.DAL.Builder.FilterParser;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;
using Dapper.Builder.Services.DAL.Builder.PropertyParser;
using Dapper.Builder.Services.DAL.Builder.SortHandler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Dapper.Builder.Dependencies_Configuration.Aggregates
{
    public class QueryBuilderDependencies<T> : IQueryBuilderDependencies<T> where T : new()
    {
        public Lazy<IPropertyParser> PropertyParser { get; set; }

        public Lazy<ISortHandler> SortHandler { get; set; }

        public Lazy<IJoinHandler> JoinHandler { get; set; }

        public IFilterParser<T> FilterParser { get; set; }

        public INamingStrategyService NamingStrategy { get; set; }

        public IDbConnection Context { get; set; }

        public Lazy<IServiceProvider> ServiceProvider { get; set; }

        public IProcessHandler ProcessHandler { get; set; }

        public BuilderConfiguration Config { get; set; }
        public QueryBuilderDependencies(
            BuilderConfiguration config,
            IProcessHandler processHandler,
            Lazy<IServiceProvider> serviceProvider,
            IDbConnection context,
            INamingStrategyService namingStrategyService,
            IFilterParser<T> filterParser,
            Lazy<IJoinHandler> joinHandler,
            Lazy<ISortHandler> sortHandler,
            Lazy<IPropertyParser> propertyParser
            )
        {
            Config = config;
            ProcessHandler = processHandler;
            ServiceProvider = serviceProvider;
            Context = context;
            NamingStrategy = namingStrategyService;
            FilterParser = filterParser;
            JoinHandler = joinHandler;
            SortHandler = sortHandler;
            PropertyParser = propertyParser;
        }
    }
}
