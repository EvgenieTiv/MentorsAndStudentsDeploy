using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public class SolutionRequest
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? FullText { get; set; }
        public int? ContentId { get; set; }
        public int? StudentId { get; set; }

        [FromForm]
        public IFormFile? File { get; set; }

        public bool? UpdateCreatedDate { get; set; } = false;
    }
}
