using BR.POCO.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

        [TestMethod]
        public void QueryToLower()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.ToLower() == "Misha").GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE ((LOWER([Users].[FirstName])) = @1)",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
        }

        [TestMethod]
        public void QueryToLowerInvariant()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.ToLowerInvariant() == "Misha").GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE ((LOWER([Users].[FirstName])) = @1)",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
        }

        [TestMethod]
        public void QueryToUpperInvariant()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.ToUpperInvariant() == "Misha").GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE ((UPPER([Users].[FirstName])) = @1)",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
        }

        [TestMethod]
        public void QueryToUpper()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.ToUpper() == "Misha").GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE ((UPPER([Users].[FirstName])) = @1)",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
        }

        [TestMethod]
        public void QueryStartsWith()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.StartsWith("M")).GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE ([Users].[FirstName] LIKE @1)",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
            Assert.AreEqual("M%", queryString.Parameters.Values.First());
        }

        [TestMethod]
        public void QueryEndsWith()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.EndsWith("M")).GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE ([Users].[FirstName] LIKE @1)",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
            Assert.AreEqual("%M", queryString.Parameters.Values.First());
        }

        [TestMethod]
        public void QueryContains()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.Contains("M")).GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE ([Users].[FirstName] LIKE @1)",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
            Assert.AreEqual("%M%", queryString.Parameters.Values.First());
        }

        [TestMethod]
        public void QueryListContains()
        {
            var ids = new List<long> { 1, 2, 3 };
            var queryString = queryBuilder.Where(a => ids.Contains(a.Id)).GetQueryString();
            Assert.AreEqual(
                          string.Compare("SELECT * FROM [Users] WHERE ([Users].[Id] IN (@1,@2,@3))",
                          queryString.Query,
                          CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                          , 0);
            foreach (var id in ids){
                Assert.AreEqual(id, queryString.Parameters[id.ToString()]);
            }
        }
    }
}
