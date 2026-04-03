using MentorsAndStudents.Common;
using System.Text.Json.Serialization;

namespace MentorsAndStudents
{
    public class Solution
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string FullText { get; set; }
        public Content Content { get; set; }
        public User Student { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime CreatedDate { get; set; }


        public string Grade { get; set; }
        public bool IsClosed { get; set; } = false;

        public DateTime ModifiedDate { get; set; }
    }
}
