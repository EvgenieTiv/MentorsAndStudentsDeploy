using MentorsAndStudents.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MentorsAndStudents
{
    [Table("Messages")]
    public class DBMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("MessageId")]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public DBUser User { get; set; }

        public int? ContentId { get; set; }

        [ForeignKey("ContentId")]
        public DBContent? Content { get; set; }

        public int? SolutionId { get; set; }

        [ForeignKey("SolutionId")]
        public DBSolution? Solution { get; set; }

        public string? Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
