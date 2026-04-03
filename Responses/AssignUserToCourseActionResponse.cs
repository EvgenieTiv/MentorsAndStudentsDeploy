namespace MentorsAndStudents
{
    public class AssignUserToCourseActionResponse
    {
        public int ManagerUserId { get; set; }
        public int AddedUserId { get; set; }
        public int CourseId { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
