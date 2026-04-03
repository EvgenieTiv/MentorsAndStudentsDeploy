namespace MentorsAndStudents
{
    public class MessageRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ContentId { get; set; }
        public int SolutionId { get; set; }
        public string? Text { get; set; }
    }
}
