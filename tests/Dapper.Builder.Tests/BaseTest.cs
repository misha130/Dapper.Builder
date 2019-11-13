using Microsoft.AspNetCore;
using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Dapper.Builder.Core;
using Microsoft.Data.SqlClient;
namespace Dapper.Builder.Tests
{
    public class BaseTest
    {
        protected IContainer Container;
        protected T Resolve<T>()
        {
            return Container.Resolve<T>();
        }


    }

}
