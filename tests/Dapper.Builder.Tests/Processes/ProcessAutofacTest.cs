using Autofac;
using BR.POCO.DB;
using Dapper.Builder.Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Dapper.Builder.Tests.Processes
{
    [TestClass]
    public class ProcessAutofacTest
    {
        private IContainer container;
        [TestMethod]
        public void ProcessesByListRegister()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new DapperBuilderModule(new AutofacBuilderConfiguration()
            {
                DatabaseType = DatabaseType.SQL,
                DbConnectionFactory = (ser) => new SqlConnection("server=(local)"),
                InsertProcesses = new List<Type> { typeof(AlmondProcess) },
                SelectPipes = new List<Type> { typeof(AlmondProcess) },
                UpdateProcesses = new List<Type> { typeof(AlmondProcess) },
            }));
            container = containerBuilder.Build();
            var qbuilder = container.Resolve<IQueryBuilder<UserMock>>();

            qbuilder.GetQueryString();

            qbuilder.GetInsertString(new UserMock());

            Assert.IsTrue(AlmondProcess.AlmondPipeActive);

            Assert.IsTrue(AlmondProcess.AlmondsActive);
        }

        [TestMethod]
        public void ProcessesByAssemblyScanningRegister()
        {

            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new DapperBuilderModule(new AutofacBuilderConfiguration()
            {
                DatabaseType = DatabaseType.SQL,
                DbConnectionFactory = (ser) => new SqlConnection("server=(local)"),
                ProcessScanAssembly = typeof(AlmondProcess).Assembly
            }));

            container = containerBuilder.Build();
            var qbuilder = container.Resolve<IQueryBuilder<UserMock>>();

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

}
