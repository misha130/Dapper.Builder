
using Dapper.Builder.Processes;
using Dapper.Builder.Services;
using System;
using System.Data;

namespace Dapper.Builder
{
    public class QueryBuilderDependencies<T> where T : new()
    {
        public Lazy<IPropertyParser> PropertyParser { get; set; }

        public Lazy<ISortHandler> SortHandler { get; set; }

        public Lazy<IJoinHandler> JoinHandler { get; set; }

        public IFilterParser<T> FilterParser { get; set; }

        public INamingStrategyService NamingStrategy { get; set; }

        public IDbConnection Context { get; set; }
        public IProcessHandler ProcessHandler { get; set; }

        public QueryBuilderDependencies(
            IProcessHandler processHandler,
            IDbConnection context,
            INamingStrategyService namingStrategyService,
            IFilterParser<T> filterParser,
            Lazy<IJoinHandler> joinHandler,
            Lazy<ISortHandler> sortHandler,
            Lazy<IPropertyParser> propertyParser
            )
        {
            ProcessHandler = processHandler;
            Context = context;
            NamingStrategy = namingStrategyService;
            FilterParser = filterParser;
            JoinHandler = joinHandler;
            SortHandler = sortHandler;
            PropertyParser = propertyParser;
        }
    }
}
