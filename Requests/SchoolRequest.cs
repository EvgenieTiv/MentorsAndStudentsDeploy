namespace MentorsAndStudents.Requests
{
    public class SchoolRequest
    {
        public int UserId { get; set; }
        public int SchoolId { get; set; }
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public GradingSystem? GradingSystem { get; set; }
    }
}
