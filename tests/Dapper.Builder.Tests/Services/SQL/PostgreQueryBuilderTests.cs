﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Autofac;
using Dapper.Builder.Autofac;
using Dapper.Builder.Services;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace Dapper.Builder.Tests.Services
{
    [TestClass]
    public class PostgreQueryBuilderTests : BaseTest
    {
        private IQueryBuilder<UserMock> queryBuilder;

        [TestInitialize]
        public void Init()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new DapperBuilderModule(new AutofacBuilderConfiguration
            {
                DatabaseType = DatabaseType.PostgreSql,
                DbConnectionFactory = (ser) => new NpgsqlConnection("Host=localhost;")
            }));
            Container = containerBuilder.Build();
            queryBuilder = Container.Resolve<IQueryBuilder<UserMock>>();
        }

        [TestMethod]
        public void AllQuery()
        {
            var queryString = queryBuilder.GetQueryString();
            Assert.AreEqual(@"SELECT * FROM ""Users""".Trim(), queryString.Query.Trim());
        }

        [TestMethod]
        public void Top5Query()
        {
            var queryString = queryBuilder.Top(5).GetQueryString();
            Assert.AreEqual(
                string.Compare(
                    @"SELECT * FROM ""Users"" LIMIT 5",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }

        [TestMethod]
        public void Top5Skip5Query()
        {
            var queryString = queryBuilder.Top(5).Skip(5).GetQueryString();
            Assert.AreEqual(
                string.Compare(
                    @"SELECT * FROM ""Users"" LIMIT 5 OFFSET 5",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }

        [TestMethod]
        public void Skip5Query()
        {
            var queryString = queryBuilder.Skip(5).GetQueryString();
            Assert.AreEqual(
                string.Compare(
                    @"SELECT * FROM ""Users"" LIMIT -1 OFFSET 5",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }


        [TestMethod]
        public void AllQuerySingleColumnByExpression()
        {
            var queryString = queryBuilder.Columns(tc => tc.Id).GetQueryString();
            Assert.AreEqual(@"SELECT ""Users"".id FROM ""Users""".Trim().ToLower(), queryString.Query.Trim().ToLower());
        }

        [TestMethod]
        public void AllQuerySingleColumnByString()
        {
            var queryString = queryBuilder.Columns(nameof(UserMock.Id)).GetQueryString();
            Assert.AreEqual(@"SELECT ""Users"".Id FROM ""Users""".Trim().ToLower(), queryString.Query.Trim().ToLower());
        }

        [TestMethod]
        public void QueryWithEqualAlwaysTrue()
        {
            var queryString = queryBuilder.Where((c) => 1 == 1).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (:1 = 1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }

        [TestMethod]
        public void QueryWithStringConstant()
        {
            var queryString = queryBuilder.Where((c) => c.Email == "misha130@gmail.com").GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (""Users"".Email = :1)".Trim().ToLower(),
                    queryString.Query.Trim().ToLower(),
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("misha130@gmail.com", queryString.Parameters["1"]);
        }

        [TestMethod]
        public void QuerySingleColumnById()
        {
            var queryString = queryBuilder.Where(tc => tc.Id == 1).Columns(tc => tc.Id).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT ""Users"".Id FROM ""Users"" WHERE (""Users"".Id = :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("1", queryString.Parameters["1"].ToString());
        }

        [TestMethod]
        public void QueryWhereIsNull()
        {
            var queryString = queryBuilder.Columns(tc => tc.Id).Where(tc => tc.Email == null).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT ""Users"".Id FROM ""Users"" WHERE (""Users"".Email is null)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }


        [TestMethod, Ignore]
        public void QueryArrayIndexWhere()
        {
            var sampleText = "something_something".Split('_');
            var queryString = queryBuilder.Where(a => a.FirstName == sampleText[0] && a.LastName == sampleText[1])
                .GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE ((""Users"".FirstName) = :1 and (""Users"".LastName = :2))",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("something", queryString.Parameters["1"]);
            Assert.AreEqual("something", queryString.Parameters["2"]);
        }

        [TestMethod]
        public void QueryDoubleWhere()
        {
            var queryString = queryBuilder.Join<ContractMock>((u, c) => c.UserId == u.Id)
                .Join<AssetMock>((u, a) => a.UserId == u.Id)
                .Where<ContractMock, AssetMock>((c, a, u) => c.UserId == a.UserId).GetQueryString();
            Assert.AreEqual(
                string.Compare(
                    @"SELECT * FROM ""Users""
                          INNER JOIN ""Contracts"" ON (""Contracts"".UserId = ""Users"".Id)
                          INNER JOIN ""Assets"" ON (""Assets"".UserId = ""Users"".Id)
                          WHERE (""Contracts"".UserId = ""Assets"".UserId)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }


        [TestMethod]
        public void QueryToLower()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.ToLower() == "Misha").GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (LOWER(""Users"".FirstName) = :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("Misha", queryString.Parameters["1"]);
        }

        //[TestMethod, Ignore]
        //public void QueryToLowerWithBoolean()
        //{
        //    var mock = new UserMock { FirstName = "Misha" };
        //    var queryString = queryBuilder.Where(a => a.Independent && a.FirstName.ToLower() == mock.FirstName.ToLower()).GetQueryString();
        //    Assert.AreEqual(
        //                  string.Compare("SELECT * FROM [Users] WHERE (([Users].[Independent] = @1) AND ((LOWER([Users].[FirstName])) = LOWER(@2)))",
        //                  queryString.Query,
        //                  CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
        //                  , 0);
        //}

        [TestMethod]
        public void QueryToLowerInvariant()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.ToLowerInvariant() == "Misha").GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (LOWER(""Users"".FirstName) = :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("Misha", queryString.Parameters["1"]);
        }

        [TestMethod]
        public void QueryToUpperInvariant()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.ToUpperInvariant() == "Misha").GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (UPPER(""Users"".FirstName) = :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("Misha", queryString.Parameters["1"]);
        }

        [TestMethod]
        public void QueryToUpper()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.ToUpper() == "Misha").GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (UPPER(""Users"".FirstName) = :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("Misha", queryString.Parameters["1"]);
        }

        [TestMethod]
        public void QueryCount()
        {
            var queryString = queryBuilder.Count().Where(a => a.Independent).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT COUNT(*) FROM ""Users"" WHERE (""Users"".Independent = :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual(true, queryString.Parameters["1"]);
        }

        [TestMethod]
        public void QueryStartsWith()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.StartsWith("M")).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (""Users"".FirstName LIKE :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("M%", queryString.Parameters["1"]);
        }

        [TestMethod]
        public void QueryEndsWith()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.EndsWith("M")).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (""Users"".FirstName LIKE :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("%M", queryString.Parameters["1"]);
        }

        [TestMethod]
        public void QueryContains()
        {
            var queryString = queryBuilder.Where(a => a.FirstName.Contains("M")).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (""Users"".FirstName LIKE :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("%M%", queryString.Parameters["1"]);
        }

        //[TestMethod, Description("lmao it doesn't actually work, you have to do == false/true")]
        //[Ignore]
        //public void QueryBoolean()
        //{
        //    var queryString = queryBuilder.Where(a => a.Independent).GetQueryString();
        //    Assert.AreEqual(
        //                  string.Compare("SELECT * FROM [Users] WHERE ([Users].[Independent] = @1)",
        //                  queryString.Query,
        //                  CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
        //                  , 0);
        //}

        //[TestMethod(), Description("lmao it doesn't actually work, you have to do == false/true")]
        //[Ignore]
        //public void QueryBooleanFalse()
        //{
        //    var queryString = queryBuilder.Where(a => !a.Independent).GetQueryString();
        //    Assert.AreEqual(
        //                  string.Compare("SELECT * FROM [Users] WHERE ([Users].[Independent] = @1)",
        //                  queryString.Query,
        //                  CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
        //                  , 0);
        //}

        [TestMethod()]
        public void QueryBooleanWithEqual()
        {
            var queryString = queryBuilder.Where(a => a.Independent == true).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (""Users"".Independent = :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual(true, queryString.Parameters["1"]);
        }

        [TestMethod()]
        public void QueryBooleanFalseWithEqual()
        {
            var queryString = queryBuilder.Where(a => a.Independent == false).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (""Users"".Independent = :1)",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual(false, queryString.Parameters["1"]);
        }

        [TestMethod]
        public void QueryListContains()
        {
            var ids = new List<long> {1, 2, 3};
            var queryString = queryBuilder.Where(a => ids.Contains(a.Id)).GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT * FROM ""Users"" WHERE (""Users"".Id IN (:1,:2,:3))",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            foreach (var id in ids)
            {
                Assert.AreEqual(id, queryString.Parameters[id.ToString()]);
            }
            Assert.AreEqual(1l, queryString.Parameters["1"]); ;
            Assert.AreEqual(2l, queryString.Parameters["2"]);
            Assert.AreEqual(3l, queryString.Parameters["3"]);
        }

        [TestMethod]
        public void UpdateMultipleUnsorted()
        {
            var queryString = queryBuilder.Columns(
                    nameof(UserMock.Email),
                    nameof(UserMock.Independent),
                    nameof(UserMock.FirstName),
                    nameof(UserMock.LastName))
                .Where(user => user.Id == 1).GetUpdateString(
                    new UserMock
                    {
                        FirstName = "Misha",
                        LastName = "Trotsky",
                        Independent = true,
                        Email = "Misha130@gmail.com"
                    });
            Assert.AreEqual(
                string.Compare(
                    @"UPDATE ""Users"" SET Email = :2, Independent = :3, FirstName = :4, LastName = :5
                    WHERE (""Users"".Id = :1)
                    RETURNING Id",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.IsTrue((long) queryString.Parameters["1"] == 1);
            Assert.IsTrue((string) queryString.Parameters["2"] == "Misha130@gmail.com");
            Assert.IsTrue((bool) queryString.Parameters["3"] == true);
        }

        [TestMethod]
        public void InsertWithEnum()
        {
            IQueryBuilder<Component> queryBuilderComp = Resolve<IQueryBuilder<Component>>();
            var guid = Guid.NewGuid().ToString();
            var queryString = queryBuilderComp.GetInsertString(new Component
            {
                ComponentType = ComponentType.Complicated,
                Name = "Component Name",
                Url = guid
            });
            Assert.AreEqual(
                string.Compare(
                    @"INSERT INTO ""Components"" (ComponentType, Name, Url) VALUES (:1, :2, :3)
                    RETURNING  Id",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.IsTrue((ComponentType) queryString.Parameters["1"] == ComponentType.Complicated);
            Assert.IsTrue((string) queryString.Parameters["2"] == "Component Name");
            Assert.IsTrue((string) queryString.Parameters["3"] == guid);
        }


        [TestMethod]
        public void QueryWithUnderscoreToLower()
        {
            var testName = "TEST_TEST";
            var queryString = queryBuilder.Where(a => (a.FirstName + "_" + a.LastName).ToLower() == testName.ToLower())
                .GetQueryString();
            Assert.AreEqual(
                string.Compare(
                    @"SELECT * FROM ""Users"" WHERE (LOWER(((""Users"".FirstName || :1) || ""Users"".LastName)) = LOWER(:2))",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
            Assert.AreEqual("_", queryString.Parameters["1"]);
            Assert.AreEqual(testName, queryString.Parameters["2"]);
        }

        [TestMethod]
        public void UpdateWithExcludeColumns()
        {
            var userMock = new UserMock();
            var queryString = queryBuilder.ExcludeColumns(user => new
                {
                    user.PasswordHash, user.Assets, user.Contracts, user.FirstName, user.LastName, user.Independent,
                    user.Picture, user.CreatedDate
                })
                .GetUpdateString(userMock);
            Assert.AreEqual(
                string.Compare(@"UPDATE ""Users"" SET Email = :1 RETURNING Id",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }

        [TestMethod]
        public void InsertWithExcludeColumns()
        {
            var userMock = new UserMock();
            var queryString = queryBuilder.ExcludeColumns(user => new
                {
                    user.PasswordHash, user.Assets, user.Contracts, user.FirstName, user.LastName, user.Independent,
                    user.Picture, user.CreatedDate
                })
                .GetInsertString(userMock);
            Assert.AreEqual(
                string.Compare(@"INSERT INTO ""USERS"" (Email) VALUES(:1) RETURNING Id",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }

        [TestMethod]
        public void SelectWithColumnsAndExclude()
        {
            var queryString = queryBuilder.Columns(c => c.Email).ExcludeColumns(user => new
                {
                    user.PasswordHash, user.Assets, user.Contracts, user.FirstName, user.LastName, user.Independent,
                    user.Picture
                })
                .GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT ""Users"".Email From ""Users""",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }

        [TestMethod]
        public void SelectWithExclude()
        {
            var queryString = queryBuilder
                .ExcludeColumns(user => new
                {
                    user.PasswordHash,
                    user.Assets,
                    user.Contracts,
                    user.FirstName,
                    user.LastName,
                    user.Independent,
                    user.Picture,
                    user.CreatedDate,
                    user.Id
                })
                .GetQueryString();
            Assert.AreEqual(
                string.Compare(@"SELECT ""Users"".Email From ""Users""",
                    queryString.Query,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols)
                , 0);
        }
    }
}