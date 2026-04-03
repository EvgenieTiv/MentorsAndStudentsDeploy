namespace MentorsAndStudents.Responses
{
    public class SchoolOrCourseUsersByTypeAndCoursesCountResponse
    {
        public int SchoolManagersCount { get; set; }
        public int MentorsCount { get; set; }
        public int StudentsCount { get; set; }
        public int CoursesCount { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
