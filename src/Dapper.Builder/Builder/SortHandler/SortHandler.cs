using System;
using System.Linq;
using FastMember;

namespace Dapper.Builder.Services
{
    public class SortHandler : ISortHandler
    {
        private readonly INamingStrategyService namingStrategy;
        public SortHandler(INamingStrategyService namingStrategy)
        {
            this.namingStrategy = namingStrategy;
        }
        public string Produce<TEntity>(SortColumn sort, string alias = null) where TEntity : new()
        {
            // have to test that the field exists on the table
            if (!Validate<TEntity>(sort.Field))
            {
                throw new Exception("Invalid Column in Sort");
            }
            return $"{alias ?? namingStrategy.GetColumnName<TEntity>(sort.Field)} {sort.Dir.ToString()}";
        }

        private bool Validate<T>(string property)
        {
            var accessor = TypeAccessor.Create(typeof(T));
            var members = accessor.GetMembers();
            return members.Any(m => m.Name.ToLower() == property.ToLower());
        }


    }
}
