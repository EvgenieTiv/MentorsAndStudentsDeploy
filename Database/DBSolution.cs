using MentorsAndStudents.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MentorsAndStudents
{
    [Table("Solutions")]
    public class DBSolution
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("SolutionId")]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullText { get; set; }
        public string FileName { get; set; }

        public int ContentId { get; set; }

        [ForeignKey("ContentId")]
        public DBContent Content { get; set; }


        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public DBUser Student { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime CreatedDate { get; set; }

        public string Grade { get; set; }
        public bool IsClosed { get; set; } = false;

        public DateTime? ModifiedDate { get; set; }
    }
}
