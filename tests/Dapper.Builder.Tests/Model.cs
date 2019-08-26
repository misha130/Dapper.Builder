using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dapper.Builder.Tests
{
    [Table("Users")]
    public class UserMock
    {
        public long Id { get; set; }
        public string Picture { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public IEnumerable<AssetMock> Assets { get; set; }
        public IEnumerable<ContractMock> Contracts { get; set; }
    }

    [Table("Assets")]
    public class AssetMock
    {
        public long Id { get; set; }
        public AssetType Type { get; set; }
        public decimal Name { get; set; }
        public long UserId { get; set; }
    }

    public enum AssetType
    {
        House,
        Car,
        Sofa,
        Cat
    }

    [Table("Contracts")]
    public class ContractMock
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public System.DateTime ExpirationDate { get; set; }
        public long UserId { get; set; }
    }
}
