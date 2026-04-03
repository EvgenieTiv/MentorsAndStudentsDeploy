namespace MentorsAndStudents
{
    public class SolutionsViewResponse
    {
        public List<SolutionView> SolutionsViews { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SolutionView
    {
        public int Id { get; set; }
        public string StudentFullName { get; set; }        
        public string Name { get; set; }
        public string FullText { get; set; }
        public string MentorFullName { get; set; }
        public string TaskName { get; set; }
        public string CourseName { get; set; }
        public string FileName { get; set; }
        public bool SolutionUpdateAllowed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastAllowedDate { get; set; }        
        public string Grade { get; set; }
        public bool IsClosed { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}
