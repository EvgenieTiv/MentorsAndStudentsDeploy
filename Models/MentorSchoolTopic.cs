namespace MentorsAndStudents.Models
{
    public class MentorSchoolTopic
    {
        public int Id { get; set; }
        public int MentorId { get; set; }
        public int SchoolId { get; set; }
        public int TopicId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
