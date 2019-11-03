using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapper.Builder.Tests.Services
{
    [TestClass]
    public class QueryBuilderTests : BaseTest
    {

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
