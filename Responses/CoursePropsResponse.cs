namespace MentorsAndStudents
{
    public class CoursePropsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TopicName { get; set; }
        public int SchoolClass { get; set; }
        public int StudentsCount { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<User> Mentors { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
