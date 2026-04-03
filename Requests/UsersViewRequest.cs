namespace MentorsAndStudents
{
    public class UsersViewRequest
    {
        public int AdminUserId { get; set; }
        public int? SchoolId { get; set; }
        public int? CourseId { get; set; }
        public bool ViewAssigned { get; set; }
    }
}
