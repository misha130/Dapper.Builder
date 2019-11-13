using Autofac;
using Dapper.Builder.Autofac;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapper.Builder.Tests.Services
{
    [TestClass]
    public class QueryBuilderTests : BaseTest
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
        private IQueryBuilder<UserMock> queryBuilder => Resolve<IQueryBuilder<UserMock>>();

        [TestMethod, Ignore]
        public void Clone()
        {
            var qb = queryBuilder.CloneInstance();
            qb.Columns(d => d.FirstName).Where(r => r.Email == "hi");
            var clonedQb = qb.CloneInstance();
            Assert.IsFalse(ReferenceEquals(qb, clonedQb));
        }
    }
}
