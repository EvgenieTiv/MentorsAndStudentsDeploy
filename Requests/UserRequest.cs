namespace MentorsAndStudents
{
    public class UserRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int UserTypeId { get; set; }
        public string? AdminCode { get; set; }
    }
}
