using System.ComponentModel.DataAnnotations.Schema;

namespace MentorsAndStudents
{
    [Table("CourseUsers")]
    public class DBCourseUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
