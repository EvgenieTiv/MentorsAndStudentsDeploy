namespace MentorsAndStudents
{
    public class ContentsViewRequest
    {
        public int MentorId { get; set; }
        public int CourseId { get; set; }
        public bool IsViewExpired { get; set; } = false;
    }
}
