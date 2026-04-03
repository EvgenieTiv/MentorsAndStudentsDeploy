using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorsAndStudents
{
    [Table("Topics")]
    public class DBTopic
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("TopicId")]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
