﻿using Autofac;
using Dapper.Builder.Extensions;
using Dapper.Builder.Processes;
using Dapper.Builder.Services;

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
            #region Internal Services

            builder.Register(_configuration.DbConnectionFactory).InstancePerLifetimeScope();

            switch (_configuration?.DatabaseType)
            {
                case DatabaseType.SQL:
                default:
                    builder.RegisterGeneric(typeof(SqlQueryBuilder<>)).As(typeof(IQueryBuilder<>))
                        .InstancePerDependency();
                    builder.RegisterGeneric(typeof(FilterParser<>)).As(typeof(IFilterParser<>)).InstancePerDependency();
                    builder.RegisterType<NamingStrategyService>().As<INamingStrategyService>().InstancePerLifetimeScope();
                    break;
                case DatabaseType.PostgreSql:
                    builder.RegisterGeneric(typeof(PostgreQueryBuilder<>)).As(typeof(IQueryBuilder<>))
                        .InstancePerDependency();
                    builder.RegisterGeneric(typeof(PostgreFilterParser<>)).As(typeof(IFilterParser<>))
                        .InstancePerDependency();
                    builder.RegisterType<PostgreNamingStrategyService>().As<INamingStrategyService>().InstancePerLifetimeScope();
                    break;
                case DatabaseType.SQLite:
                    builder.RegisterGeneric(typeof(SqliteQueryBuilder<>)).As(typeof(IQueryBuilder<>))
                        .InstancePerDependency();
                    builder.RegisterGeneric(typeof(FilterParser<>)).As(typeof(IFilterParser<>))
                        .InstancePerDependency();
                    builder.RegisterType<NamingStrategyService>().As<INamingStrategyService>().InstancePerLifetimeScope();
                    break;
                case DatabaseType.Snowflake:
                    builder.RegisterGeneric(typeof(SnowflakeQueryBuilder<>)).As(typeof(IQueryBuilder<>))
                        .InstancePerDependency();
                    builder.RegisterGeneric(typeof(FilterParser<>)).As(typeof(IFilterParser<>)).InstancePerDependency();
                    builder.RegisterType<NamingStrategyService>().As<INamingStrategyService>().InstancePerLifetimeScope();
                    break;
            }

            builder.RegisterType<JoinHandler>().As<IJoinHandler>().InstancePerLifetimeScope();
            builder.RegisterType<PropertyParser>().As<IPropertyParser>().InstancePerLifetimeScope();
            builder.RegisterType<SortHandler>().As<ISortHandler>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessHandler>().As<IProcessHandler>().InstancePerLifetimeScope();

            #endregion

            #region Aggregation

            builder.RegisterGeneric(typeof(AutofacQueryBuilderDependencies<>)).As(typeof(IQueryBuilderDependencies<>));

            #endregion

            #region Processes

            var processAndPipes = _configuration.GetProcessAndPipes();
            foreach (var selectPipe in processAndPipes.SelectPipes)
            {
                builder.RegisterType(selectPipe).As<ISelectPipe>();
            }

            foreach (var insertProcess in processAndPipes.InsertProcesses)
            {
                builder.RegisterType(insertProcess).As<IInsertProcess>();
            }

            foreach (var updateProcess in processAndPipes.UpdateProcesses)
            {
                builder.RegisterType(updateProcess).As<IUpdateProcess>();
            }

            #endregion

            builder.Register((c) =>
                _configuration ?? new AutofacBuilderConfiguration()
            ).As<IBuilderConfiguration>();
        }
    }
}