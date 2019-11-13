using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Autofac;
using Dapper.Builder.Autofac;
using Microsoft.Data.SqlClient;

namespace Dapper.Builder.Tests.Services
{
    [TestClass]
    public class AggregateServices : BaseTest
    {
        
        [TestInitialize]
        public  void Init()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new DapperBuilderModule(new AutofacBuilderConfiguration
            {
                DatabaseType = DatabaseType.SQL,
                DbConnectionFactory = (ser) => new SqlConnection("server=(local)")
            }));
            Container = containerBuilder.Build();
        }
        /// <summary>
        /// Tests whether aggregate services gets all the depenedencies
        /// </summary>
        [TestMethod]
        public void AllServices()
        {
            var dependencies = Resolve<IQueryBuilderDependencies<UserMock>>();
            var props = dependencies.GetType().GetProperties();
            Assert.IsTrue(props.All(prop => prop.GetValue(dependencies) != null));
        }
    }
}
