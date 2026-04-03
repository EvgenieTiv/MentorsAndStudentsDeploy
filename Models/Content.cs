using MentorsAndStudents.Common;
using System.Text.Json.Serialization;

namespace MentorsAndStudents
{
    public class Content
    {
        public int Id { get; set; }
        public ContentType ContentType { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }       
        public string Summary { get; set; }
        public string FullText { get; set; }
        public User Mentor { get; set; }
        public Course Course { get; set; }
        public bool SolutionUpdateAllowed { get; set; }
        public bool UpdateCreatedDate { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime CreatedDate { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime LastAllowedDate { get; set; }

        public DateTime ModifiedDate { get; set; }
    }

    public enum ContentType
    {
        Task = 0,
        Material = 1,
        Lesson = 2
    }
}
