
namespace Dapper.Builder.Builder.SortHandler
{
    /// <summary>
    /// Defines a sorting query
    /// </summary>
    public class SortColumn
    {
        public string Field { get; set; }
        public SortType Dir { get; set; }
    }

    /// <summary>
    /// Types of sort, either descending or ascending
    /// </summary>
    public enum SortType
    {
        Asc,
        Desc
    }
}
