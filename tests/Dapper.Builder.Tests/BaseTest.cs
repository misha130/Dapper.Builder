using Microsoft.AspNetCore;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper.Builder.Core;

namespace Dapper.Builder.Tests
{
    public class BaseTest
    {
        protected IServiceProvider coreServices =
          WebHost.CreateDefaultBuilder()
        .UseStartup<TestStartup>()
                .Build().Services;
        protected T Resolve<T>()
        {
            return coreServices.GetService<T>();
        }


    }

    public class TestStartup
    {
        public TestStartup(IConfiguration configuration, IHostingEnvironment environment)
        {
        }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDapperBuilder(new CoreBuilderConfiguration
            {
                DatabaseType = DatabaseType.SQL,
                DbConnectionFactory = (ser) => new SqlConnection("server=(local)")
            });


            return services.BuildServiceProvider();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        }

    }
}
