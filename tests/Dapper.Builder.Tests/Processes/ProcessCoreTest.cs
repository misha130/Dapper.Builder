using BR.POCO.DB;
using Dapper.Builder.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Dapper.Builder.Tests.Processes
{
    [TestClass]
    public class ProcessCoreTest
    {
        private IServiceProvider coreServices;
        [TestMethod]
        public void ProcessesByListRegister()
        {
            coreServices = WebHost.CreateDefaultBuilder()
             .UseStartup<TestProcessStartupByList>()
            .Build().Services;

            var qbuilder = coreServices.GetService<IQueryBuilder<UserMock>>();

            qbuilder.GetQueryString();

            qbuilder.GetInsertString(new UserMock());

            Assert.IsTrue(AlmondProcess.AlmondPipeActive);

            Assert.IsTrue(AlmondProcess.AlmondsActive);
        }

        [TestMethod]
        public void ProcessesByAssemblyScanningRegister()
        {
            coreServices = WebHost.CreateDefaultBuilder()
             .UseStartup<TestProcessStartupByAssembly>()
            .Build().Services;

            var qbuilder = coreServices.GetService<IQueryBuilder<UserMock>>();

            qbuilder.GetQueryString();

            qbuilder.GetInsertString(new UserMock());

            Assert.IsTrue(AlmondProcess.AlmondPipeActive);

            Assert.IsTrue(AlmondProcess.AlmondsActive);
        }

        [TestCleanup]
        public void Clean()
        {
            AlmondProcess.AlmondPipeActive = AlmondProcess.AlmondsActive = false;
        }
    }
    public class TestProcessStartupByAssembly
    {
        public TestProcessStartupByAssembly(IConfiguration configuration, IHostingEnvironment environment)
        {
        }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDapperBuilder(new CoreBuilderConfiguration
            {
                DatabaseType = DatabaseType.SQL,
                DbConnectionFactory = (ser) => new SqlConnection("server=(local)"),
                ProcessScanAssembly = typeof(AlmondProcess).Assembly
            });

            return services.BuildServiceProvider();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        }

    }


    public class TestProcessStartupByList
    {
        public TestProcessStartupByList(IConfiguration configuration, IHostingEnvironment environment)
        {
        }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDapperBuilder(new CoreBuilderConfiguration
            {
                DatabaseType = DatabaseType.SQL,
                DbConnectionFactory = (ser) => new SqlConnection("server=(local)"),
                InsertProcesses = new List<Type> { typeof(AlmondProcess) },
                SelectPipes = new List<Type> { typeof(AlmondProcess) },
                UpdateProcesses = new List<Type> { typeof(AlmondProcess) },
            });

            return services.BuildServiceProvider();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        }

    }
}
