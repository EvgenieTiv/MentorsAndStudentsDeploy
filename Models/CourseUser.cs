using Microsoft.EntityFrameworkCore;

namespace MentorsAndStudents
{
    public class CourseUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
