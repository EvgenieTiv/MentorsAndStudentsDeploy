namespace MentorsAndStudents
{
    public class LoginActionResponse
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
        public int UserTypeId { get; set; }
        public int UserId { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }        
    }
}
