using BR.POCO.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace Dapper.Builder.Tests.Services
{
    [TestClass]
    public class SqlQueryBuilderTests : BaseTest
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
        public void QueryWithEqualAlwaysTrue()
        {
            var queryString = queryBuilder.Where((c) => 1 == 1).GetQueryString();
            Assert.AreEqual(
              string.Compare("SELECT * FROM [Users] WHERE (@1 = 1)",
              queryString.Query,
              CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
              , 0);
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

        [TestMethod]
        public void QueryWhereIsNull()
        {
            var queryString = queryBuilder.Columns(tc => tc.Id).Where(tc => tc.Email == null).GetQueryString();
            Assert.AreEqual(
                           string.Compare("SELECT [Users].[Id] FROM [Users] WHERE ([Users].[Email] is null)",
                           queryString.Query,
                           CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                           , 0);
        }


        [TestMethod]
        public void QueryArrayIndexWhere()
        {
            var sampleText = "something_something".Split('_');
            var queryString = queryBuilder.Where(a => a.FirstName == sampleText[0] && a.LastName == sampleText[1]).GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE (([Users].[FirstName]) = @1 and ([Users].[LastName] = @2))",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
        }
    }
}
