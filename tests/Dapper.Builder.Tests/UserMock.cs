using System.ComponentModel.DataAnnotations.Schema;

 
namespace BR.POCO.DB
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
        public long CompanyId { get; set; }
        public bool Active { get; set; }
    }
}
