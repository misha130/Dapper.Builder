using Dapper.Builder.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Builder.Tests.Services
{
    [TestClass]
    public class AggregateServices : BaseTest
    {
        /// <summary>
        /// Tests whether aggregate services gets all the depenedencies
        /// </summary>
        [TestMethod]
        public void AllServices()
        {
            var dependencies = Resolve<IQueryBuilderDependencies<TestClass>>();
            var props = dependencies.GetType().GetProperties();
            Assert.IsTrue(props.All(prop => prop.GetValue(dependencies) != null));
        }
    }
}
