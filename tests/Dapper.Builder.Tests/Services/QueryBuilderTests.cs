using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Builder.Tests.Services
{
    [TestClass]
    public class QueryBuilderTests : BaseTest
    {

        private IQueryBuilder<UserMock> queryBuilder => Resolve<IQueryBuilder<UserMock>>();

        [TestMethod]
        public void Clone()
        {
            var qb = queryBuilder.CloneInstance();
            qb.Columns(d => d.FirstName).Where(r => r.Email == "hi");
            var clonedQb = qb.CloneInstance();
            var query = clonedQb.GetQueryString();
            Assert.AreEqual(query.Query.Trim().ToLower(),
                "SELECT [Users].[FirstName] FROM [Users] WHERE ([Users].[Email] = 'hi'".Trim().ToLower());
        }
    }
}
