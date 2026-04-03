namespace MentorsAndStudents
{
    public class UsersViewResponse
    {
        public List<User> UsersViews { get; set; }
        public int? SchoolId { get; set; }
        public int? CourseId { get; set; }
        public bool? IsShowingAssigned { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
