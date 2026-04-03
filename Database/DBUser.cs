using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorsAndStudents
{
    [Table("Users")]
    public class DBUser
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("UserId")]
        public int Id { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int UserTypeId { get; set; }
        public string? RefreshToken { get; set; }
    }
}
