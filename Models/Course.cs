namespace MentorsAndStudents
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<User>? AssociatedMentors { get; set; }
        public School School { get; set; }
        public Topic Topic { get; set; }
        public int SchoolClass { get; set; }
        public string SchoolClassLetter { get; set; }
        public User HomeroomTeacher { get; set; }
        public bool IsClass { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
