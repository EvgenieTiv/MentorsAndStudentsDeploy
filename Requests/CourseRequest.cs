namespace MentorsAndStudents
{
    public class CourseRequest
    {
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int SchoolId { get; set; }
        public int TopicId { get; set; }
        public int SchoolClass { get; set; }
        public string SchoolClassLetter { get; set; }
        public int? HomeroomTeacherId { get; set; }
        public string? Name { get; set; }
    }
}
