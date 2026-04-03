namespace MentorsAndStudents.Responses
{
    public class AssignUserToSchoolActionResponse
    {
        public int ManagerUserId { get; set; }
        public int AddedUserId { get; set; }
        public int SchoolId { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
