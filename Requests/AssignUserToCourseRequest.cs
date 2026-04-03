namespace MentorsAndStudents
{
    public class AssignUserToCourseRequest
    {
        public int ManagerUserId { get; set; }
        public int AddedUserId { get; set; }
        public int CourseId { get; set; }
        public int SchoolId { get; set; }
        public bool IsAssign { get; set; }
    }
}
