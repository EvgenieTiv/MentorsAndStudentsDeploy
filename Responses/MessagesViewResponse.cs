namespace MentorsAndStudents
{ 
    public class MessagesViewResponse
    {
        public List<MessageView> MessagesViews { get; set; }
        public int UserId { get; set; }
        public int ContentId { get; set; }
        public int SolutionId { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class MessageView
    {
        public int Id { get; set; }
        public string UserFullName { get; set; }
        public int UserTypeId { get; set; }
        public string? Text { get; set; }
    }
}
