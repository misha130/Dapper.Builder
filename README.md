# Dapper.Builder

The library offers a fluent builder way to create queries with Dapper.
This includes joins, sub queries with multiple database support (SQL, Postgre, Snowflake, Sqlite) 

[![NuGet](https://img.shields.io/nuget/v/Dapper.Builder.Autofac.svg)](https://www.nuget.org/packages/Dapper.Builder.Autofac)
[![NuGet](https://img.shields.io/nuget/v/Dapper.Builder.Core.svg)](https://www.nuget.org/packages/Dapper.Builder.Core)

[![Build Status](https://emdanet.visualstudio.com/Dapper.Builder/_apis/build/status/misha130.Dapper.Builder?branchName=master)](https://emdanet.visualstudio.com/Dapper.Builder/_build/latest?definitionId=4&branchName=master)
## Installation

Autofac 

    Install-Package Dapper.Builder.Autofac

Core 

    dotnet add package Dapper.Builder.Core

    
        
### .Net Core Setup


         services.AddDapperBuilder(
           new CoreBuilderConfiguration
            {
              DatabaseType = DatabaseType.SQL,
              DbConnectionFactory = (ser) => new SqlConnection("server=(local)")
            });
            

### Autofac Setup

         builder.RegisterModule(new DapperBuilderModule(new AutofacBuilderConfiguration() {
             DatabaseType = DatabaseType.SQL,
             DbConnectionFactory = (ser) => new SqlConnection("server=(local)")
        }));

        
### Usage 

#### Injection

    public class SomeRepository : ISomeRepository {

      private readonly IQueryBuilder<SomeClass> _query;
      public SomeRepository (IQueryBuilder<SomeClass> query) { _query = query; }


#### Simple Query
      public async Task<IEnumerable<SomeClass>> GetSomeSimpleStuff () {
          return await _query.Columns (
                  nameof (SomeClass.SomeColumnId),
                  nameof (SomeClass.AnotherColumn))
              .Where (sc => sc.Date < DateTime.Now.AddDays (-5))
              .SortDescending (sc => sc.CreatedDate).ExecuteAsync ();
      }
#### Query with join and sub query on itself
      public async Task<IEnumerable<SomeClass>> GetSomeComplexStuff () {
          return await _query.
              .SubQuery<SomeOtherClass> (q =>
                  q.Columns (nameof (SomeOtherClass.Id))
                  .Alias ("soc")
                  .Top (1)
                  .ParentAlias<SomeClass> ("sc")
                  .Where<SomeClass> ((sc, soc) => sc.SomeId == soc.SomeOtherId)
                  , "SubQueryAlias1")
              .SubQuery<SomeOtherClass> (q =>
                  q.Top (1)
                  .Columns (nameof (SomeOtherClass.Amount))
                  .Alias ("soc")
                  .ParentAlias<SomeClass> ("sc")
                   , "SubQueryAlias2")
              .Alias ("sc")
              .Join<SomeOtherOtherClass> (((sc, sooc) => sooc.SomeClassId == sc.Id))
              .GroupBy ((sc) => sc.SomeId).ExecuteAsync();

      }
      
#### Why a fluent builder?

The same reason why IQueryable exists. So you could pass this along to other methods, add whatever you want to it and then execute when you are finished.

        public async Task UpdateAdminData(IUser user)
            queryBuilder.Top(20);
                if(user is IAdmin){
                    ApplyAdminPipes(queryBuilder); 
            } else {
                ApplyUserPipes(queryBuilder);
            }
            await queryBuilder.ExecuteUpdateAsync(user);
        }
    
#### Road Map

* Better Alias Support
* More Database Support
	* Dapper has no out of the box support for Snowflake and their driver has no support for Dapper.
* Aggregations - ie. sum, max, min, etc.
* More methods to implement.

### Contributors

* [Misha Tarnortusky](https://github.com/misha130)
* [Avi Siboni](https://github.com/avisiboni)


### Contribute

Contributions to the package are always welcome!

* Report any bugs or issues you find on the [issue tracker](https://github.com/misha130/Dapper.Builder/issues).
* You can grab the source code at the package's [git repository](https://github.com/misha130/Dapper.Builder).

### License

All contents of this package are licensed under the [MIT license](https://opensource.org/licenses/MIT).
