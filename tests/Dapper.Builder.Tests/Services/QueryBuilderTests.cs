using BR.POCO.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dapper.Builder.Tests.Services
{
    [TestClass]
    public class QueryBuilderTests : BaseTest
    {
        private IQueryBuilder<UserMock> queryBuilder => Resolve<IQueryBuilder<UserMock>>();

        [TestMethod]
        public void AllQuery()
        {
            var queryString = queryBuilder.GetQueryString();
            Assert.AreEqual("SELECT * FROM [Users]".Trim(), queryString.Query.Trim());
        }


        [TestMethod]
        public void AllQuerySingleColumnByExpression()
        {
            var queryString = queryBuilder.Columns(tc => tc.Id).GetQueryString();
            Assert.AreEqual("SELECT [Users].[Id] FROM [Users]".Trim().ToLower(), queryString.Query.Trim().ToLower());
        }

        [TestMethod]
        public void AllQuerySingleColumnByString()
        {
            var queryString = queryBuilder.Columns(nameof(UserMock.Id)).GetQueryString();
            Assert.AreEqual("SELECT [Users].[Id] FROM [Users]".Trim().ToLower(), queryString.Query.Trim().ToLower());
        }

        [TestMethod]
        public void QuerySingleColumnById()
        {
            var queryString = queryBuilder.Where(tc => tc.Id == 1).Columns(tc => tc.Id).GetQueryString();
            Assert.AreEqual(
                string.Compare("SELECT [Users].[Id] FROM [Users] WHERE ([Users].[Id] = @1)",
                queryString.Query,
                CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }

    }
}
