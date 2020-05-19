using Microsoft.Extensions.DependencyInjection;
using Dapper.Builder.Services;
using Dapper.Builder.Processes;
using Dapper.Builder.Extensions;
using System.Data;
using System;

namespace Dapper.Builder.Core
{
    /// <summary>
    /// Adds dapper builder support to core DI
    /// </summary>
    public static class CoreExtensions
    {
        /// <summary>
        /// Sets up dapper builder for asp core DI
        /// </summary>
        /// <param name="services">The currently used service collection in Startup</param>
        /// <param name="configuration">Configuration to be used in the setup</param>
        public static void AddDapperBuilder(
            this IServiceCollection services,
            CoreBuilderConfiguration configuration)
        {
            #region Sub services

            services.AddTransient<IDbConnection>(configuration.DbConnectionFactory);
            switch (configuration?.DatabaseType)
            {
                case DatabaseType.SQL:
                default:
                    services.AddTransient(typeof(IQueryBuilder<>), typeof(SqlQueryBuilder<>));
                    services.AddTransient(typeof(IFilterParser<>), typeof(FilterParser<>));
                    services.AddTransient(typeof(INamingStrategyService), typeof(NamingStrategyService));
                    break;
                case DatabaseType.SQLite:
                    services.AddTransient(typeof(IQueryBuilder<>), typeof(SqliteQueryBuilder<>));
                    services.AddTransient(typeof(IFilterParser<>), typeof(FilterParser<>));
                    services.AddTransient(typeof(INamingStrategyService), typeof(NamingStrategyService));
                    break;
                case DatabaseType.PostgreSql:
                    services.AddTransient(typeof(IQueryBuilder<>), typeof(PostgreQueryBuilder<>));
                    services.AddTransient(typeof(IFilterParser<>), typeof(PostgreFilterParser<>));
                    services.AddTransient(typeof(INamingStrategyService), typeof(PostgreNamingStrategyService));
                    break;
                case DatabaseType.Snowflake:
                    services.AddTransient(typeof(IQueryBuilder<>), typeof(SnowflakeQueryBuilder<>));
                    services.AddTransient(typeof(IFilterParser<>), typeof(FilterParser<>));
                    services.AddTransient(typeof(INamingStrategyService), typeof(NamingStrategyService));
                    break;
            }

            services.AddTransient(typeof(IJoinHandler), typeof(JoinHandler));
            services.AddTransient(typeof(IPropertyParser), typeof(PropertyParser));
            services.AddTransient(typeof(ISortHandler), typeof(SortHandler));
            services.AddTransient(typeof(IProcessHandler), typeof(ProcessHandler));

            #endregion

            #region Aggregation

            services.AddTransient(typeof(Lazy<IJoinHandler>));
            services.AddTransient(typeof(Lazy<IServiceProvider>));
            services.AddTransient(typeof(Lazy<ISortHandler>));
            services.AddTransient(typeof(Lazy<IPropertyParser>));
            services.AddTransient(typeof(IQueryBuilderDependencies<>), typeof(CoreQueryBuilderDependencies<>));

            #endregion

            #region Processes

            var proccesAndPipes = configuration.GetProcessAndPipes();
            foreach (var selectPipe in proccesAndPipes.SelectPipes)
            {
                services.AddTransient(typeof(ISelectPipe), selectPipe);
            }

            foreach (var insertProcess in proccesAndPipes.InsertProcesses)
            {
                services.AddTransient(typeof(IInsertProcess), insertProcess);
            }

            foreach (var updateProcess in proccesAndPipes.UpdateProcesses)
            {
                services.AddTransient(typeof(IUpdateProcess), updateProcess);
            }

            #endregion

            services.AddTransient<IBuilderConfiguration>((c) => configuration ?? new CoreBuilderConfiguration());
        }
    }
}