using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper.Builder.Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;

namespace Dapper.Builder.Sample.Config
{
    public class AutofacConfig
    {
        public static IContainer Container;
        public static IContainer Build(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);
            RegisterTypes(builder);
            var container = builder.Build();
            return Container = container;
            // return the IServiceProvider implementation
        }

        private static void RegisterTypes(ContainerBuilder builder)
        {
            builder.RegisterModule(new DapperBuilderModule(new Autofac.Configuration.AutofacBuilderConfiguration
            {
                DatabaseType = DatabaseType.SQL,
                DbConnectionFactory = (c) => new SqlConnection("(local)")
            }));

        }
    }
}