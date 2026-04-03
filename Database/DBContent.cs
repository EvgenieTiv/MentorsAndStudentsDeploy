using MentorsAndStudents.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MentorsAndStudents
{
    [Table("Content")]
    public class DBContent
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("ContentId")]
        public int Id { get; set; }

        [Required]
        public ContentType ContentType { get; set; }

        public string FileName { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string FullText { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public DBUser Mentor { get; set; }

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public DBCourse Course { get; set; }

        public bool SolutionUpdateAllowed { get; set; }
        public bool UpdateCreatedDate { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime? CreatedDate { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime LastAllowedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
