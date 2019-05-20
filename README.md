# Dapper.Builder

The library offers a fluent builder way to create queries with Dapper.
This includes joins, sub queries with multiple database support (SQL, Postgre, Snowflake, Sqlite) 

## Installation

    Currently none available, maybe in the future
    
        
### .Net Core Setup


         services.AddDapperBuilder(
           new BuilderConfiguration
            {
              DatabaseType = DatabaseType.SQL,
              DbConnectionFactory = (ser) => new SqlConnection("server=(local)")
            });
            
### Usage 

#### Injection

    public class SomeRepository : ISomeRepository {

      private readonly IQueryBuilder<SomeClass> _query;
      public SomeRepository (IQueryBuilder<SomeClass> query) { _query = query; }


#### Simple Query
      public async Task<IEnumerable<SomeClass>> GetSomeSimpleStuff () {
          return _query.Columns (
                  nameof (SomeClass.SomeColumnId),
                  nameof (SomeClass.AnotherColumn))
              .Where (sc => sc.Date < DateTime.Now.AddDays (-5))
              .SortDescending (sc => sc.CreatedDate).ExecuteAsync ();
      }
#### Query with join and sub query on itself
      public async Task<IEnumerable<SomeClass>> GetSomeComplexStuff () {
          _query.
              .SubQuery<SomeOtherClass> (q =>
                  q.Columns (nameof (SomeOtherClass.Id))
                  .Alias ("soc")
                  .Top (1)
                  .ParentAlias<SomeClass> ("sc")
                  .Where<SomeClass> ((sc, soc) => sc.SomeId == soc.SomeOtherId)
                  , "SubQueryAlias1")
              .SubQuery<FuelPricing> (q =>
                  q.Top (1)
                  .Columns (nameof (SomeOtherClass.Amount))
                  .Alias ("soc")
                  .ParentAlias<SomeClass> ("sc")
                   , "SubQueryAlias2")
              .Alias ("fp")
              .Join<SomeOtherOtherClass> (((sc, sooc) => sooc.SomeClassId == sc.Id))
              .GroupBy ((sc) => sc.SomeId).ExecuteAsync();

      }
    
#### Road Map

* Better Alias Support
* More Database Support
	* Dapper has no out of the box support for Snowflake and their driver has no support for Dapper.
* Aggregations - ie. sum, max, min, etc.
* More methods to implement.
* .Net Framework 4.6.2 support without any IoC library requirements.

### Contributors

* [Misha Tarnortusky](https://github.com/misha130)


### Contribute

Contributions to the package are always welcome!

* Report any bugs or issues you find on the [issue tracker](https://github.com/misha130/Dapper.Builder/issues).
* You can grab the source code at the package's [git repository](https://github.com/misha130/Dapper.Builder).

### License

All contents of this package are licensed under the [MIT license](https://opensource.org/licenses/MIT).
