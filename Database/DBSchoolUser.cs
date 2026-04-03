using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorsAndStudents
{
    [Table("SchoolUsers")]
    public class DBSchoolUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SchoolId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
