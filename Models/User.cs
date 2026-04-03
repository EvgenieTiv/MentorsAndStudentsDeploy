namespace MentorsAndStudents
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserTypeId { get; set; }
        public UserType UserType { get; set; }        
    }

    public enum UserType
    {
        Mentor = 0,
        Student = 1,
        Admin = 2,
        SchoolManager = 3,
    }
}
