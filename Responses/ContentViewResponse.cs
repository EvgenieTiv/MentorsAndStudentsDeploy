namespace MentorsAndStudents
{
    public class ContentViewResponse
    {
        public List<TaskView> TasksViews { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class TaskView
    {
        public int Id { get; set; }
        public string MentorFullName { get; set; }
        public string TopicName { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string FullText { get; set; }
        public string FileName { get; set; }
        public bool SolutionUpdateAllowed { get; set; }
        public bool UpdateCreatedDate { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public ContentType ContentType { get; set; }
        public DateTime LastAllowedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
