using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public class ContentRequest
    {
        public int? Id { get; set; }
        public int? ContentType { get; set; }
        public string? Name { get; set; }
        public string? Summary { get; set; }
        public string? FullText { get; set; }
        public int? MentorId { get; set; }
        public int? CourseId { get; set; }

        [FromForm]
        public IFormFile? File { get; set; }
        public bool? SolutionUpdateAllowed { get; set; }
        public bool? UpdateCreatedDate { get; set; } = false;
        public DateTime? LastAllowedDate { get; set; }
    }
}
