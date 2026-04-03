using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public interface ISolutionsService
    {
        Task<ActionResult<SolutionsViewResponse>> ViewSolutions(SolutionsViewRequest view);
        Task<ActionResult<SolutionActionResponse>> UploadSolution(SolutionRequest solution);
        Task<ActionResult<SolutionActionResponse>> UpdateSolution(SolutionRequest solution);
        Task<FileStreamResult> DownloadSolution(SolutionRequest solution);
        Task<ActionResult<SolutionActionResponse>> DeleteSolution(SolutionRequest solution);
        Task<ActionResult<SolutionSetGradeResponse>> SetGradeSolution(SolutionSetGradeRequest solutionGradeModel);
    }
}
