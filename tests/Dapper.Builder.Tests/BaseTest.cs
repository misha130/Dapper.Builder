using Microsoft.AspNetCore;
using System;
using Dapper.Builder.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Dapper.Builder.Extensions.Configuration;
using System.Data.SqlClient;

namespace Dapper.Builder.Tests
{
    public class BaseTest
    {
        protected IServiceProvider services =
          WebHost.CreateDefaultBuilder()
        .UseStartup<TestStartup>()
                .Build().Services;
        protected T Resolve<T>()
        {
            return services.GetService<T>();
        }
    }

    public class TestStartup
    {
        public TestStartup(IConfiguration configuration, IHostingEnvironment environment)
        {
        }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDapperBuilder(new BuilderConfiguration
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
