using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorsAndStudents
{
    [Table("Courses")]
    public class DBCourse
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("CourseId")]
        public int Id { get; set; }
        public string Name { get; set; }

        public int SchoolId { get; set; }

        [ForeignKey("SchoolId")]
        public DBSchool School { get; set; }

        public int TopicId { get; set; }

        [ForeignKey("TopicId")]
        public DBTopic Topic { get; set; }

        public int SchoolClass { get; set; }
        public string SchoolClassLetter { get; set; }

        [ForeignKey("UserId")]
        public int HomeroomTeacherId { get; set; }
        public bool IsClass { get; set; } = false;

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
