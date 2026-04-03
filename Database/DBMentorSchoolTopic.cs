using System.ComponentModel.DataAnnotations.Schema;

namespace MentorsAndStudents
{
    [Table("MentorsSchoolsTopics")]
    public class DBMentorSchoolTopic
    {
        public int Id { get; set; }
        public int MentorId { get; set; }
        public int SchoolId { get; set; }
        public int TopicId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
