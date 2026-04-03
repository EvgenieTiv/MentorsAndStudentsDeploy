namespace MentorsAndStudents
{
    public class SolutionsViewRequest
    {
        public int? Id { get; set; }
        public int StudentId { get; set; }
        public int? ContentId { get; set; }
        public bool IsViewExpired { get; set; } = false;
    }
}
