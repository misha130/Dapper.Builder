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

            services.AddScoped<IDbConnection>(configuration.DbConnectionFactory);
            switch (configuration?.DatabaseType)
            {
                case DatabaseType.SQL:
                default:
                    services.AddTransient(typeof(IQueryBuilder<>), typeof(SqlQueryBuilder<>));
                    services.AddScoped(typeof(IFilterParser<>), typeof(FilterParser<>));
                    break;
                case DatabaseType.SQLite:
                    services.AddTransient(typeof(IQueryBuilder<>), typeof(SqliteQueryBuilder<>));
                    services.AddScoped(typeof(IFilterParser<>), typeof(FilterParser<>));
                    break;
                case DatabaseType.PostgreSql:
                    services.AddTransient(typeof(IQueryBuilder<>), typeof(PostgreQueryBuilder<>));
                    services.AddScoped(typeof(IFilterParser<>), typeof(PostgreFilterParser<>));
                    break;
                case DatabaseType.Snowflake:
                    services.AddTransient(typeof(IQueryBuilder<>), typeof(SnowflakeQueryBuilder<>));
                    services.AddScoped(typeof(IFilterParser<>), typeof(FilterParser<>));
                    break;
            }

            services.AddScoped(typeof(IJoinHandler), typeof(JoinHandler));
            services.AddScoped(typeof(IPropertyParser), typeof(PropertyParser));
            services.AddScoped(typeof(ISortHandler), typeof(SortHandler));
            services.AddScoped(typeof(INamingStrategyService), typeof(NamingStrategyService));
            services.AddScoped(typeof(IProcessHandler), typeof(ProcessHandler));

            #endregion

            #region Aggregation

            services.AddScoped(typeof(Lazy<IJoinHandler>));
            services.AddScoped(typeof(Lazy<IServiceProvider>));
            services.AddScoped(typeof(Lazy<ISortHandler>));
            services.AddScoped(typeof(Lazy<IPropertyParser>));
            services.AddScoped(typeof(IQueryBuilderDependencies<>), typeof(CoreQueryBuilderDependencies<>));

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

            services.AddScoped<IBuilderConfiguration>((c) => configuration ?? new CoreBuilderConfiguration());
        }
    }
}