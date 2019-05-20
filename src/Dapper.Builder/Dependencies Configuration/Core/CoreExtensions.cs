using Dapper.Builder.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dapper.Builder.Services.DAL.Builder;
using Dapper.Builder.Services.DAL.Builder.FilterParser;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;
using Dapper.Builder.Services.DAL.Builder.PropertyParser;
using Dapper.Builder.Services.DAL.Builder.SortHandler;
using Dapper.Builder.Builder.NamingStrategyService;
using Dapper.Builder.Builder;
using Dapper.Builder.Dependencies_Configuration.Aggregates;
using Dapper.Builder.Builder.Processes.Configuration;
using System.Data;
using System.Data.SqlClient;
using System;

namespace Dapper.Builder.Extensions
{
    public static class CoreExtensions
    {
        public static void AddDapperBuilder(
            this IServiceCollection services,
            BuilderConfiguration configuration)
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
            services.AddScoped((c) => configuration ?? new BuilderConfiguration());
            services.AddScoped(typeof(Lazy<IJoinHandler>));
            services.AddScoped(typeof(Lazy<IServiceProvider>));
            services.AddScoped(typeof(Lazy<ISortHandler>));
            services.AddScoped(typeof(Lazy<IPropertyParser>));
            services.AddScoped(typeof(IQueryBuilderDependencies<>), typeof(QueryBuilderDependencies<>));
            #endregion
        }
    }
}