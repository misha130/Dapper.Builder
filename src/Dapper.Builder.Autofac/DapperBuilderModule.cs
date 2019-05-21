using Autofac;
using Dapper.Builder.Autofac.Configuration;
using Dapper.Builder.Builder;
using Dapper.Builder.Builder.NamingStrategyService;
using Dapper.Builder.Builder.Processes.Configuration;
using Dapper.Builder.Dependencies_Configuration.Aggregates;
using Dapper.Builder.Extensions;
using Dapper.Builder.Services.DAL.Builder;
using Dapper.Builder.Services.DAL.Builder.FilterParser;
using Dapper.Builder.Services.DAL.Builder.JoinHandler;
using Dapper.Builder.Services.DAL.Builder.PropertyParser;
using Dapper.Builder.Services.DAL.Builder.SortHandler;
using Dapper.Builder.Shared.Interfaces;

namespace Dapper.Builder.Autofac
{
    /// <summary>
    /// Dapper module to be consumed by autofac
    /// </summary>
    public class DapperBuilderModule : Module
    {
        private readonly AutofacBuilderConfiguration _configuration;
        public DapperBuilderModule(AutofacBuilderConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override void Load(ContainerBuilder builder)
        {
            #region Sub services
            builder.Register(_configuration.DbConnectionFactory).InstancePerLifetimeScope();
            switch (_configuration?.DatabaseType)
            {
                case DatabaseType.SQL:
                default:
                    builder.RegisterType(typeof(SqlQueryBuilder<>)).As(typeof(IQueryBuilder<>)).InstancePerDependency();
                    builder.RegisterType(typeof(FilterParser<>)).As(typeof(IFilterParser<>)).InstancePerDependency();
                    break;
                case DatabaseType.PostgreSql:
                    builder.RegisterType(typeof(PostgreQueryBuilder<>)).As(typeof(IQueryBuilder<>)).InstancePerDependency();
                    builder.RegisterType(typeof(PostgreFilterParser<>)).As(typeof(IFilterParser<>)).InstancePerDependency();
                    break;
                case DatabaseType.Snowflake:
                    builder.RegisterType(typeof(SnowflakeQueryBuilder<>)).As(typeof(IQueryBuilder<>)).InstancePerDependency();
                    builder.RegisterType(typeof(FilterParser<>)).As(typeof(IFilterParser<>)).InstancePerDependency();
                    break;
            }
            builder.RegisterType<JoinHandler>().As<IJoinHandler>().InstancePerLifetimeScope();
            builder.RegisterType<PropertyParser>().As<IPropertyParser>().InstancePerLifetimeScope();
            builder.RegisterType<SortHandler>().As<ISortHandler>().InstancePerLifetimeScope();
            builder.RegisterType<NamingStrategyService>().As<INamingStrategyService>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessHandler>().As<IProcessHandler>().InstancePerLifetimeScope();
            #endregion
            #region Aggregation
            builder.RegisterType(typeof(AutofacQueryBuilderDependencies<>)).As(typeof(IQueryBuilderDependencies<>));
            #endregion

            builder.Register((c) => _configuration ?? new AutofacBuilderConfiguration()).As<IBuilderConfiguration>();

        }
    }
}
